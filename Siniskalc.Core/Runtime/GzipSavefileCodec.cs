namespace Siniskalc.Core.Runtime
{
    using System;
    using System.IO;
    using ICSharpCode.SharpZipLib;
    using ICSharpCode.SharpZipLib.GZip;

    /// <summary>
    /// Savefiles that are GZip files.
    /// </summary>
    /// <remarks>
    /// Some games write their savefiles as broken GZip files, causing .NET's GZipStream to throw ("unsupported compression method"),
    /// which is why SharpZipLib is used. The files can also have an incorrect CRC which every tool and library complains about.
    /// A file that is not GZip, or ends early, is a <see cref="CorruptSavefileException"/>; a file that cannot be opened is an
    /// <see cref="IOException"/> as usual.
    /// </remarks>
    public sealed class GzipSavefileCodec : ISavefileCodec
    {
        /// <inheritdoc/>
        public byte[] Read(string filePath)
        {
            try
            {
                using var fileStream = new FileInfo(filePath).OpenRead();
                using var gzipStream = new GZipInputStream(fileStream);
                using var memoryStream = new MemoryStream();
                gzipStream.CopyTo(memoryStream);

                return memoryStream.ToArray();
            }
            catch (Exception ex) when (ex is SharpZipBaseException or InvalidDataException or EndOfStreamException)
            {
                // TODO:LOG warning: {filePath} could not be decompressed; log the exception from the decompressor.
                throw new CorruptSavefileException($"{Path.GetFileName(filePath)} is not a compressed savegame, or it is damaged: {ex.Message}", ex);
            }
        }

        /// <remarks>
        /// Writes to a file of its own next to <paramref name="filePath"/> and moves it over the savefile once it is complete, so a
        /// write that fails halfway leaves the savefile as it was.
        /// </remarks>
        /// <remarks>
        /// Writes to a file of its own next to <paramref name="filePath"/> and moves it over the savefile once it is complete, so a
        /// write that fails halfway leaves the savefile as it was.
        /// </remarks>
        /// <inheritdoc/>
        public void Write(string filePath, byte[] bytes)
        {
            var temporary = filePath + ".saving";
            try
            {
                // FileMode.Create empties an existing file first; otherwise a smaller result would leave the old tail behind.
                using (var fileStream = new FileStream(temporary, FileMode.Create, FileAccess.Write))
                using (var gzipStream = new GZipOutputStream(fileStream))
                using (var memoryStream = new MemoryStream(bytes))
                {
                    memoryStream.CopyTo(gzipStream);
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
