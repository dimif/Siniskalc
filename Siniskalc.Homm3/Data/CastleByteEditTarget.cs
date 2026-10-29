namespace Siniskalc.Homm3.Data
{
    using System;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The raw bytes of one castle (a town), edited through the savefile's <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class CastleByteEditTarget : ChunkByteEditTarget<CastleChunk, Models.CastleModel>
    {
        public CastleByteEditTarget(CastleEditSession session, ICastleRegionProvider regions)
            : base(session, regions.GetRegions(session.Chunk))
        {
        }
    }
}
