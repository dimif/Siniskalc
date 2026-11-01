namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Models;

    public sealed record ArtifactEffectDefinition
    {
        public AttributeEffect[] Modifiers { get; init; } = [];
    }
}
