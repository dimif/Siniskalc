namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Builds a <see cref="SaveDocument"/> from the chunks found in a savefile, giving its collections what they read, write and
    /// compare their chunks with.
    /// </summary>
    public interface ISaveDocumentFactory
    {
        /// <param name="bytes">The savefile bytes.</param>
        /// <param name="heroChunks">The heroes.</param>
        /// <param name="offsets">Where the hero fields are, for the savefile's game version.</param>
        /// <param name="playerChunks">The players; none when omitted.</param>
        /// <param name="castleChunks">The castles; none when omitted.</param>
        /// <param name="obeliskChunks">The visited obelisks (one chunk or none); none when omitted.</param>
        SaveDocument Create(
            byte[] bytes,
            IEnumerable<HeroChunk> heroChunks,
            HeroOffsets offsets,
            IEnumerable<PlayerChunk>? playerChunks = null,
            IEnumerable<CastleChunk>? castleChunks = null,
            IEnumerable<ObeliskChunk>? obeliskChunks = null);
    }
}
