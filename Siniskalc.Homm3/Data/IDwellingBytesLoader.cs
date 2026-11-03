namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Cuts the records of the dwellings out of a savefile.
    /// </summary>
    public interface IDwellingBytesLoader
    {
        /// <summary>
        /// Returns the records of the dwellings in the order of the table, or none when the table is not found.
        /// </summary>
        /// <param name="bytes">The savefile.</param>
        /// <param name="others">Other chunks of the savefile; an overlapping generator table is refused in full.</param>
        IReadOnlyList<RecordChunk> LoadDwellingChunks(byte[] bytes, IEnumerable<BytesChunk> others);
    }
}
