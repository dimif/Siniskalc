namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto one dwelling in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class DwellingEditSession : EditSession<DwellingChunk, DwellingModel>
    {
        public DwellingEditSession(SaveDocumentHistory history, int index)
            : base(history, Homm3ChunkKinds.Dwellings, index)
        {
        }
    }
}
