namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    public sealed record ArtifactDefinition : EntityDefinition
    {
        public required GameVersion AvailableFrom { get; init; }

        public required ArtifactRole Role { get; init; }

        public required string[] Effects { get; init; }
    }
}
