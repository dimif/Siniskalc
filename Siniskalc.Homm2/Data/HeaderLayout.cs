namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// Where things are in the header of a savegame of the base game, from its start. A Price of Loyalty savegame has 4 more bytes in
    /// front of the width (<see cref="Marker"/>), so the fields before the date are 4 bytes further (<see cref="MarkerLength"/>), and the
    /// date and the tables of players, heroes and castles are 5 bytes further (<see cref="ExpansionShift"/>).
    /// </summary>
    public static class HeaderLayout
    {
        /// <summary>
        /// The 4 bytes a Price of Loyalty savegame starts with.
        /// </summary>
        public const uint Marker = 0xFFFFFFFF;

        /// <summary>
        /// How many bytes the Price of Loyalty adds in front of the header of the base game (the length of <see cref="Marker"/>).
        /// </summary>
        public const int MarkerLength = 4;

        /// <summary>
        /// How many bytes later the date and the tables of players, heroes and castles are in a Price of Loyalty savegame (the marker
        /// and one byte more).
        /// </summary>
        public const int ExpansionShift = 5;

        /// <summary>
        /// The value of the map format field (<see cref="MapFormatField"/>) in every savegame; it is the same number as in a map file.
        /// </summary>
        public const uint MapFormat = 0x5C;

        /// <summary>
        /// Where the width of the map in tiles is: a 32-bit number.
        /// </summary>
        public const int Width = 0;

        /// <summary>
        /// Where the height of the map in tiles is: a 32-bit number, equal to the width.
        /// </summary>
        public const int Height = 4;

        /// <summary>
        /// Where the map format is, after the width and the height: a 32-bit number that is <see cref="MapFormat"/>.
        /// </summary>
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
        /// Who plays each colour: a byte for each colour (Blue first). 0x0A is the computer; a human has their number among the humans, from 0
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
        /// <see cref="Controls"/> value of that colour. Unlike the other fields it is not placed from the start of the file: it is a
        /// fixed distance in front of the date (<see cref="Date"/> minus this offset), which is how the Price of Loyalty shift is applied.
        /// </summary>
        public const int CurrentHuman = 0x2AD;

        /// <summary>
        /// Where the name of the map is: text, zero terminated.
        /// </summary>
        public const int MapName = 0x42;

        /// <summary>
        /// The most bytes the name of the map has.
        /// </summary>
        public const int MapNameLength = 16;

        /// <summary>
        /// Where the description of the map is: text, zero terminated.
        /// </summary>
        public const int MapDescription = 0x7E;

        /// <summary>
        /// The most bytes the description of the map has.
        /// </summary>
        public const int MapDescriptionMaxLength = 0x100;

        /// <summary>
        /// Where the date is: the day, the week and the month, in that order, a byte each, <see cref="DateStep"/> bytes apart (the byte
        /// between is 0). The last byte is the one in front of the table of players.
        /// </summary>
        public const int Date = 0x2BB;

        /// <summary>
        /// The distance between the bytes of the day, the week and the month.
        /// </summary>
        public const int DateStep = 2;

        /// <summary>
        /// Where the record of the first player (Blue) begins.
        /// </summary>
        public const int PlayerTable = 0x2C0;

        /// <summary>
        /// The header of the first hero: the spell points come first, the name 10 bytes later.
        /// </summary>
        public const int HeroTable = 0x79C;

        /// <summary>
        /// How many bytes there are between the end of the table of heroes and the start of the table of castles.
        /// </summary>
        public const int CastleTableGap = 0x36;
    }
}
