namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// Where things are in the header of a savegame of the base game, from its start. A Price of Loyalty savegame has 4 more bytes in
    /// front of the width (<see cref="Marker"/>) and the tables that follow the header are 5 bytes further (<see cref="ExpansionShift"/>).
    /// </summary>
    public static class HeaderLayout
    {
        /// <summary>
        /// The 4 bytes a Price of Loyalty savegame starts with.
        /// </summary>
        public const uint Marker = 0xFFFFFFFF;

        /// <summary>
        /// How many bytes the Price of Loyalty adds in front of the header of the base game.
        /// </summary>
        public const int MarkerLength = 4;

        /// <summary>
        /// How many bytes later the date and the tables of players, heroes and castles are in a Price of Loyalty savegame.
        /// </summary>
        public const int ExpansionShift = 5;

        /// <summary>
        /// The number the map format starts its header with, after the width and the height (it is the same number as in a map file).
        /// </summary>
        public const uint MapFormat = 0x5C;

        public const int Width = 0;
        public const int Height = 4;
        public const int MapFormatField = 8;

        /// <summary>
        /// The difficulty the map was made with: a 16-bit number, 0 easy, 1 normal, 2 hard, 3 expert (Price of Loyalty saves of maps of
        /// each level; no map of the hardest level seen). It is not the difficulty the game was started at.
        /// </summary>
        public const int MapDifficulty = 0x0C;

        /// <summary>
        /// The difficulty the game was started at: a byte just in front of the file name of the map, 0 easy to 4 impossible.
        /// </summary>
        public const int Difficulty = 0x1C4;

        /// <summary>
        /// Which colours the map has: a byte for each colour (Blue first), 1 for a colour in the game. The bytes that follow (6 and 6) say
        /// which colours a human may play and which a computer may play; they are what the map allows, not who plays.
        /// </summary>
        public const int ColorsInGame = 0x10;

        /// <summary>
        /// Who plays each colour: a byte for each colour (Blue first). 0x0A is the computer; a human has his number among the humans, from 0
        /// (the colour of the human in a game of one, the colour itself in a game where every colour is human). Found in saves of one, four
        /// and six humans (<c>TestData/Homm2/HumanCpu</c>).
        /// </summary>
        public const int Controls = 0x1BE;

        /// <summary>
        /// The byte of <see cref="Controls"/> that says a colour is played by the computer.
        /// </summary>
        public const byte ComputerControl = 0x0A;

        /// <summary>
        /// How many bytes in front of the date the mask of the human players is: a byte for each record of the table of players (not each
        /// colour), 1 for a human. The game plays the humans from this mask, and the records of the humans come first; the controls at
        /// <see cref="Controls"/> and the count at <see cref="HumanCount"/> only feed the scenario summary. Probes that changed only those
        /// were ignored by the game; the ones that changed the mask were followed.
        /// </summary>
        public const int HumanMaskBeforeDate = 6;

        /// <summary>
        /// The number of humans (1, 2, 4 or 6 in the saves; it matches the last digit of the extension of a hot seat save).
        /// </summary>
        public const int HumanCount = 0x1DA;

        /// <summary>
        /// The faction of each colour (Knight 0, Barbarian 1, Sorceress 2, Warlock 3, Wizard 4, Necromancer 5): six bytes, Blue first.
        /// </summary>
        public const int Factions = 0x1B8;

        /// <summary>
        /// The number, among the humans, of the player whose turn it is (in a hot seat game of six humans it is the colour). It is the
        /// <see cref="Controls"/> byte of that colour.
        /// </summary>
        public const int CurrentHuman = 0x2AD;

        /// <summary>
        /// The name of the map: 16 bytes, zero terminated.
        /// </summary>
        public const int MapName = 0x42;
        public const int MapNameLength = 16;

        /// <summary>
        /// The description of the map: a zero terminated text.
        /// </summary>
        public const int MapDescription = 0x7E;
        public const int MapDescriptionMaxLength = 0x100;

        /// <summary>
        /// The day, the week and the month: a byte each, 2 bytes apart (the byte between is 0), with the day of the week first. The last byte is the one in front of the table of players.
        /// </summary>
        public const int Date = 0x2BB;
        public const int DateStep = 2;

        public const int PlayerTable = 0x2C0;

        /// <summary>
        /// The header of the first hero: the spell points come first, the name 10 bytes later.
        /// </summary>
        public const int HeroTable = 0x79C;

        /// <summary>
        /// How far the table of castles is after the end of the table of heroes.
        /// </summary>
        public const int CastleTableGap = 0x36;
    }
}
