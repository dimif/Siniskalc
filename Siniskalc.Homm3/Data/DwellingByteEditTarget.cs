namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The raw bytes of one dwelling, edited through the savefile's <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class DwellingByteEditTarget : ChunkByteEditTarget<RecordChunk, DwellingModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DwellingByteEditTarget"/> class.
        /// </summary>
        /// <param name="history">The history.</param>
        /// <param name="dwellingIndex">The dwelling index.</param>
        /// <param name="regions">The regions.</param>
        public DwellingByteEditTarget(SaveDocumentHistory history, int dwellingIndex, IDwellingRegionProvider regions)
            : this(history.CreateDwellingSession(dwellingIndex), regions)
        {
        }

        private DwellingByteEditTarget(EditSession<RecordChunk, DwellingModel> session, IDwellingRegionProvider regions)
            : base(session, regions.GetRegions(session.Chunk), () => session.Current.Name)
        {
        }
    }
}
