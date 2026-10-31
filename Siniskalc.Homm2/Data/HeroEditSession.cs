namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one hero of a savegame through its history. A change of its owner is an edit like any other: the lists of heroes of the players
    /// follow it (<see cref="PlayerListsReconciler"/>).
    /// </summary>
    public sealed class HeroEditSession : EditSession<HeroChunk, HeroModel>
    {
        public HeroEditSession(SaveDocumentHistory history, int heroIndex)
            : base(history, Homm2ChunkKinds.Hero, heroIndex, history.Current.Heroes().Deserializer, history.Current.Heroes().Serializer)
        {
        }

        public int HeroIndex => this.Index;
    }
}
