namespace Siniskalc.Homm2.Data
{
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

        public const int HeaderId = 2;

        /// <summary>
        /// The colour of the player the hero is in the service of, or 0xFF.
        /// </summary>
        public const int Owner = 3;

        // The body. Offsets are from the start of the name, which is 10 bytes into the record.

        public const int Name = 10;
        public const int NameLength = 14;

        public const int Id = Name + 0x0E;
        public const int X = Name + 0x0F;
        public const int Y = Name + 0x13;

        /// <summary>
        /// Where the hero was going when its movement ran out (two 32-bit numbers), or all bytes 0xFF.
        /// </summary>
        public const int Destination = Name + 0x17;

        public const int MaxMovement = Name + 0x27;
        public const int MovementLeft = Name + 0x2B;
        public const int Experience = Name + 0x2F;
        public const int Level = Name + 0x33;

        /// <summary>
        /// Attack, defense, spell power and knowledge, a byte each.
        /// </summary>
        public const int PrimarySkills = Name + 0x35;

        public const int ArmyTypes = Name + 0x5B;
        public const int ArmyCounts = Name + 0x60;

        /// <summary>
        /// The level of each secondary skill, a byte each, by the number of the skill.
        /// </summary>
        public const int SkillLevels = Name + 0x6A;

        /// <summary>
        /// The place in which each secondary skill was learned (1 for the first), a byte each, by the number of the skill.
        /// </summary>
        public const int SkillOrder = Name + 0x78;

        public const int SkillCount = Name + 0x86;

        /// <summary>
        /// One byte, 0 or 1, for each spell by its number (so the first byte is not used): 66 bytes.
        /// </summary>
        public const int Spells = Name + 0x89;
        public const int SpellsLength = 66;

        public const int Artifacts = Name + 0xCB;

        /// <summary>
        /// The size of the part that is the same in both versions of the game.
        /// </summary>
        public const int SharedLength = Name + 0xD9;
    }
}
