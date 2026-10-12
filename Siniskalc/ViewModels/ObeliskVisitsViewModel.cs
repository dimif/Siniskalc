namespace Siniskalc.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The obelisks of the map and which of them one player has visited, for the page of that player. Marking an obelisk is an edit like any
    /// other: it is pending, can be undone and is written when the savefile is saved. The card is empty when the savefile has no obelisks to show.
    /// </summary>
    public sealed class ObeliskVisitsViewModel : AbstractViewModelBase
    {
        private readonly SaveDocumentHistory history;
        private readonly PlayerColor color;
        private IReadOnlyList<ObeliskRowViewModel> obelisks = [];
        private readonly ObeliskActionViewModel visitAll;
        private readonly ObeliskActionViewModel clearAll;
        private ObeliskEditSession? session;

        public ObeliskVisitsViewModel(SaveDocumentHistory history, PlayerColor color)
        {
            this.history = history;
            this.color = color;
            this.VisitAllCommand = new DelegateCommand(_ => this.Edit(visits => visits.WithAll(this.color, true)), _ => this.Visits is { } v && !v.AllVisited(this.color));
            this.ClearAllCommand = new DelegateCommand(_ => this.Edit(visits => visits.WithAll(this.color, false)), _ => this.Visits is { } v && v.VisitedCount(this.color) > 0);
            this.visitAll = new ObeliskActionViewModel("Visit all", "This player has visited every obelisk", this.VisitAllCommand);
            this.clearAll = new ObeliskActionViewModel("None", "This player has visited no obelisk", this.ClearAllCommand);
            this.Refresh();
        }

        /// <summary>
        /// Gets whether the savefile has obelisks to show.
        /// </summary>
        public bool HasObelisks => this.history.Current.Obelisks().Any;

        public IReadOnlyList<ObeliskRowViewModel> Obelisks => this.obelisks;

        /// <summary>
        /// Gets what the page shows as tiles: "Visit all" and "None" first, then the obelisks.
        /// </summary>
        public IReadOnlyList<object> Tiles => [this.visitAll, this.clearAll, .. this.obelisks];

        /// <summary>
        /// Gets how many obelisks the player visited, such as "2 of 7 visited".
        /// </summary>
        public string Summary => this.Visits is { } visits ? $"{visits.VisitedCount(this.color)} of {visits.Count} visited" : string.Empty;

        public ICommand VisitAllCommand { get; }

        public ICommand ClearAllCommand { get; }

        private ObeliskVisits? Visits => this.history.Current.Obelisks().GetModel();

        public void Refresh()
        {
            var visits = this.Visits;
            if (visits == null)
            {
                this.session = null;
                this.obelisks = [];
            }
            else
            {
                this.session ??= this.history.CreateObeliskSession();
                if (this.obelisks.Count != visits.Count)
                {
                    this.obelisks = Enumerable.Range(0, visits.Count)
                        .Select(i => new ObeliskRowViewModel(
                            i + 1,
                            CoordinatesText(visits.PositionOf(i)),
                            () => this.Visits!.IsVisited(i, this.color),
                            visited => this.Edit(v => v.WithVisited(i, this.color, visited))))
                        .ToList();
                }
            }

            foreach (var row in this.obelisks)
            {
                row.Refresh();
            }

            this.RaiseAllPropertiesChanged();
            CommandManager.InvalidateRequerySuggested();
        }

        private static string CoordinatesText(Position? position) => position switch
        {
            null => string.Empty,
            var p => MapLevelText.Coordinates(p.X, p.Y, p.Z),
        };

        private void Edit(System.Func<ObeliskVisits, ObeliskVisits> edit)
        {
            this.session?.Update(edit);
        }
    }
}
