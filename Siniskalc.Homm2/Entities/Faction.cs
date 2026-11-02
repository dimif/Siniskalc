namespace Siniskalc.Homm2.Entities
{
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    public sealed class Faction : CatalogEntity, IFaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Faction"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal Faction(GameCatalog catalog, FactionDefinition definition)
            : base(catalog, definition)
        {
            this.HasLibrary = definition.Library;
            this.GrowthBuildingName = definition.GrowthBuildingName;
            this.SpecialBuildingName = definition.SpecialBuildingName;
            this.SpecialBuildingDescription = definition.SpecialBuildingDescription;
            this.GuildSpellCounts = System.Array.AsReadOnly((int[])definition.GuildSpellCounts.Clone());
        }

        /// <summary>
        /// Gets whether this is the faction of a castle that has no known faction.
        /// </summary>
        public bool IsNeutral => this.Id == "Neutral";

        /// <summary>
        /// Gets whether the castle of the faction can build a Library.
        /// </summary>
        public bool HasLibrary { get; }

        /// <summary>
        /// Gets the name of the building of the faction that adds to the growth of the first dwelling.
        /// </summary>
        public string GrowthBuildingName { get; }

        /// <summary>
        /// Gets the name of the special building of the faction.
        /// </summary>
        public string SpecialBuildingName { get; }

        /// <summary>
        /// Gets what the special building of the faction does.
        /// </summary>
        public string SpecialBuildingDescription { get; }

        /// <summary>
        /// Gets how many spells each level of the Mage Guild of the faction offers.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<int> GuildSpellCounts { get; }

        /// <summary>
        /// Gets the creatures of the faction.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<Creature> Creatures => this.Catalog.Creatures.Where(c => c.Faction == this).ToList().AsReadOnly();
    }
}
