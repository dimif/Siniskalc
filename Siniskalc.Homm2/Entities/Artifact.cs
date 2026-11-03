namespace Siniskalc.Homm2.Entities
{
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;

    public sealed class Artifact : CatalogEntity, IArtifact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Artifact"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal Artifact(GameCatalog catalog, ArtifactDefinition definition)
            : base(catalog, definition, definition.AvailableFrom)
        {
            this.IsSpellBook = definition.Role == ArtifactRole.SpellBook;
            this.IsScroll = definition.Role == ArtifactRole.Scroll;
            this.Effects = System.Array.AsReadOnly(definition.EffectInfo.Modifiers.ToArray());
            this.PrimarySkillBonus = new(
                this.Amount(ArtifactAttribute.Attack),
                this.Amount(ArtifactAttribute.Defense),
                this.Amount(ArtifactAttribute.SpellPower),
                this.Amount(ArtifactAttribute.Knowledge));
        }

        /// <summary>
        /// Gets whether the artifact is the spell book.
        /// </summary>
        public bool IsSpellBook { get; }

        /// <summary>
        /// Gets whether the artifact is a spell scroll.
        /// </summary>
        public bool IsScroll { get; }

        /// <summary>
        /// Gets the attributes the artifact changes and by how much.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<AttributeEffect> Effects { get; }

        /// <summary>
        /// Gets what the artifact adds to the four primary skills.
        /// </summary>
        public PrimarySkillBonus PrimarySkillBonus { get; }

        private int Amount(ArtifactAttribute attribute) => this.Effects.FirstOrDefault(effect => effect.Attribute == attribute)?.Amount ?? 0;
    }
}
