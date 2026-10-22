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
        /// <param name="mines">The mines of the savefile: the dwellings come after them.</param>
        /// <param name="others">The other chunks of the savefile: the table stops before the first record that would overlap one, as the player
        /// block does on some maps.</param>
        IReadOnlyList<DwellingChunk> LoadDwellingChunks(byte[] bytes, IReadOnlyList<MineChunk> mines, IEnumerable<BytesChunk> others);
    }
}
