namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// A view onto a single player in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class PlayerEditSession : EditSession<PlayerChunk, PlayerModel>
    {
        public PlayerEditSession(SaveDocumentHistory history, int playerIndex)
            : base(history, Homm3ChunkKinds.Player, playerIndex, history.Current.Players().Deserializer, history.Current.Players().Serializer)
        {
        }

        public int PlayerIndex => this.Index;
    }
}
