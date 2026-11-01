namespace Siniskalc.Homm2.Entities
{
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;

    public sealed class Artifact : CatalogEntity, IArtifact
    {
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

        public bool IsSpellBook { get; }

        public bool IsScroll { get; }

        public System.Collections.Generic.IReadOnlyList<AttributeEffect> Effects { get; }

        public PrimarySkillBonus PrimarySkillBonus { get; }

        private int Amount(ArtifactAttribute attribute) => this.Effects.FirstOrDefault(effect => effect.Attribute == attribute)?.Amount ?? 0;
    }
}
