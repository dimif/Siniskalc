namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    public sealed record PlaceDefinition : EntityDefinition
    {
        public required PlaceKind Kind { get; init; }

        public required string Description { get; init; }
    }
}
