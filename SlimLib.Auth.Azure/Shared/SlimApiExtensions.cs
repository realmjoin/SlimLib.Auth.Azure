using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlimLib;

public static class SlimApiExtensions
{
    private const string ArrayRoot = "value";

    private static readonly JsonSerializerOptions DefaultJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static JsonElement.ArrayEnumerator EnumerateGraphResults(this JsonDocument page)
    {
        return page.RootElement.GetProperty(ArrayRoot).EnumerateArray();
    }

    public static async Task<JsonElement> AsJsonElementAsync(this Task<JsonDocument?> task)
    {
        using var page = await task;

        return page is not null ? page.RootElement.Clone() : default;
    }

    public static async IAsyncEnumerable<JsonElement> AsJsonElementsAsync(this IAsyncEnumerable<JsonDocument> task)
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

    public static async Task<T?> DeserializeItemAsync<T>(this Task<JsonDocument?> task, JsonSerializerOptions? options = null)
    {
        var checkedOptions = CheckOptions(options);

        using var page = await task;

        return page is not null ? page.RootElement.Deserialize<T>(checkedOptions) : default;
    }

    public static async IAsyncEnumerable<T[]> DeserializeItemsAsync<T>(this IAsyncEnumerable<JsonDocument> task, JsonSerializerOptions? options = null)
    {
        var checkedOptions = CheckOptions(options);

        await foreach (var page in task)
        {
            using (page)
            {
                var items = page.RootElement.GetProperty(ArrayRoot).Deserialize<T[]>(checkedOptions);

                if (items is not null)
                {
                    yield return items;
                }
            }
        }
    }

    public static async IAsyncEnumerable<T> DeserializeAsync<T>(this IAsyncEnumerable<JsonDocument> task, JsonSerializerOptions? options = null)
    {
        var checkedOptions = CheckOptions(options);

        await foreach (var page in task)
        {
            using (page)
            {
                var item = page.RootElement.Deserialize<T>(checkedOptions);

                if (item is not null)
                {
                    yield return item;
                }
            }
        }
    }

    private static JsonSerializerOptions CheckOptions(JsonSerializerOptions? options) => options switch
    {
        null => DefaultJsonOptions,
        { PropertyNamingPolicy: null } => new(options) { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
        _ => options
    };
}