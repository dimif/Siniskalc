namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The page of what nobody owns: its heroes and towns as links, and what a game adds (the mines and dwellings). It listens to the history
    /// of the savegame and shows its lists again after every edit, undo and redo. It has no edits of its own: a mine or a town has its page.
    /// </summary>
    public abstract class NeutralPageViewModelBase : TabbedPageViewModelBase, INeutralPage, IDisposable
    {
        private readonly SaveDocumentHistory history;

        /// <param name="history">The history of the savegame.</param>
        /// <param name="colorHex">The colour neutral is drawn with.</param>
        /// <param name="name">What the page is called: "Neutral", or the colour of a player the savegame has no data for.</param>
        protected NeutralPageViewModelBase(SaveDocumentHistory history, string colorHex, string name = "Neutral")
            : base(name)
        {
            this.history = history;
            this.ColorHex = colorHex;
            this.ResetCommand = new DelegateCommand(_ => { }, _ => false);
            history.Changed += this.OnHistoryChanged;
        }

        /// <inheritdoc/>
        public string Name => this.Title;

        /// <inheritdoc/>
        public string ColorHex { get; }

        /// <inheritdoc/>
        public ICommand ResetCommand { get; }

        /// <inheritdoc/>
        public IReadOnlyList<ILink> Heroes { get; private set; } = [];

        /// <inheritdoc/>
        public bool HasHeroes => this.Heroes.Count > 0;

        /// <inheritdoc/>
        public IReadOnlyList<ILink> Castles { get; private set; } = [];

        /// <inheritdoc/>
        public bool HasCastles => this.Castles.Count > 0;

        /// <inheritdoc/>
        public string Summary => string.Join(" · ", new[] { Count(this.Heroes.Count, "hero", "heroes"), Count(this.Castles.Count, "town", "towns"), this.ObjectsSummary }.Where(s => s.Length > 0));

        public void Dispose() => this.history.Changed -= this.OnHistoryChanged;

        /// <summary>
        /// Shows the lists again.
        /// </summary>
        public void Refresh()
        {
            this.Heroes = this.LoadHeroes();
            this.Castles = this.LoadCastles();
            this.RefreshObjects();
            this.RaiseAllPropertiesChanged();
        }

        /// <summary>
        /// Returns "3 mines" style text, or nothing for none.
        /// </summary>
        protected static string Count(int count, string one, string many) => count == 0 ? string.Empty : $"{count} {(count == 1 ? one : many)}";

        /// <summary>
        /// Gets the heroes nobody owns.
        /// </summary>
        protected abstract IReadOnlyList<ILink> LoadHeroes();

        /// <summary>
        /// Gets the towns nobody owns.
        /// </summary>
        protected abstract IReadOnlyList<ILink> LoadCastles();

        /// <summary>
        /// Shows what a game adds again, such as its mines and dwellings.
        /// </summary>
        protected virtual void RefreshObjects()
        {
        }

        /// <summary>
        /// Gets what a game adds to the summary, such as "31 mines · 58 dwellings", or nothing.
        /// </summary>
        protected virtual string ObjectsSummary => string.Empty;

        private void OnHistoryChanged(object? sender, EventArgs e) => this.Refresh();
    }
}
