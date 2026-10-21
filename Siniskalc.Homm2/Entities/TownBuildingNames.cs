namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// The names of the two buildings that every faction has its own of: the second well and the special building.
    /// </summary>
    public static class TownBuildingNames
    {
        /// <summary>
        /// Returns the name of the second well of the faction (<see cref="TownBuildings.GrowthBuilding"/>).
        /// </summary>
        private static readonly IReadOnlyDictionary<Faction, string> SecondWellNames = new Dictionary<Faction, string>
        {
            [Faction.Knight] = "Farm",
            [Faction.Barbarian] = "Garbage Heap",
            [Faction.Sorceress] = "Crystal Garden",
            [Faction.Warlock] = "Waterfall",
            [Faction.Wizard] = "Orchard",
            [Faction.Necromancer] = "Skull Pile",
        };

        /// <summary>
        /// Returns the name of the special building of the faction (<see cref="TownBuildings.Special"/>).
        /// </summary>
        private static readonly IReadOnlyDictionary<Faction, string> SpecialNames = new Dictionary<Faction, string>
        {
            [Faction.Knight] = "Fortifications",
            [Faction.Barbarian] = "Coliseum",
            [Faction.Sorceress] = "Rainbow",
            [Faction.Warlock] = "Dungeon",
            [Faction.Wizard] = "Library",
            [Faction.Necromancer] = "Storm",
        };

        public static string SecondWellOf(Faction faction) => SecondWellNames.TryGetValue(faction, out var name) ? name : "Second well";

        public static string SpecialOf(Faction faction) => SpecialNames.TryGetValue(faction, out var name) ? name : "Special building";
    }
}
