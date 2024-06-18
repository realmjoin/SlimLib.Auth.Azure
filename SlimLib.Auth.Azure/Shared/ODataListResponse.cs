using System.Text.Json.Serialization;

namespace SlimLib.Auth.Azure.Shared;

public class ODataListResponse : ODataResponse
{
    [JsonPropertyName("@odata.count")]
    public int? ODataCount { get; set; }

    [JsonPropertyName("@odata.nextLink")]
    public string? ODataNextLink { get; set; }
}
