namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Maintains the most-recently-used list of savegame paths inside the <see cref="UserSettings"/> of a <see cref="SettingsStore"/>.
    /// </summary>
    public class RecentFilesStore
    {
        public const int MaxEntries = 10;

        private readonly SettingsStore store;
        private readonly UserSettings settings;

        public RecentFilesStore(SettingsStore store)
        {
            this.store = store;
            this.settings = store.Settings;
            this.settings.RecentFiles = this.settings.RecentFiles
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Take(MaxEntries)
                .ToList();
        }

        /// <summary>
        /// Raised after the list changed.
        /// </summary>
        public event EventHandler? Changed;

        public IReadOnlyList<string> Entries => this.settings.RecentFiles;

        public void Add(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var list = this.settings.RecentFiles;
            list.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            list.Insert(0, path);
            if (list.Count > MaxEntries)
            {
                list.RemoveRange(MaxEntries, list.Count - MaxEntries);
            }

            this.store.Save();
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Remove(string path)
        {
            if (this.settings.RecentFiles.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                this.store.Save();
                this.Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Clear()
        {
            this.settings.RecentFiles.Clear();
            this.store.Save();
            this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
