namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Builds the open game of one game from the bytes of a savefile.
    /// </summary>
    /// <typeparam name="TGame">What the game calls an open savefile.</typeparam>
    public interface ISavegameParser<out TGame>
        where TGame : IOpenGame
    {
        /// <param name="bytes">The bytes of the savefile (decompressed when the game compresses them).</param>
        /// <param name="file">The savefile the bytes were read from, or null when they did not come from one.</param>
        /// <param name="name">What to call the game when it has no file.</param>
        /// <exception cref="UnsupportedSavefileException">The bytes are not a savegame this editor knows.</exception>
        TGame Parse(byte[] bytes, string? file, string? name = null);
    }
}
