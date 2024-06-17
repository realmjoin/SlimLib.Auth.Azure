using System.Collections.Generic;
using System.Linq;

namespace SlimLib;

public static class RequestOptions
{
    public static string BuildLink(string call, IEnumerable<string> args)
    {
        if (args.Any())
        {
            return call + "?" + string.Join("&", args);
        }

        return call;
    }
}