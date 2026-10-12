namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Where the parts of a savegame are, as found by <see cref="IHomm2SaveLayoutLocator"/>. The tables of players, heroes and castles
    /// are at fixed places after a header whose size depends on the version of the game.
    /// </summary>
    /// <param name="Version">The game the savegame comes from.</param>
    /// <param name="Marker">How many bytes the file has in front of the header of the base game (the 4 bytes of the Price of Loyalty).</param>
    /// <param name="Width">The width of the map in tiles.</param>
    /// <param name="Height">The height of the map in tiles.</param>
    /// <param name="HeroStride">The size of the record of a hero.</param>
    /// <param name="DateStart">Where the date is: the day, the week and the month, a byte each, 2 bytes apart.</param>
    /// <param name="PlayerTableStart">Where the record of the first player (Blue) begins.</param>
    /// <param name="HeroTableStart">Where the record of the first hero begins, which is its header (the spell points) and not its name.</param>
    /// <param name="CastleTableStart">Where the record of the first castle begins.</param>
    public sealed record Homm2SaveLayout(
        Homm2GameVersion Version,
        int Marker,
        int Width,
        int Height,
        int HeroStride,
        int DateStart,
        int PlayerTableStart,
        int HeroTableStart,
        int CastleTableStart)
    {
        /// <summary>
        /// Gets where the name of the map begins: a 16-bit length, then the text.
        /// </summary>
        public int MapNameStart => Homm2HeaderLayout.MapName + this.Marker;

        /// <summary>
        /// Gets where the description of the map begins.
        /// </summary>
        public int MapDescriptionStart => Homm2HeaderLayout.MapDescription + this.Marker;

        /// <summary>
        /// Gets the position in the savefile of the record of the player at <paramref name="index"/>.
        /// </summary>
        public int PlayerStart(int index) => this.PlayerTableStart + (index * Homm2PlayerLayout.RecordSize);

        /// <summary>
        /// Gets the position in the savefile of the record of the hero at <paramref name="index"/>.
        /// </summary>
        public int HeroStart(int index) => this.HeroTableStart + (index * this.HeroStride);

        /// <summary>
        /// Gets the position in the savefile of the record of the castle at <paramref name="index"/>.
        /// </summary>
        public int CastleStart(int index) => this.CastleTableStart + (index * Homm2CastleLayout.RecordSize);

        /// <summary>
        /// Gets the size the savegame has at least: the end of the table of castles.
        /// </summary>
        public int MinimumLength => this.CastleStart(Constants.Table.CastleSlotCount);
    }
}
