using System;
using System.Buffers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SlimLib.Auth.Azure;

namespace SlimLib;

public class GraphOperation(ISlimHttpClient client, IAzureTenant tenant, HttpMethod method, string requestUrl, InvokeRequestOptions? options, ReadOnlyMemory<byte> utf8Data)
{
    public ISlimHttpClient Client => client;
    public IAzureTenant Tenant => tenant;
    public HttpMethod Method => method;
    public string RequestUrl => requestUrl;
    public InvokeRequestOptions? Options => options;
    public ReadOnlyMemory<byte> Utf8Data => utf8Data;

    public string[]? DependsOn { get; set; }

    public Exception? Error { get; private set; }

    public TaskAwaiter GetAwaiter() => ExecuteAsync().GetAwaiter();

    public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => ExecuteAsync().ConfigureAwait(continueOnCapturedContext);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (Method == HttpMethod.Get)
        {
            using var doc = await client.GetAsync(Tenant, RequestUrl, Options, cancellationToken).ConfigureAwait(false);
        }
        else if (Method == HttpMethod.Post)
        {
            using var doc = await client.PostAsync(Tenant, RequestUrl, Utf8Data, Options, cancellationToken).ConfigureAwait(false);
        }
        else if (Method == HttpMethod.Patch)
        {
            using var doc = await client.PatchAsync(Tenant, RequestUrl, Utf8Data, Options, cancellationToken).ConfigureAwait(false);
        }
        else if (Method == HttpMethod.Delete)
        {
            await client.DeleteAsync(Tenant, RequestUrl, Options, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            throw new NotSupportedException($"HTTP method {Method} is not supported for direct execution in this context.");
        }
    }

    public virtual void SetResultBatch(JsonElement jsonElement, JsonSerializerOptions? options = null) { }

    public virtual void SetBatchError(Exception exception) => Error = exception;

    protected static JsonDocument CloneBatchBody(JsonElement jsonElement)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            jsonElement.WriteTo(writer);
        }

        return JsonDocument.Parse(buffer.WrittenMemory);
    }
}