namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Records made by hand for the tests of the differs, so that none of them needs a savegame.
    /// </summary>
    internal static class DifferSamples
    {
        public static HeroModel Hero() => new(
            7,
            "Maximus",
            PlayerColor.Blue,
            new HeroPosition(10, 12, MapPosition.Above, FacingDirection.North),
            MapPosition.None,
            new MovementPoints(100, 1500),
            HeroLevel.AtLevel(HeroLevels.Table, 3),
            10,
            new PrimarySkills(2, 3, 4, 5),
            SecondarySkills.None,
            new Army<Creature>(5),
            Faction.Knight,
            Artifacts.None,
            SpellSet<Spell>.None);

        public static PlayerModel Player() => new(
            PlayerColor.Blue,
            new ResourceSet(10, 20, 30, 40, 50, 60, 7000),
            ResourceSet.None,
            null,
            new TavernHeroes(12, 13));

        public static CastleModel Castle() => new(
            "Xabran",
            Faction.Knight,
            PlayerColor.Blue,
            new MapPosition(20, 21),
            null,
            new Army<Creature>(5),
            null,
            Numbers.Of(0, 0, 0, 0, 0, 0),
            3,
            TownBuildings.Castle,
            Dwellings.None,
            Numbers.Of(0, 0, 0, 0, 0),
            new Numbers(new int[20]));
    }
}
