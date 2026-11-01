namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    public sealed record MapObjectDefinition : EntityDefinition
    {
        public required MapObjectRole Role { get; init; }

        public required string NameFormat { get; init; }
    }
}
