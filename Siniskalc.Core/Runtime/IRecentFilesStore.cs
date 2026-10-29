namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Collections.Generic;

    /// <summary>The most-recently-used list of savegame paths.</summary>
    public interface IRecentFilesStore
    {
        /// <summary>Raised after the list changed.</summary>
        event EventHandler? Changed;

        IReadOnlyList<string> Entries { get; }

        void Add(string path);

        void Remove(string path);

        void Clear();
    }
}
