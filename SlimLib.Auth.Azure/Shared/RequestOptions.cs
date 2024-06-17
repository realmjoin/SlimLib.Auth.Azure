using System.Collections.Generic;
using System.Linq;

namespace SlimLib;

internal static class RequestOptions
{
    internal static string BuildLink(string call, IEnumerable<string> args)
    {
        if (args.Any())
        {
            return call + "?" + string.Join("&", args);
        }

        return call;
    }
}