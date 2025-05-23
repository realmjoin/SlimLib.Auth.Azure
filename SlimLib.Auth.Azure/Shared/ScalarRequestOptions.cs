using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SlimLib;

public class ScalarRequestOptions : InvokeRequestOptions
{
    public HashSet<string> Select { get; } = new();
    public string? Expand { get; set; }

    public override JsonObject ToJson()
    {
        var json = base.ToJson();

        if (Select.Count > 0)
        {
            var list = new JsonArray();

            foreach (var item in Select)
                list.Add(JsonValue.Create(item));

            json.Add("select", list);
        }

        if (Expand is not null)
            json.Add("expand", Expand);

        return json;
    }
}