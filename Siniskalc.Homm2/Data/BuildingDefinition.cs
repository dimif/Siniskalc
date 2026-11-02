namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The definition of a building of a town, as it is read from <c>buildings.json</c>.
    /// </summary>
    public sealed record BuildingDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets the bit of the town buildings of a castle that says the building is built.
        /// </summary>
        public required TownBuildings Flag { get; init; }

        /// <summary>
        /// Gets the text that says what the building does.
        /// </summary>
        public required string Description { get; init; }

        /// <summary>
        /// Gets whether the name of the building depends on the faction (the growth building and the special one do).
        /// </summary>
        public BuildingRole Role { get; init; }

        /// <summary>
        /// Gets the creatures the building adds each week to the dwellings.
        /// </summary>
        public required int Growth { get; init; }
    }
}
