namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    public sealed record AttributeEffect
    {
        public required ArtifactAttribute Attribute { get; init; }

        public required short Amount { get; init; }
    }
}
