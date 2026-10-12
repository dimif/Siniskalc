namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;

    /// <summary>
    /// Finds the table of players in a savefile and loads each as a <see cref="PlayerChunk"/> named after its colour.
    /// </summary>
    public interface IPlayerBytesLoader
    {
        /// <param name="bytes">The savefile bytes.</param>
        /// <param name="gameVersion">The version of the savefile.</param>
        /// <param name="occupied">Chunks of other things (heroes, castles) found in the savefile; the last player is cut short where one begins.</param>
        IReadOnlyList<PlayerChunk> LoadPlayerChunks(byte[] bytes, GameVersion gameVersion, IEnumerable<BytesChunk>? occupied = null);
    }
}
