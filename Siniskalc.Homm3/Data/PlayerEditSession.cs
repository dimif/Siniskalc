namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

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

        /// <summary>
        /// Gets whether the player differs from the savefile as it was loaded or last saved. The record of a player also keeps the list of
        /// heroes, which follows the owners of the heroes and is put back with them: only the model counts.
        /// </summary>
        public override bool IsDirty => this.Current != this.Original;
    }
}
