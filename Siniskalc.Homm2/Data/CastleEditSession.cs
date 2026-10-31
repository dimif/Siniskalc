namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one castle of a savegame through its history. A change of its owner is an edit like any other: the lists of castles of the
    /// players and their days left follow it (<see cref="PlayerListsReconciler"/>).
    /// </summary>
    public sealed class CastleEditSession : EditSession<CastleChunk, CastleModel>
    {
        public CastleEditSession(SaveDocumentHistory history, int castleIndex)
            : base(history, Homm2ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer)
        {
        }

        public int CastleIndex => this.Index;
    }
}
