namespace Siniskalc.Data
{
    /// <summary>
    /// Where the setup of the players is in a savefile, as found by <see cref="PlayerControlLocator"/>.
    /// </summary>
    /// <param name="ControlsAt">The offset of the eight bytes, one per colour, that say who plays it.</param>
    /// <param name="FileNameAt">The offset of the file name of the map, after the difficulty.</param>
    /// <param name="FileNameLength">The length of the file name in bytes, including its extension.</param>
    /// <param name="HumanFlagsAt">The offset of the eight 32-bit human flags, or -1 when they are not found.</param>
    /// <param name="HumanFlagsToMarker">How far the marker that follows the current player is from the human flags: it depends on the game version.</param>
    public sealed record PlayerSetupLocation(int ControlsAt, int FileNameAt, int FileNameLength, int HumanFlagsAt, int HumanFlagsToMarker = PlayerControlLocator.HotaHumanFlagsToMarker)
    {
        /// <summary>
        /// Gets how long the marker is: FF 10 in Horn of the Abyss, FF 00 00 00 in the other versions.
        /// </summary>
        public int MarkerLength => this.HumanFlagsToMarker == PlayerControlLocator.HotaHumanFlagsToMarker ? 2 : 4;

        /// <summary>
        /// Gets the offset of the difficulty byte, which follows the eight bytes for the colours.
        /// </summary>
        public int DifficultyAt => this.ControlsAt + Constants.Player.ColorCount;

        /// <summary>
        /// Gets whether the human flags (and the current player after them) were found.
        /// </summary>
        public bool HasHumanFlags => this.HumanFlagsAt >= 0;

        /// <summary>
        /// Gets the offset of the 32-bit colour whose turn it is, which follows the human flags.
        /// </summary>
        public int CurrentPlayerAt => this.HumanFlagsAt + PlayerControlLocator.HumanFlagsLength;
    }
}
