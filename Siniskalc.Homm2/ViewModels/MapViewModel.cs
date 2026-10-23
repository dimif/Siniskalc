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
    /// The objects of the map that no player owns and that have a state the savegame keeps: its shrines (and the spell each teaches) and its
    /// sphinxes (and their riddles). One page with a tab for each kind. The mines are on the pages of their owners and on the page of Neutral.
    /// </summary>
    public sealed class MapViewModel : RecordsPageViewModelBase
    {
        public MapViewModel(Game game)
            : base("Map", game.History, Homm2ChunkKinds.Shrines, Homm2ChunkKinds.Sphinxes)
        {
            var document = game.History.Current;
            var spellNames = Spell.All.Where(s => s.IsAvailableIn(game.GameVersion)).OrderBy(s => s.Name).Select(s => s.Name).ToList();
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

        public IReadOnlyList<ShrineRowViewModel> Shrines => this.ShrineList.Items;

        /// <summary>
        /// Gets the shrines with the headings that sort them by name, either coordinate or spell.
        /// </summary>
        public SortableListViewModel<ShrineRowViewModel> ShrineList { get; }

        public IReadOnlyList<SphinxViewModel> Sphinxes { get; }

        /// <summary>
        /// Gets what the header says: how many shrines and sphinxes the map has.
        /// </summary>
        public string MapSummary => $"{this.Shrines.Count} {(this.Shrines.Count == 1 ? "shrine" : "shrines")}, {this.Sphinxes.Count} {(this.Sphinxes.Count == 1 ? "sphinx" : "sphinxes")}";

        public bool HasShrines => this.Shrines.Count > 0;

        public bool HasSphinxes => this.Sphinxes.Count > 0;

        /// <inheritdoc/>
        protected override void RefreshRecords()
        {
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
