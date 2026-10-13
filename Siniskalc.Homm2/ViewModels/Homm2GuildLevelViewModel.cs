namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One level of the Mage Guild of a castle: the spells it teaches, a box for each.
    /// </summary>
    public sealed class Homm2GuildLevelViewModel
    {
        public Homm2GuildLevelViewModel(int level, IReadOnlyList<ChoiceFieldViewModel> spells)
        {
            this.Level = level;
            this.Spells = spells;
        }

        public int Level { get; }

        public string Heading => $"Level {this.Level}";

        public IReadOnlyList<ChoiceFieldViewModel> Spells { get; }

        public void Refresh()
        {
            foreach (var spell in this.Spells)
            {
                spell.Refresh();
            }
        }
    }
}
