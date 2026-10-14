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
