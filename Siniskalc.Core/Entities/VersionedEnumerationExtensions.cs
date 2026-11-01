namespace Siniskalc.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Filters the entries of a <see cref="VersionedEnumeration{TVersion}"/> by the version of the game.
    /// </summary>
    public static class VersionedEnumerationExtensions
    {
        /// <summary>
        /// Returns the entries that are in <paramref name="version"/> of the game.
        /// </summary>
        public static IEnumerable<TEntry> AvailableIn<TEntry, TVersion>(this IEnumerable<TEntry> entries, TVersion version)
            where TEntry : VersionedEnumeration<TVersion>
            where TVersion : struct, Enum
        {
            return entries.Where(entry => entry.IsAvailableIn(version));
        }
    }
}
