namespace Siniskalc.Homm2.Entities
{
    public static class TownBuildingNames
    {
        /// <summary>
        /// Gets the name of the building of a faction that adds to the growth of the first dwelling.
        /// </summary>
        /// <param name="faction">The faction.</param>
        /// <returns>The name.</returns>
        public static string SecondWellOf(Faction faction) => faction.GrowthBuildingName;

        /// <summary>
        /// Gets the name of the special building of a faction.
        /// </summary>
        /// <param name="faction">The faction.</param>
        /// <returns>The name.</returns>
        public static string SpecialOf(Faction faction) => faction.SpecialBuildingName;
    }
}
