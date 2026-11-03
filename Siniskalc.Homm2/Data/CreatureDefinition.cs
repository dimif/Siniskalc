namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The definition of a creature, as it is read from <c>creatures.json</c>.
    /// </summary>
    public sealed record CreatureDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets the identifier of the faction of the creature, or <see langword="null"/> for a neutral creature.
        /// </summary>
        public required string? Faction { get; init; }

        /// <summary>
        /// Gets the level of the creature in its faction, from 1.
        /// </summary>
        public required int Tier { get; init; }

        /// <summary>
        /// Gets the numbers the game keeps for the creature.
        /// </summary>
        public required CreatureStatsDefinition Stats { get; init; }

        /// <summary>
        /// Gets the identifiers of the abilities of the creature.
        /// </summary>
        public required string[] Abilities { get; init; }

        /// <summary>
        /// Gets the identifier of the creature this one is the upgrade of, or <see langword="null"/> when it is not an upgrade.
        /// </summary>
        public required string? BaseCreature { get; init; }

        /// <summary>
        /// Gets the dwelling the creature is recruited in, or <see langword="null"/> for a creature no dwelling offers.
        /// </summary>
        public required Dwellings? Dwelling { get; init; }
    }
}
