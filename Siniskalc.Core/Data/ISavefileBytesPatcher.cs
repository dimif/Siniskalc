namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Writes the chunks of a savefile over its original bytes.
    /// </summary>
    public interface ISavefileBytesPatcher
    {
        /// <summary>
        /// Returns a copy of the savefile with every chunk written over it.
        /// </summary>
        /// <param name="originalSave">The bytes of the savefile as it was loaded; not modified.</param>
        /// <param name="chunks">The chunks to write, at their <see cref="BytesChunk.StartIndex"/>.</param>
        /// <returns>The patched copy.</returns>
        /// <exception cref="System.ArgumentException">A chunk lies outside the savefile or overlaps another chunk.</exception>
        byte[] Patch(byte[] originalSave, IEnumerable<BytesChunk> chunks);

        /// <summary>
        /// Returns a copy of <paramref name="target"/> with only the chunk bytes that differ from <paramref name="originalSave"/> written over it.
        /// </summary>
        /// <param name="originalSave">The bytes of the savefile as it was loaded; not modified.</param>
        /// <param name="target">The bytes to write the changes over, of the same length as <paramref name="originalSave"/>; not modified.</param>
        /// <param name="chunks">The chunks whose changes are written.</param>
        /// <returns>The patched copy of <paramref name="target"/>.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="target"/> has another length than <paramref name="originalSave"/>, or a chunk lies outside the savefile or overlaps another.</exception>
        byte[] PatchChanges(byte[] originalSave, byte[] target, IEnumerable<BytesChunk> chunks);
    }
}
