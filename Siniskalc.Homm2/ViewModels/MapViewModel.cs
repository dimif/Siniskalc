namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
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
        private readonly MapTextProvider mapText;

        /// <param name="game">The game the page shows.</param>
        /// <param name="byteEditTargets">Makes the raw bytes of a record for the Bytes tab.</param>
        /// <param name="mapText">Words the header of the map is shown with.</param>
        public MapViewModel(Game game, IByteEditTargetFactory byteEditTargets, MapTextProvider mapText)
            : base("Map", game.History, Homm2ChunkKinds.Shrines, Homm2ChunkKinds.Sphinxes)
        {
            this.mapText = mapText;
            var document = game.History.Current;
            var spellNames = game.Catalog.Spells.Where(s => s.IsAvailableIn(game.GameVersion)).OrderBy(s => s.Name).Select(s => s.Name).ToList();
            var shrines = Enumerable.Range(0, document.Shrines().Count).Select(i => new ShrineRowViewModel(game.Catalog, game.History.CreateShrineSession(i), spellNames)).ToList();
            this.ShrineList = new SortableListViewModel<ShrineRowViewModel>(
                shrines,
                new Dictionary<string, Func<ShrineRowViewModel, IComparable>>
                {
                    ["Name"] = s => s.CircleNumber,
                    ["X"] = s => (s.X, s.Y),
                    ["Y"] = s => (s.Y, s.X),
                    ["Spell"] = s => s.SpellName,
                });
            this.Sphinxes = Enumerable.Range(0, document.Sphinxes().Count).Select(i => new SphinxViewModel(game.Catalog, game.History.CreateSphinxSession(i), game.GameVersion)).ToList();
            this.ByteRecords = new ByteRecordListViewModel(RecordsOf(game.History, byteEditTargets));
            this.Refresh();
        }

        /// <summary>
        /// Gets the shrines.
        /// </summary>
        public IReadOnlyList<ShrineRowViewModel> Shrines => this.ShrineList.Items;

        /// <summary>
        /// Gets the shrines with the headings that sort them by name, either coordinate or spell.
        /// </summary>
        public SortableListViewModel<ShrineRowViewModel> ShrineList { get; }

        /// <summary>
        /// Gets the sphinxes.
        /// </summary>
        public IReadOnlyList<SphinxViewModel> Sphinxes { get; }

        /// <summary>
        /// Gets what the header says: how many shrines and sphinxes the map has.
        /// </summary>
        public string MapSummary => this.mapText.Summarize(this.Shrines.Count, this.Sphinxes.Count);

        /// <summary>
        /// Gets whether the map has a shrine.
        /// </summary>
        public bool HasShrines => this.Shrines.Count > 0;

        /// <summary>
        /// Gets whether the map has a sphinx.
        /// </summary>
        public bool HasSphinxes => this.Sphinxes.Count > 0;

        /// <summary>
        /// Gets the records of the map whose raw bytes the Bytes tab can show, and the one it shows.
        /// </summary>
        public ByteRecordListViewModel ByteRecords { get; }

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

        private static IReadOnlyList<ByteRecordViewModel> RecordsOf(SaveDocumentHistory history, IByteEditTargetFactory factory)
        {
            var document = history.Current;
            var records = new List<ByteRecordViewModel>();
            records.AddRange(Enumerable.Range(0, document.Mines().Count).Select(i => new ByteRecordViewModel(document.Mines().GetName(i), () => factory.ForMine(history, i))));
            records.AddRange(Enumerable.Range(0, document.Shrines().Count).Select(i => new ByteRecordViewModel(document.Shrines().GetName(i), () => factory.ForShrine(history, i))));
            records.AddRange(Enumerable.Range(0, document.Sphinxes().Count).Select(i => new ByteRecordViewModel(document.Sphinxes().GetName(i), () => factory.ForSphinx(history, i))));
            if (document.Turn().Count > 0)
            {
                records.Add(new ByteRecordViewModel(document.Turn().GetName(0), () => factory.ForTurn(history)));
            }

            if (document.BuiltToday().Count > 0)
            {
                records.Add(new ByteRecordViewModel(document.BuiltToday().GetName(0), () => factory.ForBuiltToday(history)));
            }

            if (document.Obelisks().Count > 0)
            {
                records.Add(new ByteRecordViewModel("Obelisks", () => factory.ForObelisks(history)));
            }

            return records;
        }
    }
}
