namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one hero of a savegame through its history.
    /// </summary>
    public sealed class Homm2HeroEditSession : EditSession<Homm2HeroChunk, Homm2HeroModel>
    {
        public Homm2HeroEditSession(SaveDocumentHistory history, int heroIndex)
            : base(history, Homm2ChunkKinds.Hero, heroIndex, history.Current.Heroes().Deserializer, history.Current.Heroes().Serializer)
        {
        }

        public int HeroIndex => this.Index;
    }
}
