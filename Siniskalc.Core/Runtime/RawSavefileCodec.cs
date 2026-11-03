namespace Siniskalc.Core.Runtime
{
    using System.IO;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Reads and writes a savefile that the game writes as it is, without compressing it.
    /// </summary>
    public sealed class RawSavefileCodec : ISavefileCodec
    {
        private static readonly ILogger<RawSavefileCodec> Logger = LogFactory.For<RawSavefileCodec>();

        /// <summary>
        /// Reads the file as it is.
        /// </summary>
        /// <param name="filePath">The path of the file to read.</param>
        /// <returns>All the bytes of the file.</returns>
        /// <exception cref="IOException">The file could not be read.</exception>
        public byte[] Read(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            Logger.LogDebug("Read {File} ({Length} bytes)", Path.GetFileName(filePath), bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Writes the bytes to a temporary file next to <paramref name="filePath"/> and moves it over the file, so a failed write leaves the old file as it was.
        /// </summary>
        /// <param name="filePath">The path of the file to write.</param>
        /// <param name="bytes">The bytes to write.</param>
        /// <exception cref="IOException">The file could not be written.</exception>
        public void Write(string filePath, byte[] bytes)
        {
            Logger.LogDebug("Writing {File} ({Length} bytes)", Path.GetFileName(filePath), bytes.Length);
            var temporary = filePath + ".saving";
            try
            {
                // FileMode.Create empties an existing file first; otherwise a smaller result would leave the old tail behind.
                using (var fileStream = new FileStream(temporary, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                }

                File.Move(temporary, filePath, overwrite: true);
            }
            catch
            {
                try
                {
                    File.Delete(temporary);
                }
                catch (IOException)
                {
                    Logger.LogWarning("Could not remove the temporary file {File} after a failed save", Path.GetFileName(temporary));

                    // The write failed already; the leftover is only a stray file.
                }

                throw;
            }
        }
    }
}
