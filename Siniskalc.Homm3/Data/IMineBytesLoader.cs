namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Cuts the records of the mines out of a savefile.
    /// </summary>
    public interface IMineBytesLoader
    {
        /// <summary>
        /// Returns the records of the mines in the order of the table, or none when the table is not found.
        /// </summary>
        IReadOnlyList<RecordChunk> LoadMineChunks(byte[] bytes);
    }
}
