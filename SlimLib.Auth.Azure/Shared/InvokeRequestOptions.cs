using System.Text.Json.Nodes;

namespace SlimLib;

public class InvokeRequestOptions
{
    public ConsistencyLevel ConsistencyLevel { get; set; }
    public string? UserAgent { get; set; }
    public int? MaxPageSize { get; set; }
    public ReturnOptions Return { get; set; }

    public virtual JsonObject ToJson()
    {
        var json = new JsonObject();

        if (ConsistencyLevel == ConsistencyLevel.Eventual)
            json.Add("ConsistencyLevel", "eventual");

        if (UserAgent is not null)
            json.Add("User-Agent", UserAgent);

        if (MaxPageSize.HasValue)
            json.Add("Prefer", $"odata.maxpagesize={MaxPageSize.Value}");

        if (Return.HasFlag(ReturnOptions.Representation))
            json.Add("Prefer", "return=representation");

        if (Return.HasFlag(ReturnOptions.Minimal))
            json.Add("Prefer", "return=minimal");

        if (Return.HasFlag(ReturnOptions.IncludeUnknownEnumMembers))
            json.Add("Prefer", "return=include-unknown-enum-members");

        return json;
    }
}