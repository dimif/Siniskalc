namespace Siniskalc.Entities
{
    /// <summary>
    /// What a <see cref="Building"/> is for, independent of the faction that has it: the Mage Guild level, the fort, the
    /// n-th creature dwelling and so on. The special structures are numbered in the order of their bits in the building
    /// masks, and a horde building is the one that boosts the growth of a dwelling.
    /// </summary>
    /// <remarks>
    /// The Mage Guild levels, the dwellings and the upgraded dwellings are each in a row of consecutive values, so that
    /// <see cref="Building"/> can turn them into a level by subtracting the first value of the row.
    /// </remarks>
    public enum BuildingType
    {
        MageGuild1,
        MageGuild2,
        MageGuild3,
        MageGuild4,
        MageGuild5,
        Tavern,
        Shipyard,
        ManaGenerator,
        Fort,
        Citadel,
        Castle,
        VillageHall,
        TownHall,
        CityHall,
        Capitol,
        Marketplace,
        ResourceSilo,
        Blacksmith,
        Special1,
        Special2,
        Special3,
        Special4,
        Horde1,
        UpgradedHorde1,
        Horde2,
        UpgradedHorde2,
        Grail,
        Dwelling1,
        Dwelling2,
        Dwelling3,
        Dwelling4,
        Dwelling5,
        Dwelling6,
        Dwelling7,
        UpgradedDwelling1,
        UpgradedDwelling2,
        UpgradedDwelling3,
        UpgradedDwelling4,
        UpgradedDwelling5,
        UpgradedDwelling6,
        UpgradedDwelling7,
    }
}
