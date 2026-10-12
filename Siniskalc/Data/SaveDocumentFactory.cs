namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

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
        private readonly IObeliskDiffer obeliskDiffer;
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
            IObeliskDiffer obeliskDiffer,
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
            this.patcher = patcher;
        }

        public SaveDocument Create(
            byte[] bytes,
            IEnumerable<HeroChunk> heroChunks,
            HeroOffsets offsets,
            IEnumerable<PlayerChunk>? playerChunks = null,
            IEnumerable<CastleChunk>? castleChunks = null,
            IEnumerable<ObeliskChunk>? obeliskChunks = null)
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
            var obelisks = new ObeliskChunkCollection(obeliskChunks ?? Enumerable.Empty<ObeliskChunk>(), this.obeliskDeserializer, this.obeliskSerializer, this.obeliskDiffer);
            return Homm3SaveDocuments.Create(bytes, heroes, players, castles, obelisks, this.patcher);
        }
    }
}
