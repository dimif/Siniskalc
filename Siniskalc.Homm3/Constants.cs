namespace Siniskalc.Homm3
{
    /// <summary>
    /// Known constants used in Heroes 3 savefiles, and the limits of the values the editor lets you set.
    /// Every minimum, maximum, step and size of a value lives here, so that the steppers, the "Max" buttons and the
    /// checks (see <see cref="RangeGuard"/>) agree.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The byte 0, which the savefile holds where something is present: the code of the spell book and the flag of a hero that has it.
        /// </summary>
        public const byte ByteOn = 0;

        /// <summary>
        /// The byte 0xFF, which the savefile holds for an empty slot or for nobody.
        /// </summary>
        public const byte ByteOff = 255;

        /// <summary>
        /// A 4-byte id with every byte set to <see cref="ByteOff"/>, marking an empty slot.
        /// </summary>
        public const uint BlankId = uint.MaxValue;

        /// <summary>
        /// The number of slots of an army or garrison.
        /// </summary>
        public const int CreatureSlotCount = 7;

        /// <summary>
        /// The number of slots of the backpack of a hero.
        /// </summary>
        public const int BackpackSlotCount = 64;

        /// <summary>
        /// The size in the savefile of a slot of the backpack.
        /// </summary>
        public const int BackpackSlotSize = 8;

        public static class Catalog
        {
            /// <summary>
            /// The number of spell codes a savefile has room for; a spell of the catalog has a code below it.
            /// </summary>
            public const int SpellCapacity = 70;

            /// <summary>
            /// The number of secondary skill codes a savefile has room for; a skill of the catalog has a code below it.
            /// </summary>
            public const int SkillCapacity = 30;

            /// <summary>
            /// The number of kinds of places a hero can visit that the catalog and the savefile have room for.
            /// </summary>
            public const int PlaceCapacity = 12;
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

            /// <summary>
            /// The least level of a hero.
            /// </summary>
            public const long MinLevel = 1;

            /// <summary>
            /// The greatest level the game gives a hero.
            /// </summary>
            public const long MaxLevel = 75;

            /// <summary>
            /// The level is a byte in the savefile, which holds more than the game gives a hero.
            /// </summary>
            public const long MaxStoredLevel = byte.MaxValue;

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
            public const long ExperienceStep = 1000;

            /// <summary>
            /// The least value of a primary skill of a hero.
            /// </summary>
            public const long MinPrimarySkill = 0;

            /// <summary>
            /// The greatest value of a primary skill of a hero that the editor lets you set.
            /// </summary>
            public const long MaxPrimarySkill = 99;

            /// <summary>
            /// The greatest value of a primary skill that the savefile holds.
            /// </summary>
            public const long MaxSavedPrimarySkill = 255;

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
            /// The least spell points of a hero.
            /// </summary>
            public const long MinSpellPoints = 0;

            /// <summary>
            /// The most spell points the editor gives a hero.
            /// </summary>
            public const long MaxSpellPoints = 32_766;

            /// <summary>
            /// The step of the spell points stepper of a hero.
            /// </summary>
            public const long SpellPointsStep = 10;

            /// <summary>
            /// The spell points a hero can have for each point of knowledge, before what Intelligence adds.
            /// </summary>
            public const int SpellPointsPerKnowledge = 10;
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
            public const long NoCoordinate = Heroes.Constants.Map.NoCoordinate;

            /// <summary>
            /// The least coordinate of something on the map, which is the value written for something that is not on it.
            /// </summary>
            public const long MinCoordinate = NoCoordinate;

            /// <summary>
            /// The biggest map is 252 squares wide, so the last coordinate is 251.
            /// </summary>
            public const long MaxCoordinate = MaxSize - 1;

            /// <summary>
            /// The greatest width of a map, in squares.
            /// </summary>
            public const int MaxSize = 252;

            /// <summary>
            /// The greatest difficulty a map can be made for.
            /// </summary>
            public const int MaxDifficulty = 4;
        }

        /// <summary>
        /// Limits of an army or garrison.
        /// </summary>
        public static class Army
        {
            /// <summary>
            /// The number of the first slot of an army, as the editor shows it.
            /// </summary>
            public const int MinSlotNumber = 1;

            /// <summary>
            /// The least creatures a stack holds.
            /// </summary>
            public const int MinStackSize = 1;

            /// <summary>
            /// The most creatures a stack holds in the editor.
            /// </summary>
            public const int MaxStackSize = 99_999;
        }

        /// <summary>
        /// Limits of the backpack of a hero.
        /// </summary>
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
            /// <summary>
            /// The least stock of a resource.
            /// </summary>
            public const long Min = 0;

            /// <summary>
            /// The most gold a player can be given here (the same in every game of the series).
            /// </summary>
            public const long MaxGold = Heroes.Constants.Resources.MaxGold;

            /// <summary>
            /// The most the editor gives a player of a resource other than gold.
            /// </summary>
            public const long MaxOther = Heroes.Constants.Resources.MaxOther;

            /// <summary>
            /// The step of the gold stepper.
            /// </summary>
            public const long GoldStep = 1000;

            /// <summary>
            /// The step of the stepper of a resource other than gold.
            /// </summary>
            public const long OtherStep = 1;

            /// <summary>
            /// What the "all resources" and "rich" presets give.
            /// </summary>
            public const int AllResourcesPreset = 99;

            /// <summary>
            /// What the "rich" preset gives in gold.
            /// </summary>
            public const int RichGoldPreset = 1_000_000;

            /// <summary>
            /// What the "Res +100" and "Gold +100k" buttons add.
            /// </summary>
            public const int AllResourcesIncrease = 100;

            /// <summary>
            /// What the "Gold +100k" button adds.
            /// </summary>
            public const int GoldIncrease = 100_000;

            /// <summary>
            /// What the "+10" button of a resource and the "+10k" button of gold add.
            /// </summary>
            public const int OtherPreset = 10;

            /// <summary>
            /// What the "+10k" button of gold adds.
            /// </summary>
            public const int GoldPreset = 10_000;
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

            /// <summary>
            /// The greatest number of a hero.
            /// </summary>
            public const int MaxHeroId = 254;

            /// <summary>
            /// The most heroes a player has on the map: the places of the list of heroes of a player (a garrisoned hero is not in it).
            /// </summary>
            public const int HeroListSize = 8;

            /// <summary>The capacity of the town-id list in a player record.</summary>
            public const int TownListSize = 72;

            /// <summary>The smallest town id.</summary>
            public const int MinTownId = 0;

            /// <summary>The largest town id; FF means no town.</summary>
            public const int MaxTownId = 254;

            /// <summary>
            /// The days a player with no town has left before being banished: 7 the day the last town is lost, 0 once banished.
            /// A player who owns a town has none (FF in the savefile).
            /// </summary>
            public const int MinDaysLeft = Heroes.Constants.Player.MinDaysLeft;

            /// <summary>
            /// The most days a player who lost its last town has before being banished.
            /// </summary>
            public const int MaxDaysLeft = Heroes.Constants.Player.MaxDaysLeft;
        }

        /// <summary>
        /// Limits of a castle.
        /// </summary>
        public static class Castle
        {
            /// <summary>
            /// The least creatures that wait in a dwelling.
            /// </summary>
            public const long MinDwellingCreatures = 0;

            /// <summary>
            /// A dwelling stores the creatures it has in 16 bits.
            /// </summary>
            public const long MaxDwellingCreatures = ushort.MaxValue;

            /// <summary>
            /// How many spells a level of the Mage Guild holds is a byte in the savefile.
            /// </summary>
            public const long MaxSpellsOfALevel = byte.MaxValue;
        }
    }
}
