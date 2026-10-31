namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of what visited places gave a hero for its next battle, in any game: the morale and the luck the savegame keeps for it, and a
    /// box for each place that gives them, which sets what the game remembers of the place and its morale or luck together. A game passes its
    /// own places; a place whose bonus is not known for sure is a box that cannot be changed.
    /// </summary>
    public sealed class NextBattleCardViewModel : AbstractViewModelBase
    {
        /// <param name="title">The heading of the card.</param>
        /// <param name="bonus">The morale and the luck for the next battle.</param>
        /// <param name="sources">The places that give them, each a box.</param>
        /// <param name="note">What the card says under the boxes, such as how far it was tried in the game.</param>
        /// <param name="clearAll">Takes every bonus and every place away, or <c>null</c> for a card that has no such button.</param>
        public NextBattleCardViewModel(string title, IReadOnlyList<IAmountRow> bonus, IReadOnlyList<IToggleRow> sources, string note, Action? clearAll = null)
        {
            this.HasClear = clearAll != null;
            this.ClearCommand = new DelegateCommand(_ => clearAll?.Invoke(), _ => clearAll != null && (bonus.Any(b => b.Value != 0) || sources.Any(s => s.IsChecked)));
            this.Title = title;
            this.Bonus = bonus;
            this.Sources = sources;
            this.Note = note;
        }

        /// <summary>
        /// Gets the heading of the card.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the morale and the luck for the next battle.
        /// </summary>
        public IReadOnlyList<IAmountRow> Bonus { get; }

        /// <summary>
        /// Gets the places that give them.
        /// </summary>
        public IReadOnlyList<IToggleRow> Sources { get; }

        /// <summary>
        /// Gets how many of the places that give a bonus the hero has visited, such as "2 of 5".
        /// </summary>
        public string SourcesText => $"{this.Sources.Count(s => s.IsChecked)} of {this.Sources.Count}";

        /// <summary>
        /// Gets a value indicating whether the card has the button that takes every bonus away.
        /// </summary>
        public bool HasClear { get; }

        /// <summary>
        /// Gets the command that sets the morale and the luck to 0 and forgets every place.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Gets what the card says under the boxes.
        /// </summary>
        public string Note { get; }

        /// <summary>
        /// Shows the hero again. Call after the hero changed.
        /// </summary>
        public void Refresh()
        {
            foreach (var row in this.Bonus)
            {
                row.Refresh();
            }

            foreach (var source in this.Sources)
            {
                source.Refresh();
            }

            this.RaisePropertyChanged(nameof(this.SourcesText));
        }
    }
}
