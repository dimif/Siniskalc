namespace Siniskalc.Homm3.Runtime
{
    using System;
    using System.IO;
    using Siniskalc.Core.Runtime;
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
        /// <summary>
        /// The number of bytes at the start of a file that tell whether it is a GZip file.
        /// </summary>
        public const int MagicLength = 2;

        private const byte FirstMagicByte = 0x1F;
        private const byte SecondMagicByte = 0x8B;

        /// <summary>
        /// Tells whether the first bytes of a file are those of a GZip file.
        /// </summary>
        public static bool IsGzip(ReadOnlySpan<byte> head) => head.Length >= MagicLength && head[0] == FirstMagicByte && head[1] == SecondMagicByte;

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
