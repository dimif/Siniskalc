namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Cuts out of a savefile the bytes that say whose turn it is.
    /// </summary>
    public interface ITurnBytesLoader
    {
        /// <summary>
        /// Returns the turn chunk (see <see cref="TurnLayout"/>), or none when the setup of the players or its human flags are not found.
        /// </summary>
        IReadOnlyList<RecordChunk> LoadTurnChunks(byte[] bytes, GameVersion gameVersion);
    }
}
