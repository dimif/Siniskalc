namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    public sealed record ArtifactDefinition : EntityDefinition
    {
        public required GameVersion AvailableFrom { get; init; }

        public ArtifactRole Role { get; init; }

        public ArtifactEffectDefinition EffectInfo { get; init; } = new();
    }
}
