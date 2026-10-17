namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The obelisks of the map and which of them one player has visited, for the page of that player (as in Heroes of Might and Magic III).
    /// Marking an obelisk is an edit like any other: it is pending, can be undone and is written when the savegame is saved. The tab is empty
    /// when the savegame has no obelisks to show.
    /// </summary>
    public sealed class ObeliskVisitsViewModel : AbstractViewModelBase
    {
        private readonly SaveDocumentHistory history;
        private readonly Color color;
        private IReadOnlyList<ToggleRowViewModel> obelisks = [];
        private ObeliskEditSession? session;

        public ObeliskVisitsViewModel(SaveDocumentHistory history, Color color)
        {
            this.history = history;
            this.color = color;
            this.VisitAllCommand = new DelegateCommand(_ => this.Edit(visits => visits.WithAll(this.color, true)), _ => this.Visits is { } v && !v.AllVisited(this.color));
            this.ClearAllCommand = new DelegateCommand(_ => this.Edit(visits => visits.WithAll(this.color, false)), _ => this.Visits is { } v && v.VisitedCount(this.color) > 0);
            this.Refresh();
        }

        /// <summary>
        /// Gets whether the savegame has obelisks to show.
        /// </summary>
        public bool HasObelisks => this.history.Current.Obelisks().Any;

        /// <summary>
        /// Gets the obelisks, each with a box that is ticked when the player has visited it.
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> Obelisks => this.obelisks;

        /// <summary>
        /// Gets how many obelisks the player visited, such as "2 of 9 visited".
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
                        .Select(i => new ToggleRowViewModel(
                            visits.PositionOf(i) is { } at ? $"Obelisk {i + 1} at {MapLevelText.Coordinates(at.X, at.Y)}" : $"Obelisk {i + 1}",
                            () => this.Visits!.IsVisited(i, this.color),
                            visited => this.Edit(v => v.WithVisited(i, this.color, visited)),
                            toolTip: "Tick it when this player has visited the obelisk. Not tried in the game."))
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

        private void Edit(System.Func<ObeliskVisits, ObeliskVisits> edit)
        {
            this.session?.Update(edit);
        }
    }
}
