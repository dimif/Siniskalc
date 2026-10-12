namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// A view onto a single castle in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class CastleEditSession : EditSession<CastleChunk, CastleModel>
    {
        public CastleEditSession(SaveDocumentHistory history, int castleIndex)
            : base(history, Homm3ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer)
        {
        }

        public int CastleIndex => this.Index;
    }
}
