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

        /// <inheritdoc/>
        public byte[] Read(string filePath) => File.ReadAllBytes(filePath);

        /// <inheritdoc/>
        public void Write(string filePath, byte[] bytes)
        {
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
