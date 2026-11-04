namespace Siniskalc.Homm3.Data
{
    using System;
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
        private readonly IDiffer<HeroModel> heroDiffer;
        private readonly Func<Entities.EntityCatalog, ICastleDeserializer> castleDeserializer;
        private readonly ISerializer<CastleChunk, CastleModel> castleSerializer;
        private readonly ICastleDiffer castleDiffer;
        private readonly IPlayerDeserializer playerDeserializer;
        private readonly ISerializer<PlayerChunk, PlayerModel> playerSerializer;
        private readonly IDeserializer<PlayerChunk, PlayerLists> playerListsDeserializer;
        private readonly ISerializer<PlayerChunk, PlayerLists> playerListsSerializer;
        private readonly IPlayerDiffer playerDiffer;
        private readonly IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>> obeliskDeserializer;
        private readonly ISerializer<ObeliskChunk, ObeliskVisits<PlayerColor>> obeliskSerializer;
        private readonly IDiffer<ObeliskVisits<PlayerColor>> obeliskDiffer;
        private readonly Func<Entities.EntityCatalog, RecordKit<RecordChunk, MineModel>> mines;
        private readonly Func<Entities.EntityCatalog, RecordKit<RecordChunk, DwellingModel>> dwellings;
        private readonly Func<Entities.EntityCatalog, RecordKit<RecordChunk, MerchantStockModel>> merchants;
        private readonly Func<Entities.EntityCatalog, RecordKit<RecordChunk, MerchantStockModel>> townMerchant;
        private readonly RecordKit<RecordChunk, HeroOwnersModel> heroOwners;
        private readonly RecordKit<RecordChunk, TurnModel> turn;
        private readonly IDocumentReconciler reconciler;
        private readonly ISavefileBytesPatcher patcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveDocumentFactory"/> class.
        /// </summary>
        /// <param name="versionEntities">The version entities.</param>
        /// <param name="heroDiffer">The hero differ.</param>
        /// <param name="castleDeserializer">The castle deserializer.</param>
        /// <param name="castleSerializer">The castle serializer.</param>
        /// <param name="castleDiffer">The castle differ.</param>
        /// <param name="playerDeserializer">The player deserializer.</param>
        /// <param name="playerSerializer">The player serializer.</param>
        /// <param name="playerListsDeserializer">The player lists deserializer.</param>
        /// <param name="playerListsSerializer">The player lists serializer.</param>
        /// <param name="playerDiffer">The player differ.</param>
        /// <param name="obeliskDeserializer">The obelisk deserializer.</param>
        /// <param name="obeliskSerializer">The obelisk serializer.</param>
        /// <param name="obeliskDiffer">The obelisk differ.</param>
        /// <param name="mines">The mines.</param>
        /// <param name="dwellings">The dwellings.</param>
        /// <param name="merchants">The merchants.</param>
        /// <param name="townMerchant">The shared town stock.</param>
        /// <param name="heroOwners">The hero owners.</param>
        /// <param name="turn">The turn.</param>
        /// <param name="reconciler">The reconciler.</param>
        /// <param name="patcher">The patcher.</param>
        public SaveDocumentFactory(
            IVersionSpecificEntitiesProviderFactory versionEntities,
            IDiffer<HeroModel> heroDiffer,
            Func<Entities.EntityCatalog, ICastleDeserializer> castleDeserializer,
            ISerializer<CastleChunk, CastleModel> castleSerializer,
            ICastleDiffer castleDiffer,
            IPlayerDeserializer playerDeserializer,
            ISerializer<PlayerChunk, PlayerModel> playerSerializer,
            IDeserializer<PlayerChunk, PlayerLists> playerListsDeserializer,
            ISerializer<PlayerChunk, PlayerLists> playerListsSerializer,
            IPlayerDiffer playerDiffer,
            IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>> obeliskDeserializer,
            ISerializer<ObeliskChunk, ObeliskVisits<PlayerColor>> obeliskSerializer,
            IDiffer<ObeliskVisits<PlayerColor>> obeliskDiffer,
            Func<Entities.EntityCatalog, RecordKit<RecordChunk, MineModel>> mines,
            Func<Entities.EntityCatalog, RecordKit<RecordChunk, DwellingModel>> dwellings,
            Func<Entities.EntityCatalog, RecordKit<RecordChunk, MerchantStockModel>> merchants,
            Func<Entities.EntityCatalog, RecordKit<RecordChunk, MerchantStockModel>> townMerchant,
            RecordKit<RecordChunk, HeroOwnersModel> heroOwners,
            RecordKit<RecordChunk, TurnModel> turn,
            IDocumentReconciler reconciler,
            ISavefileBytesPatcher patcher)
        {
            this.versionEntities = versionEntities;
            this.heroDiffer = heroDiffer;
            this.castleDeserializer = castleDeserializer;
            this.castleSerializer = castleSerializer;
            this.castleDiffer = castleDiffer;
            this.playerDeserializer = playerDeserializer;
            this.playerSerializer = playerSerializer;
            this.playerListsDeserializer = playerListsDeserializer;
            this.playerListsSerializer = playerListsSerializer;
            this.playerDiffer = playerDiffer;
            this.obeliskDeserializer = obeliskDeserializer;
            this.obeliskSerializer = obeliskSerializer;
            this.obeliskDiffer = obeliskDiffer;
            this.mines = mines;
            this.dwellings = dwellings;
            this.merchants = merchants;
            this.townMerchant = townMerchant;
            this.heroOwners = heroOwners;
            this.turn = turn;
            this.reconciler = reconciler;
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
            IEnumerable<RecordChunk>? merchantChunks = null,
            IEnumerable<RecordChunk>? heroOwnerChunks = null,
            IEnumerable<RecordChunk>? turnChunks = null,
            IEnumerable<PlayerChunk>? inactivePlayerChunks = null,
            IEnumerable<RecordChunk>? townMerchantChunks = null)
        {
            var entities = this.versionEntities.For(offsets.GameVersion);
            var heroes = new HeroChunkCollection(heroChunks, offsets, new HeroDeserializer(offsets, entities), new SafeHeroSerializer(offsets, entities), this.heroDiffer, entities.Catalog);
            var players = new PlayerChunkCollection(
                playerChunks ?? Enumerable.Empty<PlayerChunk>(),
                this.playerDeserializer,
                this.playerSerializer,
                this.playerListsDeserializer,
                this.playerListsSerializer,
                this.playerDiffer,
                heroId => heroes.FindById(heroId)?.Name ?? $"Hero {heroId}");
            var castles = new CastleChunkCollection(castleChunks ?? Enumerable.Empty<CastleChunk>(), this.castleDeserializer(entities.Catalog), this.castleSerializer, this.castleDiffer);
            var obelisks = new ObeliskChunkCollection<PlayerColor>(obeliskChunks ?? Enumerable.Empty<ObeliskChunk>(), this.obeliskDeserializer, this.obeliskSerializer, this.obeliskDiffer);
            var mines = new RecordChunkCollection<RecordChunk, MineModel>(mineChunks ?? Enumerable.Empty<RecordChunk>(), this.mines(entities.Catalog));
            var merchants = new RecordChunkCollection<RecordChunk, MerchantStockModel>(merchantChunks ?? Enumerable.Empty<RecordChunk>(), this.merchants(entities.Catalog));
            var townMerchant = new RecordChunkCollection<RecordChunk, MerchantStockModel>(townMerchantChunks ?? Enumerable.Empty<RecordChunk>(), this.townMerchant(entities.Catalog));
            var dwellings = new RecordChunkCollection<RecordChunk, DwellingModel>(dwellingChunks ?? Enumerable.Empty<RecordChunk>(), this.dwellings(entities.Catalog));
            var heroOwners = new RecordChunkCollection<RecordChunk, HeroOwnersModel>(heroOwnerChunks ?? Enumerable.Empty<RecordChunk>(), this.heroOwners);
            var turn = new RecordChunkCollection<RecordChunk, TurnModel>(turnChunks ?? Enumerable.Empty<RecordChunk>(), this.turn);
            var inactivePlayers = new PlayerChunkCollection(
                inactivePlayerChunks ?? Enumerable.Empty<PlayerChunk>(),
                this.playerDeserializer,
                this.playerSerializer,
                this.playerListsDeserializer,
                this.playerListsSerializer,
                this.playerDiffer,
                heroId => heroes.FindById(heroId)?.Name ?? $"Hero {heroId}");
            return Homm3SaveDocuments.Create(bytes, heroes, players, castles, obelisks, mines, dwellings, merchants, heroOwners, turn, this.reconciler, this.patcher, inactivePlayers, townMerchant);
        }
    }
}
