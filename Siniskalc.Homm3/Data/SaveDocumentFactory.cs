namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

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
        private readonly RecordKit<RecordChunk, MineModel> mines;
        private readonly RecordKit<RecordChunk, DwellingModel> dwellings;
        private readonly RecordKit<RecordChunk, MerchantStockModel> merchants;
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
            RecordKit<RecordChunk, MineModel> mines,
            RecordKit<RecordChunk, DwellingModel> dwellings,
            RecordKit<RecordChunk, MerchantStockModel> merchants,
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
            this.mines = mines;
            this.dwellings = dwellings;
            this.merchants = merchants;
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
            IEnumerable<RecordChunk>? mineChunks = null,
            IEnumerable<RecordChunk>? dwellingChunks = null,
            IEnumerable<RecordChunk>? merchantChunks = null)
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
            var mines = new RecordChunkCollection<RecordChunk, MineModel>(mineChunks ?? Enumerable.Empty<RecordChunk>(), this.mines);
            var merchants = new RecordChunkCollection<RecordChunk, MerchantStockModel>(merchantChunks ?? Enumerable.Empty<RecordChunk>(), this.merchants);
            var dwellings = new RecordChunkCollection<RecordChunk, DwellingModel>(dwellingChunks ?? Enumerable.Empty<RecordChunk>(), this.dwellings);
            return Homm3SaveDocuments.Create(bytes, heroes, players, castles, obelisks, mines, dwellings, merchants, this.patcher);
        }
    }
}
