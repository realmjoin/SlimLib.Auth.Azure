using SlimLib.Auth.Azure;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SlimLib;

public interface ISlimHttpClient
{
    Task<JsonDocument?> GetAsync(IAzureTenant tenant, string requestUri, InvokeRequestOptions? options, CancellationToken cancellationToken);
    Task<JsonDocument?> PostAsync(IAzureTenant tenant, string requestUri, ReadOnlyMemory<byte> utf8Data, InvokeRequestOptions? options, CancellationToken cancellationToken);
    Task<JsonDocument?> PatchAsync(IAzureTenant tenant, string requestUri, ReadOnlyMemory<byte> utf8Data, InvokeRequestOptions? options, CancellationToken cancellationToken);
    Task DeleteAsync(IAzureTenant tenant, string requestUri, InvokeRequestOptions? options, CancellationToken cancellationToken);

    IAsyncEnumerable<JsonDocument> GetArrayAsync(IAzureTenant tenant, string requestUri, InvokeRequestOptions? options, CancellationToken cancellationToken);
}