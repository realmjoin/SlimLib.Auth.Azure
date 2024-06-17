using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlimLib;

public static class SlimApiExtensions
{
    private const string ArrayRoot = "value";

    public static JsonElement.ArrayEnumerator EnumerateGraphResults(this JsonDocument page)
    {
        return page.RootElement.GetProperty(ArrayRoot).EnumerateArray();
    }

    public static async Task<JsonElement> AsJsonElement(this Task<JsonDocument?> task)
    {
        using var page = await task;

        return page is not null ? page.RootElement.Clone() : default;
    }

    public static async IAsyncEnumerable<JsonElement> AsJsonElements(this IAsyncEnumerable<JsonDocument> task)
    {
        await foreach (var page in task)
        {
            using (page)
            {
                foreach (var element in page.EnumerateGraphResults())
                {
                    yield return element.Clone();
                }
            }
        }
    }

    public static async Task<T?> Deserialize<T>(this Task<JsonDocument?> task, JsonSerializerOptions? options = null)
    {
        options ??= new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        options.PropertyNamingPolicy ??= JsonNamingPolicy.CamelCase;

        using var page = await task;

        return page is not null ? page.RootElement.Deserialize<T>(options) : default;
    }

    public static async IAsyncEnumerable<T[]> Deserialize<T>(this IAsyncEnumerable<JsonDocument> task, JsonSerializerOptions? options = null)
    {
        options ??= new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        options.PropertyNamingPolicy ??= JsonNamingPolicy.CamelCase;

        await foreach (var page in task)
        {
            using (page)
            {
                var items = page.RootElement.GetProperty(ArrayRoot).Deserialize<T[]>(options);

                if (items is not null)
                {
                    yield return items;
                }
            }
        }
    }
}