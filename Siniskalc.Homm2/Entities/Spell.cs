namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class Spell : CatalogEntity, ISpell
    {
        internal Spell(GameCatalog catalog, SpellDefinition definition)
            : base(catalog, definition)
        {
            this.SpellPoints = definition.SpellPoints;
            this.Type = definition.Type;
            this.Level = definition.Level;
            this.Group = catalog.Groups[definition.Group];
        }

        public int SpellPoints { get; }

        public SpellType Type { get; }

        public int Level { get; }

        public SpellGroup Group { get; }
    }
}
