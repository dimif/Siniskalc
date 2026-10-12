namespace Siniskalc.Core.Runtime
{
    using System;
    using System.IO;

    /// <summary>
    /// Reads and writes a savefile that the game writes as it is, without compressing it.
    /// </summary>
    public sealed class RawSavefileCodec : ISavefileCodec
    {
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
                    // TODO:LOG warning: the temporary file {temporary} could not be removed after a failed save.
                    // The write failed already; the leftover is only a stray file.
                }

                throw;
            }
        }
    }
}
