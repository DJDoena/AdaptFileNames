using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DoenaSoft.AbstractionLayer.IOServices;
using SIO = System.IO;

namespace DoenaSoft.AdaptFileNames;

internal static class Extensions
{
    internal static List<string> SplitAtDash(this IFileInfo source)
        => [.. source.NameWithoutExtension.Split(" - ").Select(p => p.Trim())];

}
