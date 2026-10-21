namespace Siniskalc.Homm3.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The obelisks of the map and which of them one player has visited, for the page of that player. Marking an obelisk is an edit like any
    /// other: it is pending, can be undone and is written when the savefile is saved. The card is empty when the savefile has no obelisks to show.
    /// </summary>
    public sealed class ObeliskVisitsViewModel : ObeliskVisitsViewModelBase
    {
        private readonly SaveDocumentHistory history;
        private readonly PlayerColor color;
        private ObeliskEditSession<PlayerColor>? session;

        public ObeliskVisitsViewModel(SaveDocumentHistory history, PlayerColor color)
            : base("The obelisks of the map this player has visited. Each one shows another piece of the puzzle that leads to the Grail.")
        {
            this.history = history;
            this.color = color;
            this.Refresh();
        }

        /// <inheritdoc/>
        protected override int Count => this.Visits?.Count ?? 0;

        /// <inheritdoc/>
        protected override int VisitedCount => this.Visits?.VisitedCount(this.color) ?? 0;

        private ObeliskVisits<PlayerColor>? Visits => this.history.Current.Obelisks().GetModel();

        /// <inheritdoc/>
        protected override void Prepare() => this.session = this.Visits == null ? null : this.session ?? this.history.CreateObeliskSession();

        /// <inheritdoc/>
        protected override string CoordinatesOf(int index) => this.Visits!.PositionOf(index) switch
        {
            null => string.Empty,
            var p => MapLevelText.Coordinates(p.X, p.Y, p.Level),
        };

        /// <inheritdoc/>
        protected override bool IsVisited(int index) => this.Visits!.IsVisited(index, this.color);

        /// <inheritdoc/>
        protected override void SetVisited(int index, bool visited) => this.session?.Update(v => v.WithVisited(index, this.color, visited));

        /// <inheritdoc/>
        protected override void SetAll(bool visited) => this.session?.Update(v => v.WithAll(this.color, visited));
    }
}
