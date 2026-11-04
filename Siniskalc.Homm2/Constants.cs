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

            /// <summary>
            /// The number of hero records in a savefile, whatever the map.
            /// </summary>
            public const int HeroCount = 54;

            /// <summary>
            /// The number of player records in a savefile, one for each colour whether it plays or not.
            /// </summary>
            public const int PlayerCount = 6;

            /// <summary>
            /// The number of castle records in a savefile; the ones the map does not use are empty.
            /// </summary>
            public const int CastleSlotCount = 72;
        }

        /// <summary>
        /// Limits of the values of a hero.
        /// </summary>
        public static class Hero
        {
            /// <summary>
            /// The number of artifact slots of a hero.
            /// </summary>
            public const int ArtifactSlotCount = 14;

            /// <summary>
            /// The number of secondary skills the game has, which is the length of the tables of skill levels of a hero.
            /// </summary>
            public const int SecondarySkillCount = 14;

            /// <summary>
            /// The most secondary skills a hero can know at once.
            /// </summary>
            public const int MaxSecondarySkills = 8;

            /// <summary>
            /// What the places of the map give a hero for its next battle was seen from -1 to 2; the game shows a total from -3 to 3.
            /// </summary>
            public const long MinBattleBonus = -3;

            /// <summary>
            /// The greatest total of morale or luck the places of the map give a hero for its next battle.
            /// </summary>
            public const long MaxBattleBonus = 3;

            /// <summary>
            /// The number of spells the game has.
            /// </summary>
            public const int SpellCount = 65;

            /// <summary>
            /// The heroes of each faction are numbered together, this many at a time.
            /// </summary>
            public const int PerFaction = 9;

            /// <summary>
            /// The least coordinate of a hero on the map.
            /// </summary>
            public const long MinCoordinate = 0;

            /// <summary>
            /// The greatest coordinate of a hero on the map (a coordinate is a 32-bit number in the savefile).
            /// </summary>
            public const long MaxCoordinate = int.MaxValue;

            /// <summary>
            /// The least level of a hero.
            /// </summary>
            public const long MinLevel = 1;

            /// <summary>
            /// The greatest level of a hero (a byte in the savefile).
            /// </summary>
            public const long MaxLevel = 255;

            /// <summary>
            /// The least experience of a hero.
            /// </summary>
            public const long MinExperience = 0;

            /// <summary>
            /// The most experience the editor gives a hero.
            /// </summary>
            public const long MaxExperience = 2_000_000_000;

            /// <summary>
            /// The step of the experience stepper of a hero.
            /// </summary>
            public const long ExperienceStep = 100;

            /// <summary>
            /// The least value of a primary skill of a hero.
            /// </summary>
            public const long MinPrimarySkill = 0;

            /// <summary>
            /// The greatest value of a primary skill of a hero (a byte in the savefile).
            /// </summary>
            public const long MaxPrimarySkill = 255;

            /// <summary>
            /// The least spell points of a hero.
            /// </summary>
            public const long MinSpellPoints = 0;

            /// <summary>
            /// The greatest spell points the editor gives a hero. The savefile holds 16 bits, but the game shows 32,767 and 65,535 as -1.
            /// </summary>
            public const long MaxSpellPoints = 32_766;

            /// <summary>
            /// The spell points a hero can have for each point of knowledge.
            /// </summary>
            public const int SpellPointsPerKnowledge = 10;

            /// <summary>
            /// The least movement points of a hero.
            /// </summary>
            public const long MinMovementPoints = 0;

            /// <summary>
            /// The most movement points the editor gives a hero.
            /// </summary>
            public const long MaxMovementPoints = 99_999;

            /// <summary>
            /// The step of the movement stepper of a hero.
            /// </summary>
            public const long MovementStep = 100;

            /// <summary>
            /// The most places of a kind a hero has a bit for: the set of a kind is a 32 bit number and its bit 0 is not used, so the first place of
            /// a kind is bit 1 and a place after the 31st has none.
            /// </summary>
            public const int MaxVisitedPlacesPerKind = 31;
        }

        /// <summary>
        /// Limits of the values of a player.
        /// </summary>
        public static class Player
        {
            /// <summary>
            /// The most heroes a player can have.
            /// </summary>
            public const int HeroListSize = 5;

            /// <summary>
            /// The number of heroes the tavern offers a player.
            /// </summary>
            public const int TavernSize = 2;

            /// <summary>
            /// The most castles a player can have.
            /// </summary>
            public const int CastleListSize = 8;

            /// <summary>
            /// The number of resources a player keeps: wood, mercury, ore, sulfur, crystal, gems and gold.
            /// </summary>
            public const int ResourceCount = 7;

            /// <summary>
            /// The greatest stock of a resource the editor gives a player.
            /// </summary>
            public const long MaxResource = Heroes.Constants.Resources.MaxGold;

            /// <summary>
            /// The most the editor gives a player (the same in every game of the series). The savegame holds 32 bits; the game took well over 1,000,000 gold and over 1,000 of the other resources (save E23LIMIT, where the next turn went past them).
            /// </summary>
            public const long MaxGold = Heroes.Constants.Resources.MaxGold;

            /// <summary>
            /// The greatest stock of a resource other than gold the editor gives a player.
            /// </summary>
            public const long MaxOther = Heroes.Constants.Resources.MaxOther;
        }

        /// <summary>
        /// Limits of the values of a castle.
        /// </summary>
        public static class Castle
        {
            /// <summary>
            /// The number of dwellings of a castle.
            /// </summary>
            public const int DwellingCount = 6;

            /// <summary>
            /// The number of levels of a Mage Guild.
            /// </summary>
            public const int MageGuildLevels = 5;

            /// <summary>
            /// The number of spells a level of the Mage Guild can offer.
            /// </summary>
            public const int MageGuildSlotsPerLevel = 4;

            /// <summary>
            /// The length of the field of the name of a castle in the savefile.
            /// </summary>
            public const int NameLength = 13;

            /// <summary>
            /// The least coordinate of a castle on the map.
            /// </summary>
            public const long MinCoordinate = 0;

            /// <summary>
            /// The greatest coordinate of a castle on the map (a byte in the savefile).
            /// </summary>
            public const long MaxCoordinate = byte.MaxValue;

            /// <summary>
            /// The least number of spells a level of the Mage Guild offers.
            /// </summary>
            public const long MinGuildSpellCount = 0;

            /// <summary>
            /// The most spells a level of the Mage Guild offers.
            /// </summary>
            public const long MaxGuildSpellCount = MageGuildSlotsPerLevel;

            /// <summary>
            /// The buildings of the town are 16 bits in the savegame.
            /// </summary>
            public const long MaxTownBuildings = ushort.MaxValue;

            /// <summary>
            /// The least creatures that wait in a dwelling.
            /// </summary>
            public const long MinCreatures = 0;

            /// <summary>
            /// The most creatures a dwelling holds in the savefile (16 bits).
            /// </summary>
            public const long MaxCreatures = 65_535;

            /// <summary>
            /// The most creatures the editor lets wait in a dwelling: what the savegame holds (16 bits), the same as the most a stack can hold, so
            /// both can be tried at the limit. The game was only seen with a few dozen.
            /// </summary>
            public const long MaxWaiting = MaxCreatures;

            /// <summary>
            /// The creatures the Well adds each week to every dwelling that is built.
            /// </summary>
            public const int WellGrowth = 2;

            /// <summary>
            /// The creatures the second well of the faction (Farm, Skull Pile and so on) adds each week to the first dwelling only.
            /// </summary>
            public const int SecondWellGrowth = 8;
        }

        /// <summary>
        /// Limits of the values of a sphinx.
        /// </summary>
        public static class Sphinx
        {
            /// <summary>
            /// The most answers a sphinx accepts.
            /// </summary>
            public const int MaxAnswers = 8;

            /// <summary>
            /// The most characters an answer of a sphinx has.
            /// </summary>
            public const int MaxAnswerLength = 12;
        }
    }
}
