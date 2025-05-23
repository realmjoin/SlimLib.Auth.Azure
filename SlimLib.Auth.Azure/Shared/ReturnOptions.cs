using System;

namespace SlimLib;

[Flags]
public enum ReturnOptions
{
    Unspecified,
    Representation,
    Minimal,
    IncludeUnknownEnumMembers,
}