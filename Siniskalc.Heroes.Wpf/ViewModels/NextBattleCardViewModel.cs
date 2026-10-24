namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of what visited places gave a hero for its next battle, in any game: the morale and the luck the savegame keeps for it, and a
    /// box for each place that gives them, which sets what the game remembers of the place and its morale or luck together. A game passes its
    /// own places; a place whose bonus is not known for sure is a box that cannot be changed.
    /// </summary>
    public sealed class NextBattleCardViewModel
    {
        /// <param name="title">The heading of the card.</param>
        /// <param name="bonus">The morale and the luck for the next battle.</param>
        /// <param name="sources">The places that give them, each a box.</param>
        /// <param name="note">What the card says under the boxes, such as how far it was tried in the game.</param>
        public NextBattleCardViewModel(string title, IReadOnlyList<AmountRowViewModel> bonus, IReadOnlyList<ToggleRowViewModel> sources, string note)
        {
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
        public IReadOnlyList<AmountRowViewModel> Bonus { get; }

        /// <summary>
        /// Gets the places that give them.
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> Sources { get; }

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
        }
    }
}
