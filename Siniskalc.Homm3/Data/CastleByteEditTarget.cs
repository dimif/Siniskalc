namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The raw bytes of one castle (a town), edited through the savefile's <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class CastleByteEditTarget : ChunkByteEditTarget<CastleChunk, Models.CastleModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CastleByteEditTarget"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="regions">The regions.</param>
        public CastleByteEditTarget(CastleEditSession session, ICastleRegionProvider regions)
            : base(session, regions.GetRegions(session.Chunk))
        {
        }
    }
}
