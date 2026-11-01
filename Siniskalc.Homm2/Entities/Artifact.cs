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
            this.Effects = System.Array.AsReadOnly(definition.Effects.Select(id => catalog.Effects[id]).ToArray());
            this.PrimarySkillBonus = this.Effects.Aggregate(PrimarySkillBonus.None, (sum, effect) => sum + effect.Bonus);
        }

        public bool IsSpellBook { get; }

        public bool IsScroll { get; }

        public System.Collections.Generic.IReadOnlyList<ArtifactEffect> Effects { get; }

        public PrimarySkillBonus PrimarySkillBonus { get; }
    }
}
