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
        public const long MaxCreatureCount = 32_767;

        /// <summary>
        /// The most a savegame name can have before its extension: the game lists no longer ones.
        /// </summary>
        public const int MaxSavegameNameLength = 8;

        /// <summary>
        /// What is kept in a savefile: how many heroes, players and castles it has, whatever the map.
        /// </summary>
        public static class Table
        {
            /// <summary>
            /// The largest map of the game is this many tiles wide.
            /// </summary>
            public const int MaxMapSize = 144;

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

            /// <summary>
            /// What the places of the map give a hero for its next battle was seen from -1 to 2; the game shows a total from -3 to 3.
            /// </summary>
            public const long MinBattleBonus = -3;
            public const long MaxBattleBonus = 3;
            public const int SpellCount = 65;

            /// <summary>
            /// The heroes of each faction are numbered together, this many at a time.
            /// </summary>
            public const int PerFaction = 9;

            public const long MinCoordinate = 0;
            public const long MaxCoordinate = int.MaxValue;

            public const long MinLevel = 1;
            public const long MaxLevel = 255;

            public const long MinExperience = 0;
            public const long MaxExperience = 2_000_000_000;
            public const long ExperienceStep = 100;

            public const long MinPrimarySkill = 0;
            public const long MaxPrimarySkill = 255;

            public const long MinSpellPoints = 0;
            public const long MaxSpellPoints = 65_535;

            /// <summary>
            /// The spell points a hero can have for each point of knowledge.
            /// </summary>
            public const int SpellPointsPerKnowledge = 10;

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
            public const long MaxResource = Heroes.Constants.Resources.MaxGold;

            /// <summary>
            /// The most the editor gives a player (the same in every game of the series). The savegame holds 32 bits; the game took well over 1,000,000 gold and over 1,000 of the other resources (save E23LIMIT, where the next turn went past them).
            /// </summary>
            public const long MaxGold = Heroes.Constants.Resources.MaxGold;
            public const long MaxOther = Heroes.Constants.Resources.MaxOther;
            public const long GoldStep = 1000;
            public const long OtherStep = 1;

            /// <summary>
            /// What the "Res +100" and "Gold +100k" buttons add.
            /// </summary>
            public const int AllResourcesIncrease = 100;
            public const int GoldIncrease = 100_000;

            /// <summary>
            /// What the "+10" button of a resource and the "+10k" button of gold add.
            /// </summary>
            public const int OtherPreset = 10;
            public const int GoldPreset = 10_000;
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

            public const long MinCoordinate = 0;
            public const long MaxCoordinate = byte.MaxValue;

            public const long MinGuildSpellCount = 0;
            public const long MaxGuildSpellCount = MageGuildSlotsPerLevel;

            /// <summary>
            /// The buildings of the town are 16 bits in the savegame.
            /// </summary>
            public const long MaxTownBuildings = ushort.MaxValue;

            public const long MinCreatures = 0;
            public const long MaxCreatures = 65_535;

            /// <summary>
            /// The most creatures the editor lets wait in a dwelling. The savegame holds more, but the game was only seen with a few dozen.
            /// </summary>
            public const long MaxWaiting = 999;

            /// <summary>
            /// The creatures the Well adds each week to every dwelling that is built.
            /// </summary>
            public const int WellGrowth = 2;

            /// <summary>
            /// The creatures the second well of the faction (Farm, Skull Pile and so on) adds each week to the first dwelling only.
            /// </summary>
            public const int SecondWellGrowth = 8;
        }
    }
}
