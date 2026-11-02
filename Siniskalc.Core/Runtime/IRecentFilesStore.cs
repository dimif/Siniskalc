namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Collections.Generic;

    /// <summary>The most-recently-used list of savegame paths.</summary>
    public interface IRecentFilesStore
    {
        /// <summary>Raised after the list changed.</summary>
        event EventHandler? Changed;

        /// <summary>Gets the paths, the most recent first.</summary>
        IReadOnlyList<string> Entries { get; }

        /// <summary>Puts <paramref name="path"/> at the top of the list, taking it from where it was in the list. Blank paths are ignored.</summary>
        /// <param name="path">The path of a savegame.</param>
        void Add(string path);

        /// <summary>Takes <paramref name="path"/> out of the list when it is there.</summary>
        /// <param name="path">The path of a savegame.</param>
        void Remove(string path);

        /// <summary>Empties the list.</summary>
        void Clear();
    }
}
