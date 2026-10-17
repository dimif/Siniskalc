namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The obelisks of the map and which of them one player has visited, for the page of that player (as in Heroes of Might and Magic III).
    /// Marking an obelisk is an edit like any other: it is pending, can be undone and is written when the savegame is saved. The tab is empty
    /// when the savegame has no obelisks to show.
    /// </summary>
    public sealed class ObeliskVisitsViewModel : ObeliskVisitsViewModelBase
    {
        private readonly SaveDocumentHistory history;
        private readonly Color color;
        private ObeliskEditSession? session;

        public ObeliskVisitsViewModel(SaveDocumentHistory history, Color color)
            : base("The obelisks of the map this player has visited. Each one shows another piece of the puzzle that leads to the Ultimate Artifact.")
        {
            this.history = history;
            this.color = color;
            this.Refresh();
        }

        /// <inheritdoc/>
        protected override int Count => this.Visits?.Count ?? 0;

        /// <inheritdoc/>
        protected override int VisitedCount => this.Visits?.VisitedCount(this.color) ?? 0;

        private ObeliskVisits? Visits => this.history.Current.Obelisks().GetModel();

        /// <inheritdoc/>
        protected override void Prepare() => this.session = this.Visits == null ? null : this.session ?? this.history.CreateObeliskSession();

        /// <inheritdoc/>
        protected override string CoordinatesOf(int index) => this.Visits!.PositionOf(index) is { } at ? MapLevelText.Coordinates(at.X, at.Y) : string.Empty;

        /// <inheritdoc/>
        protected override bool IsVisited(int index) => this.Visits!.IsVisited(index, this.color);

        /// <inheritdoc/>
        protected override void SetVisited(int index, bool visited) => this.session?.Update(v => v.WithVisited(index, this.color, visited));

        /// <inheritdoc/>
        protected override void SetAll(bool visited) => this.session?.Update(v => v.WithAll(this.color, visited));
    }
}
