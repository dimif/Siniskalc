namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The obelisks of the map and which of them one player has visited, for the page of that player, in any game. Marking an obelisk is an
    /// edit like any other: it is pending, can be undone and is written when the savefile is saved. The card is empty when the savefile has
    /// no obelisks to show. A game gives the kind of chunk it keeps its obelisks in, the word for what the puzzle leads to and how it writes
    /// a place on its map.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public sealed class ObeliskVisitsViewModel<TColor> : ObeliskVisitsViewModelBase
        where TColor : AbstractEnumeration, IPlayerColor
    {
        private readonly SaveDocumentHistory history;
        private readonly ChunkKind kind;
        private readonly TColor color;
        private EditSession<ObeliskChunk, ObeliskVisits<TColor>>? session;

        /// <param name="history">The history of the savefile, where the edits go.</param>
        /// <param name="kind">The kind of chunk the game keeps its obelisks in.</param>
        /// <param name="color">The player whose visits are shown.</param>
        /// <param name="goal">What the puzzle of the obelisks leads to, in the words of the game, such as "Grail".</param>
        /// <param name="hasLevels">Whether the map of the game has two levels, so the places say which level they are on.</param>
        public ObeliskVisitsViewModel(SaveDocumentHistory history, ChunkKind kind, TColor color, string goal, bool hasLevels)
            : base($"The obelisks of the map this player has visited. Each one shows another piece of the puzzle that leads to the {goal}.", hasLevels)
        {
            this.history = history;
            this.kind = kind;
            this.color = color;
            this.Refresh();
        }

        /// <inheritdoc/>
        protected override int Count => this.Visits?.Count ?? 0;

        /// <inheritdoc/>
        protected override int VisitedCount => this.Visits?.VisitedCount(this.color) ?? 0;

        private ObeliskVisits<TColor>? Visits => ((ObeliskChunkCollection<TColor>)this.history.Current.GetCollection(this.kind)).GetModel();

        /// <inheritdoc/>
        protected override void Prepare() => this.session = this.Visits == null ? null : this.session ?? new EditSession<ObeliskChunk, ObeliskVisits<TColor>>(this.history, this.kind, 0);

        /// <inheritdoc/>
        protected override MapPosition? PositionOf(int index) => this.Visits!.PositionOf(index);

        /// <inheritdoc/>
        protected override bool IsVisited(int index) => this.Visits!.IsVisited(index, this.color);

        /// <inheritdoc/>
        protected override void SetVisited(int index, bool visited) => this.session?.Update(v => v.WithVisited(index, this.color, visited));

        /// <inheritdoc/>
        protected override void SetAll(bool visited) => this.session?.Update(v => v.WithAll(this.color, visited));
    }
}
