namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Where the parts of a savegame are, as found by <see cref="ISaveLayoutLocator"/>. The tables of players, heroes and castles
    /// are at fixed places after a header whose size depends on the version of the game.
    /// </summary>
    /// <param name="Version">The game the savegame comes from.</param>
    /// <param name="Width">The width of the map in tiles.</param>
    /// <param name="Height">The height of the map in tiles.</param>
    /// <param name="HeroStride">The size of the record of a hero.</param>
    /// <param name="DateStart">Where the date is: the day, the week and the month, a byte each, 2 bytes apart.</param>
    /// <param name="PlayerTableStart">Where the record of the first player (Blue) begins.</param>
    /// <param name="HeroTableStart">Where the record of the first hero begins, which is its header (the spell points) and not its name.</param>
    /// <param name="CastleTableStart">Where the record of the first castle begins.</param>
    public sealed record SaveLayout(
        GameVersion Version,
        int Width,
        int Height,
        int HeroStride,
        int DateStart,
        int PlayerTableStart,
        int HeroTableStart,
        int CastleTableStart)
    {
        /// <summary>
        /// Gets how many bytes the file has in front of the header of the base game: none for the base game, the 4 bytes of the marker for
        /// the Price of Loyalty.
        /// </summary>
        public int MarkerLength => this.Version == GameVersion.PriceOfLoyalty ? HeaderLayout.MarkerLength : 0;

        /// <summary>
        /// Gets where the difficulty the map was made with is.
        /// </summary>
        public int MapDifficultyStart => HeaderLayout.MapDifficulty + this.MarkerLength;

        /// <summary>
        /// Gets where the difficulty the game was started at is.
        /// </summary>
        public int DifficultyStart => HeaderLayout.Difficulty + this.MarkerLength;

        /// <summary>
        /// Gets where the byte is that says whether the first colour is in the game.
        /// </summary>
        public int ColorsInGameStart => HeaderLayout.ColorsInGame + this.MarkerLength;

        /// <summary>
        /// Gets where the byte is that says who plays the first colour.
        /// </summary>
        public int ControlsStart => HeaderLayout.Controls + this.MarkerLength;

        /// <summary>
        /// Gets where the mask of the human players is: a byte for each record of the table of players.
        /// </summary>
        public int HumanMaskStart => this.DateStart - HeaderLayout.HumanMaskBeforeDate;

        /// <summary>
        /// Gets where the number of the human whose turn it is is: a fixed distance in front of the date (the Price of Loyalty has the bytes between the
        /// marker and the date 5 bytes further on, not 4).
        /// </summary>
        public int CurrentHumanStart => this.DateStart - (HeaderLayout.Date - HeaderLayout.CurrentHuman);

        /// <summary>
        /// Gets where the name of the map begins: a 16-bit length, then the text.
        /// </summary>
        public int MapNameStart => HeaderLayout.MapName + this.MarkerLength;

        /// <summary>
        /// Gets where the description of the map begins.
        /// </summary>
        public int MapDescriptionStart => HeaderLayout.MapDescription + this.MarkerLength;

        /// <summary>
        /// Gets the position in the savefile of the record of the player at <paramref name="index"/>.
        /// </summary>
        public int PlayerStart(int index) => this.PlayerTableStart + (index * PlayerLayout.RecordSize);

        /// <summary>
        /// Gets the position in the savefile of the record of the hero at <paramref name="index"/>.
        /// </summary>
        public int HeroStart(int index) => this.HeroTableStart + (index * this.HeroStride);

        /// <summary>
        /// Gets the position in the savefile of the record of the castle at <paramref name="index"/>.
        /// </summary>
        public int CastleStart(int index) => this.CastleTableStart + (index * CastleLayout.RecordSize);

        /// <summary>
        /// Gets the size the savegame has at least: the end of the table of castles.
        /// </summary>
        public int MinimumLength => this.CastleStart(Constants.Table.CastleSlotCount);
    }
}
