namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one player of a savegame through its history.
    /// </summary>
    public sealed class PlayerEditSession : EditSession<PlayerChunk, PlayerModel>
    {
        public PlayerEditSession(SaveDocumentHistory history, int playerIndex)
            : base(history, Homm2ChunkKinds.Player, playerIndex, history.Current.Players().Deserializer, history.Current.Players().Serializer)
        {
        }

        public int PlayerIndex => this.Index;

        /// <summary>
        /// Gets whether the player differs from the savefile as it was loaded or last saved. The record of a player also keeps the lists of
        /// heroes and castles, which follow the owners of the heroes and the castles and are put back with them: only the model counts.
        /// </summary>
        public override bool IsDirty => this.Current != this.Original;
    }
}
