namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// The definition of a faction, as it is read from <c>factions.json</c>.
    /// </summary>
    public sealed record FactionDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets whether the castle of the faction can build a Library.
        /// </summary>
        public required bool Library { get; init; }

        /// <summary>
        /// Gets the name of the building of the faction that adds to the growth of the first dwelling.
        /// </summary>
        public required string GrowthBuildingName { get; init; }

        /// <summary>
        /// Gets the name of the special building of the faction.
        /// </summary>
        public required string SpecialBuildingName { get; init; }

        /// <summary>
        /// Gets what the special building of the faction does.
        /// </summary>
        public required string SpecialBuildingDescription { get; init; }

        /// <summary>
        /// Gets the numbers of the heroes of the faction.
        /// </summary>
        public required int[] HeroIds { get; init; }

        /// <summary>
        /// Gets how many spells each level of the Mage Guild of the faction offers.
        /// </summary>
        public required int[] GuildSpellCounts { get; init; }
    }
}
