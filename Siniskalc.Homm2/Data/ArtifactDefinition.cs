namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The definition of an artifact, as it is read from <c>artifacts.json</c>.
    /// </summary>
    public sealed record ArtifactDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets the first version of the game that has the artifact.
        /// </summary>
        public required GameVersion AvailableFrom { get; init; }

        /// <summary>
        /// Gets what the artifact is for the editor: an ordinary one, the spell book or a scroll.
        /// </summary>
        public ArtifactRole Role { get; init; }

        /// <summary>
        /// Gets what the artifact adds to the skills of its owner.
        /// </summary>
        public ArtifactEffectDefinition EffectInfo { get; init; } = new();
    }
}
