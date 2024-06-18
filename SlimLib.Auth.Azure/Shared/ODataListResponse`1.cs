using System.Text.Json.Serialization;

namespace SlimLib.Auth.Azure.Shared;

public class ODataListResponse<T> : ODataListResponse
{
    [JsonPropertyName("value")]
    public T[] ODataValues { get; set; } = null!;
}
