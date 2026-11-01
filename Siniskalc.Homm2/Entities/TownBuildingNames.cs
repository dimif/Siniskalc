namespace Siniskalc.Homm2.Entities
{
    public static class TownBuildingNames
    {
        public static string SecondWellOf(Faction faction) => faction.GrowthBuildingName;

        public static string SpecialOf(Faction faction) => faction.SpecialBuildingName;
    }
}
