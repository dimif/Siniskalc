namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Homm2.Data;

    public sealed class Building : CatalogEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="definition">The definition.</param>
        internal Building(GameCatalog catalog, BuildingDefinition definition)
            : base(catalog, definition)
        {
            this.Flag = definition.Flag;
            this.Description = definition.Description;
            this.Role = definition.Role;
            this.Growth = definition.Growth;
        }

        /// <summary>
        /// Gets the bit of the town buildings of a castle that says the building is built.
        /// </summary>
        public TownBuildings Flag { get; }

        /// <summary>
        /// Gets the text that says what the building does.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets whether the name of the building depends on the faction.
        /// </summary>
        public BuildingRole Role { get; }

        /// <summary>
        /// Gets the creatures the building adds each week to the dwellings.
        /// </summary>
        public int Growth { get; }

        /// <summary>
        /// Gets the name of the building in a faction.
        /// </summary>
        /// <param name="faction">The faction of the castle.</param>
        /// <returns>The name of the faction's own building for the growth and special roles, the name of the building for the others.</returns>
        public string NameFor(Faction faction) => this.Role == BuildingRole.Growth ? faction.GrowthBuildingName : this.Role == BuildingRole.Special ? faction.SpecialBuildingName : this.Name;

        /// <summary>
        /// Gets what the building does in a faction.
        /// </summary>
        /// <param name="faction">The faction of the castle.</param>
        /// <returns>The description of the faction's special building for the special role, the description of the building for the others.</returns>
        public string DescriptionFor(Faction faction) => this.Role == BuildingRole.Special ? faction.SpecialBuildingDescription : this.Description;
    }
}
