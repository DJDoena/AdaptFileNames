using DoenaSoft.AbstractionLayer.IOServices;
using System.Collections.Generic;
using System.Linq;

namespace DoenaSoft.AdaptBookFileNames;

internal static class Extensions
{
    internal static List<string> SplitAtDash(this IFileInfo source)
        => [.. source.NameWithoutExtension.Split(" - ").Select(p => p.Trim())];

}