namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of the adventure spells that are active on a hero this turn, in any game: one tile for each spell the game has, ticked while the
    /// spell is on the hero. The game gives the spells (HoMM3 has four, HoMM2 one) and how each is read and written.
    /// </summary>
    public sealed class ActiveSpellsCardViewModel
    {
        /// <param name="spells">The spells, each a box that reads and writes the hero.</param>
        public ActiveSpellsCardViewModel(IReadOnlyList<ToggleRowViewModel> spells)
        {
            this.Spells = spells;
        }

        /// <summary>
        /// Gets the spells that can be active on the hero, each with its switch.
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> Spells { get; }

        /// <summary>
        /// Shows the hero again. Call after the hero changed.
        /// </summary>
        public void Refresh()
        {
            foreach (var spell in this.Spells.ToList())
            {
                spell.Refresh();
            }
        }
    }
}
