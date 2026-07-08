using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SlimLib.Auth.Azure;

namespace SlimLib;

public class GraphArrayOperation<T>(ISlimHttpClient client, IAzureTenant tenant, HttpMethod method, string requestUrl, InvokeRequestOptions? options, ReadOnlyMemory<byte> utf8Data, Func<JsonDocument, T> executeFunc) : GraphOperation(client, tenant, method, requestUrl, options, utf8Data)
{
    private T? result;

    public T? Result
    {
        get => Error is null ? result : throw Error;
        private set => result = value;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => GetDataAsync(RequestUrl, cancellationToken);

    public async IAsyncEnumerator<T> GetDataAsync(string requestUrl, CancellationToken cancellationToken = default)
    {
        if (Method == HttpMethod.Get)
        {
            await foreach (var item in Client.GetArrayAsync(Tenant, requestUrl, Options, cancellationToken))
            {
                yield return executeFunc(item);
            }

            yield break;
        }
        else if (Method == HttpMethod.Post)
        {
            var doc = await Client.PostAsync(Tenant, requestUrl, Utf8Data, Options, cancellationToken).ConfigureAwait(false);
            if (doc is not null)
            {
                yield return executeFunc(doc);
            }

            yield break;
        }
        else if (Method == HttpMethod.Patch)
        {
            var doc = await Client.PatchAsync(Tenant, requestUrl, Utf8Data, Options, cancellationToken).ConfigureAwait(false);
            if (doc is not null)
            {
                yield return executeFunc(doc);
            }

            yield break;
        }
        else if (Method == HttpMethod.Delete)
        {
            await Client.DeleteAsync(Tenant, requestUrl, Options, cancellationToken).ConfigureAwait(false);
            yield break;
        }

        throw new NotSupportedException($"HTTP method {Method} is not supported for direct execution in this context.");
    }

    public async Task<T?> GetNextPageAsync(string? nextLink, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(nextLink)) return default;

        var operation = new GraphOperation<T>(Client, Tenant, Method, nextLink, Options, utf8Data: default, executeFunc);

        Result = await operation.ExecuteAsync(cancellationToken).ConfigureAwait(false);

        return Result;
    }

    public override void SetResultBatch(JsonElement jsonElement, JsonSerializerOptions? options = null)
    {
        var doc = CloneBatchBody(jsonElement);
        Result = executeFunc(doc);
    }
}