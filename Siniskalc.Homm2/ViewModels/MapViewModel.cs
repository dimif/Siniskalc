namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// The objects of the map that have a state the savegame keeps: its mines (and who owns them), its shrines (and the spell each teaches)
    /// and its sphinxes (and their riddles). One page with a tab for each kind.
    /// </summary>
    public sealed class MapViewModel : ResettablePageViewModelBase
    {
        private readonly Game game;

        public MapViewModel(Game game)
            : base("Map", game.History)
        {
            this.game = game;
            var document = game.History.Current;
            var spellNames = Spell.All.Where(s => s.IsAvailableIn(game.GameVersion)).OrderBy(s => s.Name).Select(s => s.Name).ToList();
            this.Mines = Enumerable.Range(0, document.Mines().Count).Select(i => new MineRowViewModel(game.History.CreateMineSession(i))).ToList();
            this.Shrines = Enumerable.Range(0, document.Shrines().Count).Select(i => new ShrineRowViewModel(game.History.CreateShrineSession(i), spellNames)).ToList();
            this.Sphinxes = Enumerable.Range(0, document.Sphinxes().Count).Select(i => new SphinxViewModel(game.History.CreateSphinxSession(i), game.GameVersion)).ToList();
            this.Refresh();
        }

        public IReadOnlyList<MineRowViewModel> Mines { get; }

        public IReadOnlyList<ShrineRowViewModel> Shrines { get; }

        public IReadOnlyList<SphinxViewModel> Sphinxes { get; }

        public bool HasMines => this.Mines.Count > 0;

        public bool HasShrines => this.Shrines.Count > 0;

        public bool HasSphinxes => this.Sphinxes.Count > 0;

        /// <summary>
        /// Gets how many mines there are and how many have an owner, such as "41 mines, 36 of them owned".
        /// </summary>
        public string MinesSummary
        {
            get
            {
                var owned = this.game.History.Current.Mines().GetModels().Count(m => m.Owner != null);
                return $"{this.Mines.Count} mines, {owned} of them owned";
            }
        }

        /// <summary>
        /// Shows the objects again, after the history changed.
        /// </summary>
        public void Refresh()
        {
            foreach (var row in this.Mines)
            {
                row.Refresh();
            }

            foreach (var row in this.Shrines)
            {
                row.Refresh();
            }

            foreach (var sphinx in this.Sphinxes)
            {
                sphinx.Refresh();
            }

            this.HasPendingChanges = this.PendingChangesOfPage().Any();
            this.RaiseAllPropertiesChanged();
        }

        /// <inheritdoc/>
        protected override void OnHistoryChanged() => this.Refresh();

        /// <inheritdoc/>
        protected override void Reset()
        {
            foreach (var change in this.PendingChangesOfPage().ToList())
            {
                change.Revert(this.game.History);
            }
        }

        private IEnumerable<Siniskalc.Core.Data.PendingChange> PendingChangesOfPage() =>
            this.game.History.GetPendingChanges().Where(c => c.Kind == Homm2ChunkKinds.Mines || c.Kind == Homm2ChunkKinds.Shrines || c.Kind == Homm2ChunkKinds.Sphinxes);
    }
}
