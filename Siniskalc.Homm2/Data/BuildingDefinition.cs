namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    public sealed record BuildingDefinition : EntityDefinition
    {
        public required TownBuildings Flag { get; init; }

        public required string Description { get; init; }

        public required BuildingRole Role { get; init; }

        public required int Growth { get; init; }
    }
}
