namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto a single castle in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class CastleEditSession : EditSession<CastleChunk, CastleModel>, ICastleSession
    {
        public CastleEditSession(SaveDocumentHistory history, int castleIndex)
            : base(history, Homm3ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer)
        {
        }

        public int CastleIndex => this.Index;

        /// <inheritdoc/>
        public string Name => this.History.Current.Castles().GetName(this.CastleIndex);

        /// <inheritdoc/>
        public GameVersion GameVersion => this.History.Current.GameVersion();

        /// <inheritdoc/>
        public IHeroSummaries<PlayerColor> Heroes => this.History.Current.Heroes();

        /// <inheritdoc/>
        public HeroModel? FindHero(int heroId)
        {
            var heroes = this.History.Current.Heroes();
            var index = heroes.IndexOfId(heroId);
            return index < 0 ? null : heroes.GetModel(index);
        }

        /// <inheritdoc/>
        public IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForCastle(this);
    }
}
