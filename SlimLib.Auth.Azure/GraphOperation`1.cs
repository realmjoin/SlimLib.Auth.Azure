using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SlimLib.Auth.Azure;

namespace SlimLib;

public class GraphOperation<T>(ISlimHttpClient client, IAzureTenant tenant, HttpMethod method, string requestUrl, InvokeRequestOptions? options, ReadOnlyMemory<byte> utf8Data, Func<JsonDocument, T> executeFunc) : GraphOperation(client, tenant, method, requestUrl, options, utf8Data)
{
    private T? result;

    public T? Result
    {
        get => Error is null ? result : throw Error;
        private set => result = value;
    }

    public new TaskAwaiter<T?> GetAwaiter() => ExecuteAsync().GetAwaiter();

    public new ConfiguredTaskAwaitable<T?> ConfigureAwait(bool continueOnCapturedContext) => ExecuteAsync().ConfigureAwait(continueOnCapturedContext);

    public new async Task<T?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        JsonDocument? doc;

        if (Method == HttpMethod.Get)
        {
            doc = await Client.GetAsync(Tenant, RequestUrl, Options, cancellationToken).ConfigureAwait(false);
        }
        else if (Method == HttpMethod.Post)
        {
            doc = await Client.PostAsync(Tenant, RequestUrl, Utf8Data, Options, cancellationToken).ConfigureAwait(false);
        }
        else if (Method == HttpMethod.Patch)
        {
            doc = await Client.PatchAsync(Tenant, RequestUrl, Utf8Data, Options, cancellationToken).ConfigureAwait(false);
        }
        else if (Method == HttpMethod.Delete)
        {
            await Client.DeleteAsync(Tenant, RequestUrl, Options, cancellationToken).ConfigureAwait(false);
            doc = null;
        }
        else
        {
            throw new NotSupportedException($"HTTP method {Method} is not supported for direct execution in this context.");
        }

        if (doc is null)
        {
            return default;
        }

        Result = executeFunc(doc);
        return Result;
    }

    public override void SetResultBatch(JsonElement jsonElement, JsonSerializerOptions? options = null)
    {
        var doc = CloneBatchBody(jsonElement);
        Result = executeFunc(doc);
    }
}