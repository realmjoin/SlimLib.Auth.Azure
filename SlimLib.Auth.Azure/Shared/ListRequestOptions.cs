using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SlimLib;

public class ListRequestOptions
{
    public HashSet<string> Select { get; } = new();
    public string? Filter { get; set; }
    public string? Search { get; set; }
    public string? Expand { get; set; }
    public HashSet<string> OrderBy { get; } = new();
    public bool? Count { get; set; }
    public int? Skip { get; set; }
    public int? Top { get; set; }
    public int? MaxPageSize { get; set; }
    public bool ConsistencyLevelEventual { get; set; }

    public JsonObject ToJson()
    {
        var json = new JsonObject();

        if (Select.Count > 0)
        {
            var list = new JsonArray();

            foreach (var item in Select)
                list.Add(JsonValue.Create(item));

            json.Add("select", list);
        }

        if (Filter is not null)
            json.Add("filter", Filter);

        if (Search is not null)
            json.Add("search", Search);

        if (Expand is not null)
            json.Add("expand", Expand);

        if (OrderBy.Count > 0)
        {
            var list = new JsonArray();

            foreach (var item in OrderBy)
                list.Add(JsonValue.Create(item));

            json.Add("orderBy", list);
        }

        if (Count is not null)
            json.Add("count", Count.Value);

        if (Skip is not null)
            json.Add("skip", Skip.Value);

        if (Top is not null)
            json.Add("top", Top.Value);

        if (ConsistencyLevelEventual)
            json.Add("consistencyLevel", "eventual");

        return json;
    }
}