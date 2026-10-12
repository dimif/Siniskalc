namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// One creature in the creature finder list.
    /// </summary>
    public sealed class Homm2CreatureChoiceViewModel
    {
        public Homm2CreatureChoiceViewModel(Homm2Creature creature)
        {
            this.Creature = creature;
        }

        public Homm2Creature Creature { get; }

        public string Name => this.Creature.Name;

        /// <summary>
        /// Gets the town the creature is recruited in, or "Neutral"; the list is grouped by it.
        /// </summary>
        public string Race => this.Creature.Race?.ToString() ?? "Neutral";

        /// <summary>
        /// Gets the creature level, 1 to 6.
        /// </summary>
        public int Level => this.Creature.Level;

        /// <summary>
        /// Gets how many of the creature a dwelling grows in a week.
        /// </summary>
        public int Growth => this.Creature.Growth;
    }
}
