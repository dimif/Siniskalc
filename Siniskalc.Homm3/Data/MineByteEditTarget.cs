namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The raw bytes of one mine, edited through the savefile's <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class MineByteEditTarget : ChunkByteEditTarget<RecordChunk, MineModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MineByteEditTarget"/> class.
        /// </summary>
        /// <param name="history">The history.</param>
        /// <param name="mineIndex">The mine index.</param>
        /// <param name="regions">The regions.</param>
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
