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
        /// <typeparam name="TEntry">The type of entry.</typeparam>
        /// <typeparam name="TVersion">The game's enum of its versions.</typeparam>
        /// <param name="entries">The entries to filter.</param>
        /// <param name="version">The version of the game.</param>
        /// <returns>The entries available in <paramref name="version"/>, in their order; evaluated lazily.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entries"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TEntry> AvailableIn<TEntry, TVersion>(this IEnumerable<TEntry> entries, TVersion version)
            where TEntry : VersionedEnumeration<TVersion>
            where TVersion : struct, Enum
        {
            return entries.Where(entry => entry.IsAvailableIn(version));
        }
    }
}
