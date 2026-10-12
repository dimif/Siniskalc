namespace Siniskalc
{
    /// <summary>
    /// Known constants used in Heroes 3 savefiles, and the limits of the values the editor lets you set.
    /// Every minimum, maximum, step and size of a value lives here, so that the steppers, the "Max" buttons and the
    /// checks (see <see cref="RangeGuard"/>) agree.
    /// </summary>
    public static class Constants
    {
        public const byte ByteOn = 0;
        public const byte ByteOff = 255;

        /// <summary>
        /// A 4-byte id with every byte set to <see cref="ByteOff"/>, marking an empty slot.
        /// </summary>
        public const uint BlankId = uint.MaxValue;

        public const int CreatureSlotCount = 7;
        public const int BackpackSlotCount = 64;
        public const int BackpackSlotSize = 8;

        /// <summary>
        /// Where the project lives on the web.
        /// </summary>
        public static class App
        {
            public const string Name = "Siniskalc";
            public const string RepositoryUrl = "https://github.com/dimif/Siniskalc";
            public const string ReleasesUrl = RepositoryUrl + "/releases";
            public const string IssuesUrl = RepositoryUrl + "/issues";
            public const string UserGuideUrl = RepositoryUrl + "/blob/main/docs/USER_GUIDE.md";
        }

        /// <summary>
        /// Limits of the values of a hero.
        /// </summary>
        public static class Hero
        {
            /// <summary>
            /// The most morale or luck a hero has, and the least it has the negative of.
            /// </summary>
            public const int MaxMoraleOrLuck = 3;

            public const long MinLevel = 1;
            public const long MaxLevel = 75;

            public const long MinExperience = 0;
            public const long MaxExperience = 2_000_000_000;
            public const long ExperienceStep = 1000;

            public const long MinPrimarySkill = 0;
            public const long MaxPrimarySkill = 99;

            public const long MinMovementPoints = 0;
            public const long MaxMovementPoints = 99_999;
            public const long MovementStep = 100;

            public const long MinSpellPoints = 0;
            public const long MaxSpellPoints = 999;
            public const long SpellPointsStep = 10;
        }

        /// <summary>
        /// Limits of the secondary skill slots of a hero.
        /// </summary>
        public static class SkillSlots
        {
            /// <summary>
            /// The most skills the hero screen shows.
            /// </summary>
            public const int Max = 8;
        }

        /// <summary>
        /// Limits of the map and of positions on it.
        /// </summary>
        public static class Map
        {
            /// <summary>
            /// Written as the coordinate of something that is not on the map.
            /// </summary>
            public const long NoCoordinate = -1;

            public const long MinCoordinate = NoCoordinate;

            /// <summary>
            /// The biggest map is 252 squares wide, so the last coordinate is 251.
            /// </summary>
            public const long MaxCoordinate = MaxSize - 1;

            public const int MaxSize = 252;
            public const int MaxDifficulty = 4;
        }

        /// <summary>
        /// Limits of an army or garrison.
        /// </summary>
        public static class Army
        {
            public const int MinSlotNumber = 1;
            public const int MinStackSize = 1;
            public const int MaxStackSize = 99_999;
        }

        public static class Backpack
        {
            /// <summary>
            /// The backpack is shown in two pages of this many slots.
            /// </summary>
            public const int SlotsPerPage = 32;
        }

        /// <summary>
        /// Limits of the resources of a player.
        /// </summary>
        public static class Resources
        {
            public const long Min = 0;

            /// <summary>
            /// The most gold a player can be given here. The game itself shows large amounts, but not beyond this.
            /// </summary>
            public const long MaxGold = 999_999_999;
            public const long MaxOther = 999_999;
            public const long GoldStep = 1000;
            public const long OtherStep = 1;

            /// <summary>
            /// What the "all resources" and "rich" presets give.
            /// </summary>
            public const int AllResourcesPreset = 99;
            public const int RichGoldPreset = 1_000_000;

            /// <summary>
            /// What the "All +99" and "Gold +100k" buttons add.
            /// </summary>
            public const int AllResourcesIncrease = 99;
            public const int GoldIncrease = 100_000;
        }

        /// <summary>
        /// Limits of the obelisks of a map.
        /// </summary>
        public static class Obelisks
        {
            /// <summary>
            /// The most obelisks a map can have: one for each piece of the puzzle that shows where the Grail is.
            /// </summary>
            public const int Max = 48;
        }

        /// <summary>
        /// Limits of what a player's record holds apart from its resources.
        /// </summary>
        public static class Player
        {
            /// <summary>
            /// The number of colours, which is the number of players a savefile holds a struct for.
            /// </summary>
            public const int ColorCount = 8;

            /// <summary>
            /// A hero id is a byte and FF stands for none, so the highest id is 254. The savefiles have 156 heroes (215 in Horn of the Abyss).
            /// </summary>
            public const int MinHeroId = 0;
            public const int MaxHeroId = 254;
        }

        /// <summary>
        /// Limits of a castle.
        /// </summary>
        public static class Castle
        {
            public const long MinDwellingCreatures = 0;

            /// <summary>
            /// A dwelling stores the creatures it has in 16 bits.
            /// </summary>
            public const long MaxDwellingCreatures = ushort.MaxValue;
        }
    }
}
