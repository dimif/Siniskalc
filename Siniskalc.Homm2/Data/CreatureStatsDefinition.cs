namespace Siniskalc.Homm2.Data
{
    public sealed record CreatureStatsDefinition
    {
        public required short Atk { get; init; }

        public required short Def { get; init; }

        public required short MinDamage { get; init; }

        public required short MaxDamage { get; init; }

        public required short Health { get; init; }

        public required short Speed { get; init; }

        public required short Growth { get; init; }

        public required int AiValue { get; init; }

        public required int FightValue { get; init; }

        public required int Gold { get; init; }

        public short Wood { get; init; }

        public short Ore { get; init; }

        public short Mercury { get; init; }

        public short Sulfur { get; init; }

        public short Crystal { get; init; }

        public short Gems { get; init; }

        public short Shots { get; init; }
    }
}
