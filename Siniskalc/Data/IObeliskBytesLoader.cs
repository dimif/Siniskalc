namespace Siniskalc.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Finds the obelisks the player has visited in a savefile and loads them as an <see cref="ObeliskChunk"/>.
    /// </summary>
    public interface IObeliskBytesLoader
    {
        /// <param name="bytes">The savefile bytes.</param>
        /// <param name="players">The players found in the savefile; the obelisks are in front of the first colour's struct.</param>
        /// <returns>The chunk, or nothing when the savefile has no obelisks, or has other data where they would be.</returns>
        IReadOnlyList<ObeliskChunk> LoadObeliskChunks(byte[] bytes, IReadOnlyList<PlayerChunk> players);
    }
}
