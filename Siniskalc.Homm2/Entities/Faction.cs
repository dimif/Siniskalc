namespace Siniskalc.Homm2.Entities
{
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class Faction : CatalogEntity, IFaction
    {
        internal Faction(GameCatalog catalog, FactionDefinition definition)
            : base(catalog, definition)
        {
            this.HasLibrary = definition.Library;
            this.GrowthBuildingName = definition.GrowthBuildingName;
            this.SpecialBuildingName = definition.SpecialBuildingName;
            this.SpecialBuildingDescription = definition.SpecialBuildingDescription;
            this.GuildSpellCounts = System.Array.AsReadOnly((int[])definition.GuildSpellCounts.Clone());
        }

        public bool IsNeutral => this.Id == "Neutral";

        public bool HasLibrary { get; }

        public string GrowthBuildingName { get; }

        public string SpecialBuildingName { get; }

        public string SpecialBuildingDescription { get; }

        public System.Collections.Generic.IReadOnlyList<int> GuildSpellCounts { get; }

        public System.Collections.Generic.IReadOnlyList<Creature> Creatures => this.Catalog.Creatures.Where(c => c.Faction == this).ToList().AsReadOnly();
    }
}
