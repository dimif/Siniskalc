namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Cuts the records of the mines out of a savefile.
    /// </summary>
    public interface IMineBytesLoader
    {
        /// <summary>
        /// Returns the records of the mines in the order of the table, or none when the table is not found.
        /// </summary>
        IReadOnlyList<MineChunk> LoadMineChunks(byte[] bytes);
    }
}
