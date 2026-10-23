namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The raw bytes of one mine, edited through the savefile's <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class MineByteEditTarget : ChunkByteEditTarget<RecordChunk, MineModel>
    {
        public MineByteEditTarget(SaveDocumentHistory history, int mineIndex, IMineRegionProvider regions)
            : this(history.CreateMineSession(mineIndex), regions)
        {
        }

        private MineByteEditTarget(EditSession<RecordChunk, MineModel> session, IMineRegionProvider regions)
            : base(session, regions.GetRegions(session.Chunk), () => session.Current.Name)
        {
        }
    }
}
