namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Data;

    /// <summary>
    /// Where the fields of a castle are in its record, from the start of the record. There are 72 records, and the number of a castle is its
    /// place in the table; the ones the map does not use are empty. A castle's name is at the end of its record.
    /// </summary>
    public static class CastleLayout
    {
        /// <summary>
        /// The size of the record of a castle.
        /// </summary>
        public const int RecordSize = 100;

        /// <summary>
        /// The number of the castle (a byte), which is its place in the table.
        /// </summary>
        public const int Index = 0x00;

        /// <summary>
        /// The colour of the player who owns the castle, or 0xFF.
        /// </summary>
        public const int Owner = 0x01;

        /// <summary>
        /// The faction of the castle, a byte.
        /// </summary>
        public const int Faction = 0x03;

        /// <summary>
        /// The column of the castle on the map (a byte).
        /// </summary>
        public const int X = 0x04;

        /// <summary>
        /// The row of the castle on the map (a byte).
        /// </summary>
        public const int Y = 0x05;

        /// <summary>
        /// Where a ship built in the castle appears (two bytes), or 0xFF when the castle is not on the coast.
        /// </summary>
        public const int Boat = 0x06;

        /// <summary>
        /// The creature types of the garrison, a byte for each of the five slots.
        /// </summary>
        public const int GarrisonTypes = 0x08;

        /// <summary>
        /// The creature counts of the garrison, 16 bits for each of the five slots.
        /// </summary>
        public const int GarrisonCounts = 0x0D;

        /// <summary>
        /// The hero who is in the castle, or 0xFF.
        /// </summary>
        public const int VisitingHero = 0x17;

        /// <summary>
        /// The buildings of the town (16 bits): the Mage Guild, the tavern, the well, the statue and the others, and whether it has a castle.
        /// </summary>
        public const int TownBuildings = 0x18;

        /// <summary>
        /// The dwellings and the level of the Mage Guild (32 bits).
        /// </summary>
        public const int Dwellings = 0x1A;

        /// <summary>
        /// The creatures that wait in each of the six dwellings, 16 bits each.
        /// </summary>
        public const int Creatures = 0x1E;

        /// <summary>
        /// The creatures that wait in the upgraded dwellings 2 to 6 (six u16, the last for the sixth dwelling upgraded twice): an upgraded dwelling keeps its count here and its own slot
        /// above is 0 (seen in the castles of Broken Alliance: Hillstone, Dragontooth, Bloodreign, Woodhaven and Vertigo).
        /// </summary>
        public const int UpgradedCreatures = 0x2A;

        /// <summary>
        /// The place of the list of upgraded creatures the game also fills (with <see cref="SecondUpgradeMarkerValue"/>) for the dragons upgraded
        /// twice: 3 in the saves it made, 0 in E35 where the dragons did not show.
        /// </summary>
        public const int SecondUpgradeMarker = 4;

        /// <summary>
        /// The value the game puts at <see cref="SecondUpgradeMarker"/> when the dragons are upgraded a second time.
        /// </summary>
        public const int SecondUpgradeMarkerValue = 3;

        /// <summary>
        /// The spells of the Mage Guild: four bytes for each of its five levels, each the number of the spell minus one.
        /// </summary>
        public const int GuildSpells = 0x3C;

        /// <summary>
        /// How many spells each level of the Mage Guild offers, a byte each.
        /// </summary>
        public const int GuildSpellCounts = 0x50;

        /// <summary>
        /// The name of the castle, in a field of <see cref="Constants.Castle.NameLength"/> bytes.
        /// </summary>
        public const int Name = 0x57;

        /// <summary>
        /// The garrison: a byte for the type and 16 bits for the count of each of its five slots.
        /// </summary>
        public static ArmyLayout Garrison { get; } = new(GarrisonTypes, 1, GarrisonCounts, 2, Constants.ArmySlotCount, Constants.MaxCreatureCount);

        /// <summary>
        /// Gets where the creatures of dwelling <paramref name="dwelling"/> (1 to 6) wait in the savegame when it is built to
        /// <paramref name="level"/> (0 for not built, 1 for the dwelling, 2 once it is upgraded, 3 for the sixth when it is upgraded twice): in its
        /// own list until it is upgraded, then in the list of the upgraded ones, the dragons upgraded a second time in the sixth place (E34DWUP2:
        /// the Black Dragon had none in the fifth).
        /// </summary>
        /// <param name="dwelling">The dwelling, from 1 to 6.</param>
        /// <param name="level">How far the dwelling is built.</param>
        /// <returns>The offset in the record.</returns>
        public static int CreaturesOffset(int dwelling, int level) =>
            level >= 2 ? UpgradedCreatures + ((level >= 3 ? 5 : dwelling - 2) * 2) : Creatures + ((dwelling - 1) * 2);

        /// <summary>
        /// Gets every place the savegame can keep creatures of dwelling <paramref name="dwelling"/> (1 to 6) in: its own, its upgrade and, for the
        /// sixth, the one of the second upgrade.
        /// </summary>
        /// <param name="dwelling">The dwelling, from 1 to 6.</param>
        /// <returns>The offsets in the record.</returns>
        public static IReadOnlyList<int> CreaturesOffsets(int dwelling) =>
            [Creatures + ((dwelling - 1) * 2), .. dwelling >= 2 ? new[] { UpgradedCreatures + ((dwelling - 2) * 2) } : [], .. dwelling == 6 ? new[] { UpgradedCreatures + (5 * 2) } : []];
    }
}
