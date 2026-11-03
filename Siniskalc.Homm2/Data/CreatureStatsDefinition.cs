namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// The numbers the game keeps for a creature, as they are read from <c>creatures.json</c>.
    /// </summary>
    public sealed record CreatureStatsDefinition
    {
        /// <summary>
        /// Gets the attack.
        /// </summary>
        public required short Atk { get; init; }

        /// <summary>
        /// Gets the defense.
        /// </summary>
        public required short Def { get; init; }

        /// <summary>
        /// Gets the least damage of one creature.
        /// </summary>
        public required short MinDamage { get; init; }

        /// <summary>
        /// Gets the greatest damage of one creature.
        /// </summary>
        public required short MaxDamage { get; init; }

        /// <summary>
        /// Gets the hit points of one creature.
        /// </summary>
        public required short Health { get; init; }

        /// <summary>
        /// Gets the speed.
        /// </summary>
        public required short Speed { get; init; }

        /// <summary>
        /// Gets the creatures its dwelling adds each week.
        /// </summary>
        public required short Growth { get; init; }

        /// <summary>
        /// Gets the value the computer players give the creature.
        /// </summary>
        public required int AiValue { get; init; }

        /// <summary>
        /// Gets the value of the creature for the strength of an army.
        /// </summary>
        public required int FightValue { get; init; }

        /// <summary>
        /// Gets the price in gold.
        /// </summary>
        public required int Gold { get; init; }

        /// <summary>
        /// Gets the price in wood.
        /// </summary>
        public short Wood { get; init; }

        /// <summary>
        /// Gets the price in ore.
        /// </summary>
        public short Ore { get; init; }

        /// <summary>
        /// Gets the price in mercury.
        /// </summary>
        public short Mercury { get; init; }

        /// <summary>
        /// Gets the price in sulfur.
        /// </summary>
        public short Sulfur { get; init; }

        /// <summary>
        /// Gets the price in crystal.
        /// </summary>
        public short Crystal { get; init; }

        /// <summary>
        /// Gets the price in gems.
        /// </summary>
        public short Gems { get; init; }

        /// <summary>
        /// Gets the shots of a ranged creature, 0 for the others.
        /// </summary>
        public short Shots { get; init; }
    }
}
