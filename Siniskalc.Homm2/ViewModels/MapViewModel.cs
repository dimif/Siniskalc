namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The objects of the map that have a state the savegame keeps: its mines (and who owns them), its shrines (and the spell each teaches)
    /// and its sphinxes (and their riddles). One page with a tab for each kind.
    /// </summary>
    public sealed class MapViewModel : RecordsPageViewModelBase
    {
        private const string MineOwnerToolTip = "Who owns the mine. The flag on the map and the daily income change with it: the game works the income out at the start of the next day. Not tried in the game.";

        public MapViewModel(Game game)
            : base("Map", game.History, Homm2ChunkKinds.Mines, Homm2ChunkKinds.Shrines, Homm2ChunkKinds.Sphinxes)
        {
            var document = game.History.Current;
            var spellNames = Spell.All.Where(s => s.IsAvailableIn(game.GameVersion)).OrderBy(s => s.Name).Select(s => s.Name).ToList();
            this.Mines = new MinesListViewModel<MineRowViewModel<MineModel, PlayerColor>>(
                Enumerable.Range(0, document.Mines().Count)
                    .Select(i => new MineRowViewModel<MineModel, PlayerColor>(game.History.CreateMineSession(i), (mine, owner) => mine with { Owner = owner }, MineOwnerToolTip, hasLevels: false))
                    .ToList());
            var shrines = Enumerable.Range(0, document.Shrines().Count).Select(i => new ShrineRowViewModel(game.History.CreateShrineSession(i), spellNames)).ToList();
            this.ShrineList = new SortableListViewModel<ShrineRowViewModel>(
                shrines,
                new Dictionary<string, Func<ShrineRowViewModel, IComparable>>
                {
                    ["Name"] = s => s.CircleNumber,
                    ["X"] = s => (s.X, s.Y),
                    ["Y"] = s => (s.Y, s.X),
                    ["Spell"] = s => s.SpellName,
                });
            this.Sphinxes = Enumerable.Range(0, document.Sphinxes().Count).Select(i => new SphinxViewModel(game.History.CreateSphinxSession(i), game.GameVersion)).ToList();
            this.Refresh();
        }

        /// <summary>
        /// Gets the mines with the headings that sort them, the mine that is picked and how many are owned. HoMM2 has nothing more to show for
        /// the mine that is picked.
        /// </summary>
        public MinesListViewModel<MineRowViewModel<MineModel, PlayerColor>> Mines { get; }

        public IReadOnlyList<ShrineRowViewModel> Shrines => this.ShrineList.Items;

        /// <summary>
        /// Gets the shrines with the headings that sort them by name, either coordinate or spell.
        /// </summary>
        public SortableListViewModel<ShrineRowViewModel> ShrineList { get; }

        public IReadOnlyList<SphinxViewModel> Sphinxes { get; }

        public bool HasShrines => this.Shrines.Count > 0;

        public bool HasSphinxes => this.Sphinxes.Count > 0;

        /// <inheritdoc/>
        protected override void RefreshRecords()
        {
            this.Mines.Refresh();

            foreach (var row in this.Shrines)
            {
                row.Refresh();
            }

            foreach (var sphinx in this.Sphinxes)
            {
                sphinx.Refresh();
            }
        }
    }
}
