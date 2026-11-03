namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// What an artifact adds to one attribute of a hero.
    /// </summary>
    public sealed record AttributeEffect
    {
        /// <summary>
        /// Gets the attribute that changes.
        /// </summary>
        public required ArtifactAttribute Attribute { get; init; }

        /// <summary>
        /// Gets how much the attribute changes by; negative for a penalty.
        /// </summary>
        public required short Amount { get; init; }
    }
}
