namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Maintains the most-recently-used list of savegame paths inside the <see cref="UserSettings"/> of a <see cref="SettingsStore"/>.
    /// </summary>
    public class RecentFilesStore : IRecentFilesStore
    {
        /// <summary>
        /// The number of paths the list keeps.
        /// </summary>
        public const int MaxEntries = 10;

        private static readonly ILogger<RecentFilesStore> Logger = LogFactory.For<RecentFilesStore>();

        private readonly ISettingsStore store;
        private readonly UserSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecentFilesStore"/> class and drops blank paths and those past <see cref="MaxEntries"/>.
        /// </summary>
        /// <param name="store">The store whose settings hold the list.</param>
        public RecentFilesStore(ISettingsStore store)
        {
            this.store = store;
            this.settings = store.Settings;
            this.settings.RecentFiles = this.settings.RecentFiles
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Take(MaxEntries)
                .ToList();
            Logger.LogDebug("Loaded {Count} recent files", this.settings.RecentFiles.Count);
        }

        /// <summary>
        /// Raised after the list changed.
        /// </summary>
        public event EventHandler? Changed;

        /// <inheritdoc/>
        public IReadOnlyList<string> Entries => this.settings.RecentFiles;

        /// <inheritdoc/>
        /// <exception cref="SettingsException">The settings file cannot be written.</exception>
        public void Add(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Logger.LogTrace("Ignored a blank path");
                return;
            }

            Logger.LogDebug("Added {File} to the recent files", Path.GetFileName(path));

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

        /// <inheritdoc/>
        /// <exception cref="SettingsException">The settings file cannot be written.</exception>
        public void Remove(string path)
        {
            if (this.settings.RecentFiles.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                Logger.LogDebug("Removed {File} from the recent files", Path.GetFileName(path));
                this.store.Save();
                this.Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="SettingsException">The settings file cannot be written.</exception>
        public void Clear()
        {
            Logger.LogDebug("Cleared the {Count} recent files", this.settings.RecentFiles.Count);
            this.settings.RecentFiles.Clear();
            this.store.Save();
            this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
