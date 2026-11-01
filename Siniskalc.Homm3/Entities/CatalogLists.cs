namespace Siniskalc.Homm3.Entities;

using System;
using System.Collections.Generic;

internal static class CatalogLists
{
    public static IReadOnlyList<T> AsReadOnly<T>(this T[] values) => Array.AsReadOnly(values);
}
