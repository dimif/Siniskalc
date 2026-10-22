namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Builds a <see cref="SaveDocument"/>. The hero reader and writer depend on the game version, so they are made here, once
    /// per document, from the entities of the version; everything else is handed in.
    /// </summary>
    public sealed class SaveDocumentFactory : ISaveDocumentFactory
    {
        private readonly IVersionSpecificEntitiesProviderFactory versionEntities;
        private readonly IHeroDiffer heroDiffer;
        private readonly ICastleDeserializer castleDeserializer;
        private readonly ICastleSerializer castleSerializer;
        private readonly ICastleDiffer castleDiffer;
        private readonly IPlayerDeserializer playerDeserializer;
        private readonly IPlayerSerializer playerSerializer;
        private readonly IPlayerDiffer playerDiffer;
        private readonly IObeliskDeserializer obeliskDeserializer;
        private readonly IObeliskSerializer obeliskSerializer;
        private readonly IDiffer<ObeliskVisits<PlayerColor>> obeliskDiffer;
        private readonly IMineDeserializer mineDeserializer;
        private readonly IMineSerializer mineSerializer;
        private readonly IMineDiffer mineDiffer;
        private readonly IDwellingDeserializer dwellingDeserializer;
        private readonly IDwellingSerializer dwellingSerializer;
        private readonly IDwellingDiffer dwellingDiffer;
        private readonly IMerchantStockDeserializer merchantDeserializer;
        private readonly IMerchantStockSerializer merchantSerializer;
        private readonly IMerchantStockDiffer merchantDiffer;
        private readonly ISavefileBytesPatcher patcher;

        public SaveDocumentFactory(
            IVersionSpecificEntitiesProviderFactory versionEntities,
            IHeroDiffer heroDiffer,
            ICastleDeserializer castleDeserializer,
            ICastleSerializer castleSerializer,
            ICastleDiffer castleDiffer,
            IPlayerDeserializer playerDeserializer,
            IPlayerSerializer playerSerializer,
            IPlayerDiffer playerDiffer,
            IObeliskDeserializer obeliskDeserializer,
            IObeliskSerializer obeliskSerializer,
            IDiffer<ObeliskVisits<PlayerColor>> obeliskDiffer,
            IMineDeserializer mineDeserializer,
            IMineSerializer mineSerializer,
            IMineDiffer mineDiffer,
            IDwellingDeserializer dwellingDeserializer,
            IDwellingSerializer dwellingSerializer,
            IDwellingDiffer dwellingDiffer,
            IMerchantStockDeserializer merchantDeserializer,
            IMerchantStockSerializer merchantSerializer,
            IMerchantStockDiffer merchantDiffer,
            ISavefileBytesPatcher patcher)
        {
            this.versionEntities = versionEntities;
            this.heroDiffer = heroDiffer;
            this.castleDeserializer = castleDeserializer;
            this.castleSerializer = castleSerializer;
            this.castleDiffer = castleDiffer;
            this.playerDeserializer = playerDeserializer;
            this.playerSerializer = playerSerializer;
            this.playerDiffer = playerDiffer;
            this.obeliskDeserializer = obeliskDeserializer;
            this.obeliskSerializer = obeliskSerializer;
            this.obeliskDiffer = obeliskDiffer;
            this.mineDeserializer = mineDeserializer;
            this.mineSerializer = mineSerializer;
            this.mineDiffer = mineDiffer;
            this.dwellingDeserializer = dwellingDeserializer;
            this.dwellingSerializer = dwellingSerializer;
            this.dwellingDiffer = dwellingDiffer;
            this.merchantDeserializer = merchantDeserializer;
            this.merchantSerializer = merchantSerializer;
            this.merchantDiffer = merchantDiffer;
            this.patcher = patcher;
        }

        /// <inheritdoc/>
        public SaveDocument Create(
            byte[] bytes,
            IEnumerable<HeroChunk> heroChunks,
            HeroOffsets offsets,
            IEnumerable<PlayerChunk>? playerChunks = null,
            IEnumerable<CastleChunk>? castleChunks = null,
            IEnumerable<ObeliskChunk>? obeliskChunks = null,
            IEnumerable<MineChunk>? mineChunks = null,
            IEnumerable<DwellingChunk>? dwellingChunks = null,
            IEnumerable<MerchantStockChunk>? merchantChunks = null)
        {
            var entities = this.versionEntities.For(offsets.GameVersion);
            var heroes = new HeroChunkCollection(heroChunks, offsets, new HeroDeserializer(offsets, entities), new SafeHeroSerializer(offsets, entities), this.heroDiffer);
            var players = new PlayerChunkCollection(
                playerChunks ?? Enumerable.Empty<PlayerChunk>(),
                this.playerDeserializer,
                this.playerSerializer,
                this.playerDiffer,
                heroId => heroes.FindById(heroId)?.Name ?? $"Hero {heroId}");
            var castles = new CastleChunkCollection(castleChunks ?? Enumerable.Empty<CastleChunk>(), this.castleDeserializer, this.castleSerializer, this.castleDiffer);
            var obelisks = new ObeliskChunkCollection<PlayerColor>(obeliskChunks ?? Enumerable.Empty<ObeliskChunk>(), this.obeliskDeserializer, this.obeliskSerializer, this.obeliskDiffer);
            var mines = new MineChunkCollection(mineChunks ?? Enumerable.Empty<MineChunk>(), this.mineDeserializer, this.mineSerializer, this.mineDiffer);
            var merchants = new MerchantStockChunkCollection(merchantChunks ?? Enumerable.Empty<MerchantStockChunk>(), this.merchantDeserializer, this.merchantSerializer, this.merchantDiffer);
            var dwellings = new DwellingChunkCollection(dwellingChunks ?? Enumerable.Empty<DwellingChunk>(), this.dwellingDeserializer, this.dwellingSerializer, this.dwellingDiffer);
            return Homm3SaveDocuments.Create(bytes, heroes, players, castles, obelisks, mines, dwellings, merchants, this.patcher);
        }
    }
}
