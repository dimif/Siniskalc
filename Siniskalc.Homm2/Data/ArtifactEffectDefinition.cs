namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// What an artifact adds to the skills of its owner, as it is read from <c>artifacts.json</c>.
    /// </summary>
    public sealed record ArtifactEffectDefinition
    {
        /// <summary>
        /// Gets the attributes the artifact changes and by how much.
        /// </summary>
        public AttributeEffect[] Modifiers { get; init; } = [];
    }
}
