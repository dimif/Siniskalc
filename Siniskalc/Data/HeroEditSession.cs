namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// A view onto a single hero in a <see cref="SaveDocumentHistory"/>. The hero keeps what depends on its artifacts (spells, skill
    /// bonuses) up to date itself, so its model is written as it is.
    /// </summary>
    public sealed class HeroEditSession : EditSession<HeroChunk, HeroModel>
    {
        public HeroEditSession(SaveDocumentHistory history, int heroIndex)
            : base(history, Homm3ChunkKinds.Hero, heroIndex, history.Current.Heroes().Deserializer, history.Current.Heroes().Serializer)
        {
            this.Offsets = history.Current.Heroes().Offsets;
        }

        public int HeroIndex => this.Index;

        /// <summary>
        /// Gets where the hero fields are in the chunk, for the savefile's game version.
        /// </summary>
        public HeroOffsets Offsets { get; }
    }
}
