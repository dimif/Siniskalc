namespace Siniskalc.Homm3.Entities;

using System;
using System.Collections.Generic;

internal static class CatalogLists
{
    /// <summary>
    /// Wraps an array as a read-only list.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="values">The array.</param>
    /// <returns>The list.</returns>
    public static IReadOnlyList<T> AsReadOnly<T>(this T[] values) => Array.AsReadOnly(values);
}
