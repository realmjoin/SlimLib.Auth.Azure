using System.Collections.Generic;

namespace SlimLib;

public class ListRequestOptions : ScalarRequestOptions
{
    public string? Filter { get; set; }
    public string? Search { get; set; }
    public HashSet<string> OrderBy { get; } = new();
    public bool? Count { get; set; }
    public int? Skip { get; set; }
    public int? Top { get; set; }
}