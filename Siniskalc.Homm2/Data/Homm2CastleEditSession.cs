namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one castle of a savegame through its history.
    /// </summary>
    public sealed class Homm2CastleEditSession : EditSession<Homm2CastleChunk, Homm2CastleModel>
    {
        public Homm2CastleEditSession(SaveDocumentHistory history, int castleIndex)
            : base(history, Homm2ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer)
        {
        }

        public int CastleIndex => this.Index;
    }
}
