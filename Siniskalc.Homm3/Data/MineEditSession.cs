namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto one mine in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class MineEditSession : EditSession<MineChunk, MineModel>
    {
        public MineEditSession(SaveDocumentHistory history, int index)
            : base(history, Homm3ChunkKinds.Mines, index)
        {
        }
    }
}
