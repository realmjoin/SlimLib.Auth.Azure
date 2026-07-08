using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlimLib;

public static class SlimApiExtensions
{
    private const string ArrayRoot = "value";

    private static readonly JsonSerializerOptions DefaultJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task<JsonElement> AsJsonElementAsync(this GraphOperation<JsonDocument?> task)
    {
        using var page = await task;

        return page is not null ? page.RootElement.Clone() : default;
    }

    public static async IAsyncEnumerable<JsonElement> AsJsonElementsAsync(this GraphArrayOperation<JsonDocument> task)
    {
        await foreach (var page in task)
        {
            using (page)
            {
                foreach (var element in page.RootElement.GetProperty(ArrayRoot).EnumerateArray())
                {
                    yield return element.Clone();
                }
            }
        }
    }

    public static async Task<T?> DeserializeItemAsync<T>(this GraphOperation<JsonDocument?> task, JsonSerializerOptions? options = null)
    {
        var checkedOptions = CheckOptions(options);

        using var page = await task;

        return page is not null ? page.RootElement.Deserialize<T>(checkedOptions) : default;
    }

    public static async IAsyncEnumerable<T[]> DeserializeItemsAsync<T>(this GraphArrayOperation<JsonDocument> task, JsonSerializerOptions? options = null)
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

    public static async IAsyncEnumerable<T> DeserializeAsync<T>(this GraphArrayOperation<JsonDocument> task, JsonSerializerOptions? options = null)
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

    public static async Task<List<T>> ToListAsync<T>(this GraphArrayOperation<JsonDocument> task, int limit = -1, JsonSerializerOptions? options = null)
    {
        if (limit < 0) limit = int.MaxValue;

        var checkedOptions = CheckOptions(options);

        var results = new List<T>();

        await foreach (var page in task)
        {
            using (page)
            {
                var items = page.RootElement.GetProperty(ArrayRoot).Deserialize<T[]>(checkedOptions);

                if (items is not null)
                {
                    foreach (var item in items)
                    {
                        if (results.Count >= limit)
                            return results;

                        results.Add(item);
                    }
                }
            }
        }

        return results;
    }

    public static async Task<T[]> ToArrayAsync<T>(this GraphArrayOperation<JsonDocument> task, int limit = -1, JsonSerializerOptions? options = null)
        => [.. await task.ToListAsync<T>(limit, options).ConfigureAwait(false)];

    private static JsonSerializerOptions CheckOptions(JsonSerializerOptions? options) => options switch
    {
        null => DefaultJsonOptions,
        { PropertyNamingPolicy: null } => new(options) { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
        _ => options
    };
}