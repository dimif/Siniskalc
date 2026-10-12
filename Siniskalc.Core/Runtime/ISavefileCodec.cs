namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Reads and writes the bytes of a savefile as a file on disk, undoing and applying whatever the game wraps them in.
    /// </summary>
    public interface ISavefileCodec
    {
        /// <summary>
        /// Reads the savefile and returns its bytes as the game wrote them, before any compression.
        /// </summary>
        /// <param name="filePath">
        /// The path of the file to read.
        /// </param>
        /// <returns>
        /// The bytes of the savefile as the game wrote them, before any compression.
        /// </returns>
        byte[] Read(string filePath);

        /// <summary>
        /// Writes <paramref name="bytes"/> to <paramref name="filePath"/>, replacing a file that is there.
        /// A write that fails leaves the file that was there as it was.
        /// A write that fails leaves the file that was there as it was.
        /// </summary>
        /// <param name="filePath">
        /// The path of the file to write.
        /// </param>
        /// <param name="bytes">
        /// The bytes to write to the file.
        /// </param>
        void Write(string filePath, byte[] bytes);
    }
}
