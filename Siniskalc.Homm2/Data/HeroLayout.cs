namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Data;

    /// <summary>
    /// Where the fields of a hero are in its record, from the start of the record (which is where its header is). The record is 236 bytes
    /// in the base game and 250 in the Price of Loyalty, which adds 14 bytes at its end that the editor leaves as they are.
    /// </summary>
    public static class HeroLayout
    {
        /// <summary>
        /// The size of the record of the base game.
        /// </summary>
        public const int BaseRecordSize = 236;

        /// <summary>
        /// The size of the record of the Price of Loyalty.
        /// </summary>
        public const int ExpansionRecordSize = 250;

        // The header: the fields that come before the name, which belong to the hero that follows.

        /// <summary>
        /// The spell points the hero has now (16 bits). The most it can have is its knowledge times 10, which is not kept.
        /// </summary>
        public const int SpellPoints = 0;

        /// <summary>
        /// The number of the hero that follows, in the header of the record.
        /// </summary>
        public const int HeaderId = 2;

        /// <summary>
        /// The colour of the player the hero is in the service of, or 0xFF.
        /// </summary>
        public const int Owner = 3;

        // The body. Offsets are from the start of the name, which is 10 bytes into the record.
        /// <summary>
        /// The name of the hero, from the start of the record; the offsets of the body are from here.
        /// </summary>
        public const int Name = 10;

        /// <summary>
        /// The length of the field of the name.
        /// </summary>
        public const int NameLength = 14;

        /// <summary>
        /// The number of the hero (a byte).
        /// </summary>
        public const int Id = Name + 0x0E;

        /// <summary>
        /// The column of the hero on the map (32 bits).
        /// </summary>
        public const int X = Name + 0x0F;

        /// <summary>
        /// The row of the hero on the map (32 bits).
        /// </summary>
        public const int Y = Name + 0x13;

        /// <summary>
        /// Where the hero was going when its movement ran out (two 32-bit numbers), or all bytes 0xFF.
        /// </summary>
        public const int Destination = Name + 0x17;

        /// <summary>
        /// The direction the hero faces, a byte from 0 to 7 clockwise from north (0 north, 1 northeast, 2 east, 3 southeast, 4 south,
        /// 5 southwest, 6 west, 7 northwest): the way of its last step. Found in the MarkerFacing saves.
        /// </summary>
        public const int Facing = Name + 0x22;

        /// <summary>
        /// The byte 9 of the tile the hero stands on (what the tile is: 0x99 for an obelisk, 0xA3 the gate of a castle...). The table of tiles
        /// holds a marker on the tile of a hero instead, so this is where the tile's own byte is kept while the hero is on it.
        /// </summary>
        public const int TileObject = Name + 0x23;

        /// <summary>
        /// The movement points the hero has at the start of a day (32 bits).
        /// </summary>
        public const int MaxMovement = Name + 0x27;

        /// <summary>
        /// The movement points the hero has left today (32 bits).
        /// </summary>
        public const int MovementLeft = Name + 0x2B;

        /// <summary>
        /// The experience of the hero (32 bits).
        /// </summary>
        public const int Experience = Name + 0x2F;

        /// <summary>
        /// The level of the hero.
        /// </summary>
        public const int Level = Name + 0x33;

        /// <summary>
        /// The morale (a signed byte) and the luck (the byte after it) the hero has for its next battle only (Threedom: a temple gave 2, a fountain 1).
        /// </summary>
        public const int BattleMorale = Name + 0x3A;

        /// <summary>
        /// The luck the hero has for its next battle only, a signed byte; see <see cref="BattleMorale"/>.
        /// </summary>
        public const int BattleLuck = Name + 0x3B;

        /// <summary>
        /// Five 32 bit sets of the places the hero has visited, each a bit for a place of its kind: gazebos, forts, witch doctor's huts,
        /// mercenary camps and standing stones (Threedom).
        /// </summary>
        public const int VisitedPlaces = Name + 0x3D;

        /// <summary>
        /// The set of the places called Xanadu the hero has visited (the sixth after the five of <see cref="VisitedPlaces"/>; the one in
        /// front of it, at +0x51, was never set in the saves).
        /// </summary>
        public const int VisitedXanadu = Name + 0x55;

        /// <summary>
        /// 32 bits of what the hero remembers of the fountain, oasis, faerie ring, graveyard and temple it visited (cleared with their bonus),
        /// the arena and the stables (<see cref="Entities.HeroEffects"/>). The Price of Loyalty adds bytes after them.
        /// </summary>
        public const int Effects = Name + 0xD9;

        /// <summary>
        /// Attack, defense, spell power and knowledge, a byte each.
        /// </summary>
        public const int PrimarySkills = Name + 0x35;

        /// <summary>
        /// The creature types of the army, a byte for each of the five slots.
        /// </summary>
        public const int ArmyTypes = Name + 0x5B;

        /// <summary>
        /// The creature counts of the army, 16 bits for each of the five slots.
        /// </summary>
        public const int ArmyCounts = Name + 0x60;

        /// <summary>
        /// The level of each secondary skill, a byte each, by the number of the skill.
        /// </summary>
        public const int SkillLevels = Name + 0x6A;

        /// <summary>
        /// The place in which each secondary skill was learned (1 for the first), a byte each, by the number of the skill.
        /// </summary>
        public const int SkillOrder = Name + 0x78;

        /// <summary>
        /// The number of secondary skills the hero knows, a byte.
        /// </summary>
        public const int SkillCount = Name + 0x86;

        /// <summary>
        /// One byte, 0 or 1, for each spell by its number (so the first byte is not used): 66 bytes.
        /// </summary>
        public const int Spells = Name + 0x89;

        /// <summary>
        /// The length of the table of spells.
        /// </summary>
        public const int SpellsLength = 66;

        /// <summary>
        /// The artifacts the hero carries, a byte for each of its slots.
        /// </summary>
        public const int Artifacts = Name + 0xCB;

        /// <summary>
        /// The size of the part that is the same in both versions of the game.
        /// </summary>
        public const int SharedLength = Name + 0xD9;

        /// <summary>
        /// The army: a byte for the type and 16 bits for the count of each of its five slots.
        /// </summary>
        public static ArmyLayout Army { get; } = new(ArmyTypes, 1, ArmyCounts, 2, Constants.ArmySlotCount, Constants.MaxCreatureCount);
    }
}
