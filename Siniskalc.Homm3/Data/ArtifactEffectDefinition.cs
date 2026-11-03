namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// What an artifact does for its owner, as it is read from <c>artifacts.json</c>.
    /// </summary>
    public sealed record ArtifactEffectDefinition
    {
        /// <summary>
        /// Gets the attributes the artifact changes and by how much.
        /// </summary>
        public AttributeEffect[] Modifiers { get; init; } = [];

        /// <summary>
        /// Gets the spells the artifact gives the hero.
        /// </summary>
        public SpellGrantDefinition[] SpellGrants { get; init; } = [];

        /// <summary>
        /// Gets whether the artifact gives the hero the spell book.
        /// </summary>
        public bool GrantsSpellBook { get; init; }

        /// <summary>
        /// Gets the text that tells the other things the artifact does.
        /// </summary>
        public string Other { get; init; } = string.Empty;

        /// <summary>
        /// Gets how much the artifact changes an attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>The amount, or 0 when the artifact does not change the attribute.</returns>
        internal short Amount(ArtifactAttribute attribute) => this.Modifiers.FirstOrDefault(effect => effect.Attribute == attribute)?.Amount ?? 0;
    }
}
