namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Homm2.Data;

    public sealed class Building : CatalogEntity
    {
        internal Building(GameCatalog catalog, BuildingDefinition definition)
            : base(catalog, definition)
        {
            this.Flag = definition.Flag;
            this.Description = definition.Description;
            this.Role = definition.Role;
            this.Growth = definition.Growth;
        }

        public TownBuildings Flag { get; }

        public string Description { get; }

        public BuildingRole Role { get; }

        public int Growth { get; }

        public string NameFor(Faction faction) => this.Role == BuildingRole.Growth ? faction.GrowthBuildingName : this.Role == BuildingRole.Special ? faction.SpecialBuildingName : this.Name;

        public string DescriptionFor(Faction faction) => this.Role == BuildingRole.Special ? faction.SpecialBuildingDescription : this.Description;
    }
}
