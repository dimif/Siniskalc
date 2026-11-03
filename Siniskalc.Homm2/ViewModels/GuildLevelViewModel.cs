namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One level of the Mage Guild of a castle: the spells it teaches, a box for each.
    /// </summary>
    public sealed class GuildLevelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuildLevelViewModel"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="spells">The spells.</param>
        public GuildLevelViewModel(int level, IReadOnlyList<ChoiceFieldViewModel> spells)
        {
            this.Level = level;
            this.Spells = spells;
        }

        /// <summary>
        /// Gets the level of the Mage Guild, from 1.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets the title of the level.
        /// </summary>
        public string Heading => $"Level {this.Level}";

        /// <summary>
        /// Gets the fields of the spells the level offers.
        /// </summary>
        public IReadOnlyList<ChoiceFieldViewModel> Spells { get; }

        /// <summary>
        /// Reads the model again and tells the view that every value may have changed.
        /// </summary>
        public void Refresh()
        {
            foreach (var spell in this.Spells)
            {
                spell.Refresh();
            }
        }
    }
}
