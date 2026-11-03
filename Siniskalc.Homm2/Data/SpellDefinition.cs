namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The definition of a spell, as it is read from <c>spells.json</c>.
    /// </summary>
    public sealed record SpellDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets the spell points the spell costs.
        /// </summary>
        public required int SpellPoints { get; init; }

        /// <summary>
        /// Gets whether the spell is cast in a battle, on the map or both.
        /// </summary>
        public required SpellType Type { get; init; }

        /// <summary>
        /// Gets the level of the spell.
        /// </summary>
        public required int Level { get; init; }

        /// <summary>
        /// Gets the identifier of the group the spell belongs to.
        /// </summary>
        public required string Group { get; init; }
    }
}
