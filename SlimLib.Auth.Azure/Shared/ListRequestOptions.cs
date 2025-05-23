using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SlimLib;

public class ListRequestOptions : ScalarRequestOptions
{
    public string? Filter { get; set; }
    public string? Search { get; set; }
    public HashSet<string> OrderBy { get; } = new();
    public bool? Count { get; set; }
    public int? Skip { get; set; }
    public int? Top { get; set; }

    public override JsonObject ToJson()
    {
        var json = base.ToJson();

        if (Filter is not null)
            json.Add("filter", Filter);

        if (Search is not null)
            json.Add("search", Search);

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

        return json;
    }
}