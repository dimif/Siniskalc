namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The objects of the map that no player owns and that have a state the savegame keeps: its shrines (and the spell each teaches) and its
    /// sphinxes (and their riddles). One page with a tab for each kind. The mines are on the pages of their owners and on the page of Neutral;
    /// the Bytes tab shows the raw bytes of every record of the map, the mines among them.
    /// </summary>
    public sealed class MapViewModel : RecordsPageViewModelBase
    {
        private MapRecordViewModel? selectedRecord;

        /// <param name="game">The game the page shows.</param>
        /// <param name="byteEditTargets">Makes the raw bytes of a record for the Bytes tab; none when <c>null</c>.</param>
        public MapViewModel(Game game, IByteEditTargetFactory? byteEditTargets = null)
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
            this.Records = byteEditTargets == null ? [] : RecordsOf(game.History, byteEditTargets);
            this.selectedRecord = this.Records.FirstOrDefault();
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

        /// <summary>
        /// Gets the records of the map whose raw bytes the Bytes tab can show.
        /// </summary>
        public IReadOnlyList<MapRecordViewModel> Records { get; }

        /// <summary>
        /// Gets or sets the record the Bytes tab shows.
        /// </summary>
        public MapRecordViewModel? SelectedRecord
        {
            get => this.selectedRecord;
            set
            {
                if (value != this.selectedRecord)
                {
                    this.selectedRecord = value;
                    this.RaisePropertyChanged(nameof(this.SelectedRecord));
                }
            }
        }

        /// <summary>
        /// Creates the raw bytes of the record that is picked, or <c>null</c> when none is. The caller owns the target and disposes it.
        /// </summary>
        public IByteEditTarget? CreateByteEditTarget() => this.SelectedRecord?.Create();

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

        private static IReadOnlyList<MapRecordViewModel> RecordsOf(SaveDocumentHistory history, IByteEditTargetFactory factory)
        {
            var document = history.Current;
            var records = new List<MapRecordViewModel>();
            records.AddRange(Enumerable.Range(0, document.Mines().Count).Select(i => new MapRecordViewModel(document.Mines().GetName(i), () => factory.ForMine(history, i))));
            records.AddRange(Enumerable.Range(0, document.Shrines().Count).Select(i => new MapRecordViewModel(document.Shrines().GetName(i), () => factory.ForShrine(history, i))));
            records.AddRange(Enumerable.Range(0, document.Sphinxes().Count).Select(i => new MapRecordViewModel(document.Sphinxes().GetName(i), () => factory.ForSphinx(history, i))));
            if (document.Turn().Count > 0)
            {
                records.Add(new MapRecordViewModel(document.Turn().GetName(0), () => factory.ForTurn(history)));
            }

            if (document.BuiltToday().Count > 0)
            {
                records.Add(new MapRecordViewModel(document.BuiltToday().GetName(0), () => factory.ForBuiltToday(history)));
            }

            if (document.Obelisks().Count > 0)
            {
                records.Add(new MapRecordViewModel("Obelisks", () => factory.ForObelisks(history)));
            }

            return records;
        }
    }
}
