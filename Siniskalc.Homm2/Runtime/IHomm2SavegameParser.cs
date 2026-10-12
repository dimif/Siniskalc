namespace Siniskalc.Homm2.Runtime
{
    /// <summary>
    /// Builds a <see cref="Homm2Game"/> from the bytes of a savefile.
    /// </summary>
    public interface IHomm2SavegameParser
    {
        /// <param name="bytes">The bytes of the savefile.</param>
        /// <param name="file">The savefile the bytes were read from, or null when they did not come from one.</param>
        /// <param name="name">What to call the game when it has no file.</param>
        /// <exception cref="Siniskalc.Core.Runtime.UnsupportedSavefileException">The bytes are not a savegame this editor knows.</exception>
        Homm2Game Parse(byte[] bytes, string? file, string? name = null);
    }
}
