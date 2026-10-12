namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one player of a savegame through its history.
    /// </summary>
    public sealed class Homm2PlayerEditSession : EditSession<Homm2PlayerChunk, Homm2PlayerModel>
    {
        public Homm2PlayerEditSession(SaveDocumentHistory history, int playerIndex)
            : base(history, Homm2ChunkKinds.Player, playerIndex, history.Current.Players().Deserializer, history.Current.Players().Serializer)
        {
        }

        public int PlayerIndex => this.Index;
    }
}
