namespace Siniskalc.Runtime
{
    /// <summary>
    /// Builds a <see cref="Game"/> from the (decompressed) bytes of a savefile.
    /// </summary>
    public interface ISavegameParser
    {
        /// <param name="file">The savefile the bytes were read from, or null when they did not come from one.</param>
        /// <param name="name">What to call the game when it has no file.</param>
        Game Parse(byte[] bytes, string? file, string? name = null);
    }
}
