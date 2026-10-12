namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// The lines of the recent files menu, kept up to date with the list of recent files. Picking a file raises <see cref="OpenRequested"/>:
    /// what opening means is up to whoever listens.
    /// </summary>
    public sealed class RecentFilesMenuViewModel : IDisposable
    {
        private readonly RecentFilesStore store;

        public RecentFilesMenuViewModel(RecentFilesStore store)
        {
            this.store = store;
            this.store.Changed += this.OnStoreChanged;
            this.Rebuild();
        }

        /// <summary>
        /// Raised with the path of the file the user picked.
        /// </summary>
        public event Action<string>? OpenRequested;

        /// <summary>
        /// Gets the lines of the menu: the files with the newest first, then a separator and the command that clears the list; or a note
        /// when there are no files.
        /// </summary>
        public ObservableCollection<RecentMenuItemViewModel> Items { get; } = new();

        public void Dispose() => this.store.Changed -= this.OnStoreChanged;

        private void OnStoreChanged(object? sender, EventArgs e) => this.Rebuild();

        private void Rebuild()
        {
            this.Items.Clear();
            if (this.store.Entries.Count == 0)
            {
                this.Items.Add(new RecentMenuItemViewModel(RecentMenuItemKind.Empty, "(empty)"));
                return;
            }

            var number = 1;
            foreach (var path in this.store.Entries)
            {
                // A menu takes the character after an underscore as the access key, so the underscores of a path are doubled.
                var picked = path;
                this.Items.Add(new RecentMenuItemViewModel(
                    RecentMenuItemKind.File,
                    $"_{number++} {path.Replace("_", "__")}",
                    path,
                    new DelegateCommand(_ => this.OpenRequested?.Invoke(picked))));
            }

            this.Items.Add(new RecentMenuItemViewModel(RecentMenuItemKind.Separator, string.Empty));
            this.Items.Add(new RecentMenuItemViewModel(RecentMenuItemKind.Clear, "_Clear list", command: new DelegateCommand(_ => this.store.Clear())));
        }
    }
}
