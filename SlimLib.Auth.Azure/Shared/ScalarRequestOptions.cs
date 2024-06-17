using System.Collections.Generic;

namespace SlimLib;

public class ScalarRequestOptions
{
    public HashSet<string> Select { get; } = new();
    public string? Expand { get; set; }
}