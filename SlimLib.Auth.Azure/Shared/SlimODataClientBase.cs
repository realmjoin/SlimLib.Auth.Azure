using Microsoft.Extensions.Logging;
using SlimLib.Auth.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace SlimLib;

public abstract class SlimODataClientBase : ISlimHttpClient
{
    private readonly IAuthenticationProvider authenticationProvider;
    private readonly HttpClient httpClient;
    private readonly ILogger logger;

    protected SlimODataClientBase(IAuthenticationProvider authenticationProvider, HttpClient httpClient, ILogger logger)
    {
        this.authenticationProvider = authenticationProvider;
        this.httpClient = httpClient;
        this.logger = logger;
    }

    protected HttpClient HttpClient => httpClient;

    protected abstract string Scope { get; }

    protected abstract SlimApiException CreateApiError(HttpStatusCode statusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, string errorCode, string errorMessage);

    protected async Task DeleteAsync(IAzureTenant tenant, string requestUri, InvokeRequestOptions? options, CancellationToken cancellationToken)
    {
        using var doc = await SendAsync(tenant, HttpMethod.Delete, requestUri, null, options, null, cancellationToken).ConfigureAwait(false);
    }

    protected Task<JsonDocument?> GetAsync(IAzureTenant tenant, string requestUri, InvokeRequestOptions? options, CancellationToken cancellationToken)
        => SendAsync(tenant, HttpMethod.Get, requestUri, null, options, null, cancellationToken);

    protected Task<JsonDocument?> PatchAsync(IAzureTenant tenant, string requestUri, ReadOnlyMemory<byte> utf8Data, InvokeRequestOptions? options, CancellationToken cancellationToken)
        => SendAsync(tenant, HttpMethod.Patch, requestUri, utf8Data, options, null, cancellationToken);

    protected Task<JsonDocument?> PostAsync(IAzureTenant tenant, string requestUri, ReadOnlyMemory<byte> utf8Data, InvokeRequestOptions? options, CancellationToken cancellationToken)
        => SendAsync(tenant, HttpMethod.Post, requestUri, utf8Data, options, null, cancellationToken);

    protected async IAsyncEnumerable<JsonDocument> GetArrayAsync(IAzureTenant tenant, string nextLink, InvokeRequestOptions? options, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string? link = nextLink;

        do
        {
            var doc = await GetAsync(tenant, link, options, cancellationToken).ConfigureAwait(false);

            if (doc is not null)
            {
                HandleNextLink(doc.RootElement, ref link);
                yield return doc;
            }

        } while (link != null);
    }

    public async Task BatchRequestAsync(IAzureTenant tenant, IList<GraphOperation> operations, CancellationToken cancellationToken)
    {
        var requests = new JsonArray();

        var payload = new JsonObject
        {
            ["requests"] = requests
        };

        var i = 0;

        foreach (var operation in operations)
        {
            var request = new JsonObject
            {
                ["id"] = i++.ToString(),
                ["method"] = operation.Method.ToString(),
                ["url"] = operation.RequestUrl,
            };

            if (operation.DependsOn is not null)
            {
                request["dependsOn"] = JsonSerializer.SerializeToNode(operation.DependsOn);
            }

            operation.Options?.ConfigureBatchRequest(request);

            requests.Add(request);
        }

        using var response = await PostAsync(tenant, "$batch", JsonSerializer.SerializeToUtf8Bytes(payload), options: null, cancellationToken) ?? throw new InvalidOperationException("Batch request failed.");

        if (response.RootElement.TryGetProperty("responses", out var responses) && responses is { ValueKind: JsonValueKind.Array })
        {
            for (var j = 0; j < requests.Count; j++)
            {
                var item = responses.EnumerateArray().FirstOrDefault(x => x.TryGetProperty("id", out var id) && id.GetString() == j.ToString());

                var error = CreateBatchError(item);

                if (error is not null)
                {
                    operations[j].SetBatchError(error);
                }
                else if (item.TryGetProperty("body", out var body))
                {
                    operations[j].SetResultBatch(body);
                }
            }

            return;
        }

        throw new InvalidOperationException("Batch request failed.");
    }

    protected async Task<JsonDocument?> SendAsync(IAzureTenant tenant, HttpMethod method, string requestUri, ReadOnlyMemory<byte>? utf8Data, InvokeRequestOptions? options, Func<HttpResponseMessage, Task>? httpResponseMessageCustomResponseHandler, CancellationToken cancellationToken)
    {
        using var response = await SendInternalAsync(tenant, method, requestUri, utf8Data, options, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
        {
            logger.LogInformation("Got no content for HTTP request to {requestUri}.", requestUri);
            if (httpResponseMessageCustomResponseHandler != null)
            {
                await httpResponseMessageCustomResponseHandler(response);
            }
            return null;
        }

        if (httpResponseMessageCustomResponseHandler != null)
        {
            await httpResponseMessageCustomResponseHandler(response);
        }

        using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var doc = await JsonSerializer.DeserializeAsync<JsonDocument>(content, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw CreateApiErrorFromDocument(response.StatusCode, response.Headers, doc);

        return doc;
    }

    protected async Task<HttpResponseMessage> SendInternalAsync(IAzureTenant tenant, HttpMethod method, string requestUri, ReadOnlyMemory<byte>? utf8Data, InvokeRequestOptions? options, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, requestUri);

        if (utf8Data != null)
        {
            request.Content = new ReadOnlyMemoryContent(utf8Data.Value)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") { CharSet = Encoding.UTF8.WebName } }
            };
        }

        await authenticationProvider.AuthenticateRequestAsync(tenant, Scope, request).ConfigureAwait(false);

        options?.ConfigureHttpRequest(request);

        return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    protected static void HandleNextLink(JsonElement root, ref string? nextLink)
    {
        if (root.TryGetProperty("@odata.nextLink", out var el))
        {
            nextLink = el.GetString();
        }
        else
        {
            nextLink = null;
        }
    }

    private SlimApiException CreateApiErrorFromDocument(HttpStatusCode statusCode, HttpResponseHeaders headers, JsonDocument? root)
    {
        try
        {
            if (root?.RootElement.TryGetProperty("error", out var error) == true)
            {
                return CreateApiError(statusCode, headers, error.GetProperty("code").GetString() ?? "", error.GetProperty("message").GetString() ?? "");
            }
        }
        catch
        {
        }

        return CreateApiError(0, [], "Unkown error", "");
    }

    private SlimApiException? CreateBatchError(JsonElement item)
    {
        if (item is not { ValueKind: JsonValueKind.Object })
            return null;

        if (item.TryGetProperty("status", out var status) && status is { ValueKind: JsonValueKind.Number })
        {
            var http = (HttpStatusCode)status.GetInt32();

            if (http == HttpStatusCode.OK)
                return null;

            if (item.TryGetProperty("body", out var body) && body is { ValueKind: JsonValueKind.Object })
            {
                if (body.TryGetProperty("error", out var error))
                {
                    var headers = new List<KeyValuePair<string, IEnumerable<string>>>();

                    if (item.TryGetProperty("headers", out var headersElement) && headersElement is { ValueKind: JsonValueKind.Object })
                    {
                        foreach (var header in headersElement.EnumerateObject())
                        {
                            var values = new List<string>();

                            if (header.Value.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var value in header.Value.EnumerateArray())
                                {
                                    if (value.ValueKind == JsonValueKind.String)
                                    {
                                        var str = value.GetString();

                                        if (str is not null)
                                            values.Add(str);
                                    }
                                }
                            }
                            else if (header.Value.ValueKind == JsonValueKind.String)
                            {
                                var str = header.Value.GetString();

                                if (str is not null)
                                    values.Add(str);
                            }

                            if (values.Count > 0)
                                headers.Add(new(header.Name, values));
                        }
                    }

                    if (error is { ValueKind: JsonValueKind.Object })
                        return CreateApiError(http, headers, error.GetProperty("code").GetString() ?? "", error.GetProperty("message").GetString() ?? "");
                    else
                        return CreateApiError(http, headers, "Unkown error", "");
                }
            }
        }

        return null;
    }

    Task<JsonDocument?> ISlimHttpClient.GetAsync(IAzureTenant tenant, string requestUri, InvokeRequestOptions? options, CancellationToken cancellationToken)
        => GetAsync(tenant, requestUri, options, cancellationToken);

    Task<JsonDocument?> ISlimHttpClient.PostAsync(IAzureTenant tenant, string requestUri, ReadOnlyMemory<byte> utf8Data, InvokeRequestOptions? options, CancellationToken cancellationToken)
        => PostAsync(tenant, requestUri, utf8Data, options, cancellationToken);

    Task<JsonDocument?> ISlimHttpClient.PatchAsync(IAzureTenant tenant, string requestUri, ReadOnlyMemory<byte> utf8Data, InvokeRequestOptions? options, CancellationToken cancellationToken)
        => PatchAsync(tenant, requestUri, utf8Data, options, cancellationToken);

    Task ISlimHttpClient.DeleteAsync(IAzureTenant tenant, string requestUri, InvokeRequestOptions? options, CancellationToken cancellationToken)
        => DeleteAsync(tenant, requestUri, options, cancellationToken);

    IAsyncEnumerable<JsonDocument> ISlimHttpClient.GetArrayAsync(IAzureTenant tenant, string requestUri, InvokeRequestOptions? options, CancellationToken cancellationToken)
        => GetArrayAsync(tenant, requestUri, options, cancellationToken);
}