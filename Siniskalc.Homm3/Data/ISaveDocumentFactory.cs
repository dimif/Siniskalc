namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;

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
        /// <param name="mineChunks">The mines; none when omitted.</param>
        /// <param name="dwellingChunks">The dwellings; none when omitted.</param>
        /// <param name="merchantChunks">The stocks of the merchants; none when omitted.</param>
        /// <param name="heroOwnerChunks">The table of hero owners (one chunk or none); none when omitted.</param>
        /// <param name="turnChunks">Whose turn it is (one chunk or none); none when omitted.</param>
        SaveDocument Create(
            byte[] bytes,
            IEnumerable<HeroChunk> heroChunks,
            HeroOffsets offsets,
            IEnumerable<PlayerChunk>? playerChunks = null,
            IEnumerable<CastleChunk>? castleChunks = null,
            IEnumerable<ObeliskChunk>? obeliskChunks = null,
            IEnumerable<RecordChunk>? mineChunks = null,
            IEnumerable<RecordChunk>? dwellingChunks = null,
            IEnumerable<RecordChunk>? merchantChunks = null,
            IEnumerable<RecordChunk>? heroOwnerChunks = null,
            IEnumerable<RecordChunk>? turnChunks = null);
    }
}
