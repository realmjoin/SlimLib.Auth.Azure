using System.Text.Json.Serialization;

namespace SlimLib.Auth.Azure.Shared;

public class ODataResponse
{
    [JsonPropertyName("@odata.context")]
    public string ODataContext { get; set; } = null!;

    [JsonPropertyName("@odata.type")]
    public string? ODataType { get; set; }
}
