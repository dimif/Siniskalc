namespace Siniskalc.Homm2.Entities
{
    /// <summary>
    /// The names of the two buildings that every faction has its own of: the second well and the special building.
    /// </summary>
    public static class TownBuildingNames
    {
        /// <summary>
        /// Returns the name of the second well of the faction (<see cref="TownBuildings.SecondWell"/>).
        /// </summary>
        public static string SecondWellOf(Faction? faction) => faction switch
        {
            Faction.Knight => "Farm",
            Faction.Barbarian => "Garbage Heap",
            Faction.Sorceress => "Crystal Garden",
            Faction.Warlock => "Waterfall",
            Faction.Wizard => "Orchard",
            Faction.Necromancer => "Skull Pile",
            _ => "Second well",
        };

        /// <summary>
        /// Returns the name of the special building of the faction (<see cref="TownBuildings.Special"/>).
        /// </summary>
        public static string SpecialOf(Faction? faction) => faction switch
        {
            Faction.Knight => "Fortifications",
            Faction.Barbarian => "Coliseum",
            Faction.Sorceress => "Rainbow",
            Faction.Warlock => "Dungeon",
            Faction.Wizard => "Library",
            Faction.Necromancer => "Storm",
            _ => "Special building",
        };
    }
}
