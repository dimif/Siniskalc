namespace Siniskalc.Homm2.Data
{
    public sealed record FactionDefinition : EntityDefinition
    {
        public required bool Library { get; init; }

        public required string GrowthBuildingName { get; init; }

        public required string SpecialBuildingName { get; init; }

        public required string SpecialBuildingDescription { get; init; }

        public required int[] HeroIds { get; init; }

        public required int[] GuildSpellCounts { get; init; }
    }
}
