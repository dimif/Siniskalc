namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto a single castle in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class CastleEditSession : EditSession<CastleChunk, CastleModel>, ICastleSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CastleEditSession"/> class.
        /// </summary>
        /// <param name="history">The history.</param>
        /// <param name="castleIndex">The castle index.</param>
        public CastleEditSession(SaveDocumentHistory history, int castleIndex)
            : base(history, Homm3ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer)
        {
        }

        /// <summary>
        /// Gets the number of the castle.
        /// </summary>
        public int CastleIndex => this.Index;

        /// <inheritdoc/>
        public string Name => this.History.Current.Castles().GetName(this.CastleIndex);

        /// <inheritdoc/>
        public GameVersion GameVersion => this.History.Current.GameVersion();

        /// <inheritdoc/>
        public IReadOnlyList<PlayerColor> PlayersInGame => this.History.Current.Players().GetColors();

        /// <inheritdoc/>
        public IHeroSummaries<PlayerColor> Heroes => this.History.Current.Heroes();

        /// <inheritdoc/>
        public IEditSession<MerchantStockModel>? CreateTownMerchantSession() =>
            this.History.Current.TownMerchant().Count == 0 ? null : new EditSession<RecordChunk, MerchantStockModel>(this.History, Homm3ChunkKinds.TownMerchant, 0);

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
