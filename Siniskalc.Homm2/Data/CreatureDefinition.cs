namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    public sealed record CreatureDefinition : EntityDefinition
    {
        public required string? Faction { get; init; }

        public required int Tier { get; init; }

        public required CreatureStatsDefinition Stats { get; init; }

        public required string[] Abilities { get; init; }

        public required string? BaseCreature { get; init; }

        public required Dwellings? Dwelling { get; init; }
    }
}
