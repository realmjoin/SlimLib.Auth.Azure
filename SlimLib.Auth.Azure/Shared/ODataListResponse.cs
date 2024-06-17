using System.Text.Json.Serialization;

namespace SlimLib.Auth.Azure.Shared;

public class ODataListResponse<T> : ODataResponse
{
    [JsonPropertyName("@odata.count")]
    public int? ODataCount { get; set; }

    [JsonPropertyName("@odata.nextLink")]
    public string? ODataNextLink { get; set; }

    [JsonPropertyName("value")]
    public T[] ODataValues { get; set; } = null!;
}
