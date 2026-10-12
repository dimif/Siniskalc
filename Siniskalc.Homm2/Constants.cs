namespace Siniskalc.Homm2
{
    /// <summary>
    /// Known constants used in Heroes of Might and Magic II savefiles: the number of things the game keeps, and the limits of the
    /// values the editor lets you set. Every minimum, maximum, step and size of a value lives here, so that the steppers, the "Max"
    /// buttons and the checks (see <see cref="Siniskalc.Core.RangeGuard"/>) agree.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The byte that marks an empty slot or "nobody" in the tables of the savefile.
        /// </summary>
        public const byte Empty = 0xFF;

        /// <summary>
        /// The size of the army of a hero or of a castle.
        /// </summary>
        public const int ArmySlotCount = 5;

        /// <summary>
        /// The most creatures a stack holds in the editor (a stack is a 16-bit number in the savefile).
        /// </summary>
        public const long MaxCreatureCount = 65_535;

        /// <summary>
        /// What is kept in a savefile: how many heroes, players and castles it has, whatever the map.
        /// </summary>
        public static class Table
        {
            public const int HeroCount = 54;
            public const int PlayerCount = 6;
            public const int CastleSlotCount = 72;
        }

        /// <summary>
        /// Limits of the values of a hero.
        /// </summary>
        public static class Hero
        {
            public const int ArtifactSlotCount = 14;
            public const int SecondarySkillCount = 14;
            public const int MaxSecondarySkills = 8;
            public const int SpellCount = 65;

            public const long MinLevel = 1;
            public const long MaxLevel = 255;

            public const long MinExperience = 0;
            public const long MaxExperience = 2_000_000_000;
            public const long ExperienceStep = 100;

            public const long MinPrimarySkill = 0;
            public const long MaxPrimarySkill = 255;

            public const long MinSpellPoints = 0;
            public const long MaxSpellPoints = 65_535;

            public const long MinMovementPoints = 0;
            public const long MaxMovementPoints = 99_999;
            public const long MovementStep = 100;
        }

        /// <summary>
        /// Limits of the values of a player.
        /// </summary>
        public static class Player
        {
            public const int HeroListSize = 5;
            public const int TavernSize = 2;
            public const int CastleListSize = 8;
            public const int ResourceCount = 7;

            public const long MinResource = 0;
            public const long MaxResource = 2_000_000_000;

            /// <summary>
            /// The most the editor gives a player. The savegame holds 32 bits, but the game's screens are not made for more than this.
            /// </summary>
            public const long MaxGold = 999_999;
            public const long MaxOther = 999;
            public const long GoldStep = 1000;
            public const long OtherStep = 1;

            /// <summary>
            /// What the "All +99" and "Gold +100k" buttons add.
            /// </summary>
            public const int AllResourcesIncrease = 99;
            public const int GoldIncrease = 100_000;
        }

        /// <summary>
        /// Limits of the values of a castle.
        /// </summary>
        public static class Castle
        {
            public const int DwellingCount = 6;
            public const int MageGuildLevels = 5;
            public const int MageGuildSlotsPerLevel = 4;
            public const int NameLength = 13;

            public const long MinCreatures = 0;
            public const long MaxCreatures = 65_535;
        }
    }
}
