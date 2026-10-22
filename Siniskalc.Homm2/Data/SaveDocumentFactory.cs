namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Builds the document of a savegame, giving each collection the deserializer, the serializer and the differ of its kind.
    /// </summary>
    public sealed class SaveDocumentFactory : ISaveDocumentFactory
    {
        private readonly IHeroDeserializer heroDeserializer;
        private readonly IHeroSerializer heroSerializer;
        private readonly IHeroDiffer heroDiffer;
        private readonly IPlayerDeserializer playerDeserializer;
        private readonly IPlayerSerializer playerSerializer;
        private readonly IPlayerDiffer playerDiffer;
        private readonly ICastleDeserializer castleDeserializer;
        private readonly ICastleSerializer castleSerializer;
        private readonly ICastleDiffer castleDiffer;
        private readonly IObeliskDeserializer obeliskDeserializer;
        private readonly IObeliskSerializer obeliskSerializer;
        private readonly IDiffer<ObeliskVisits<PlayerColor>> obeliskDiffer;
        private readonly RecordKit<MapObjectChunk, MineModel> mines;
        private readonly RecordKit<MapObjectChunk, ShrineModel> shrines;
        private readonly RecordKit<MapObjectChunk, SphinxModel> sphinxes;
        private readonly RecordKit<MapObjectChunk, TurnModel> turn;
        private readonly RecordKit<MapObjectChunk, BuiltTodayModel> builtToday;
        private readonly ISavefileBytesPatcher patcher;

        public SaveDocumentFactory(
            IHeroDeserializer heroDeserializer,
            IHeroSerializer heroSerializer,
            IHeroDiffer heroDiffer,
            IPlayerDeserializer playerDeserializer,
            IPlayerSerializer playerSerializer,
            IPlayerDiffer playerDiffer,
            ICastleDeserializer castleDeserializer,
            ICastleSerializer castleSerializer,
            ICastleDiffer castleDiffer,
            IObeliskDeserializer obeliskDeserializer,
            IObeliskSerializer obeliskSerializer,
            IDiffer<ObeliskVisits<PlayerColor>> obeliskDiffer,
            RecordKit<MapObjectChunk, MineModel> mines,
            RecordKit<MapObjectChunk, ShrineModel> shrines,
            RecordKit<MapObjectChunk, SphinxModel> sphinxes,
            RecordKit<MapObjectChunk, TurnModel> turn,
            RecordKit<MapObjectChunk, BuiltTodayModel> builtToday,
            ISavefileBytesPatcher patcher)
        {
            this.heroDeserializer = heroDeserializer;
            this.heroSerializer = heroSerializer;
            this.heroDiffer = heroDiffer;
            this.playerDeserializer = playerDeserializer;
            this.playerSerializer = playerSerializer;
            this.playerDiffer = playerDiffer;
            this.castleDeserializer = castleDeserializer;
            this.castleSerializer = castleSerializer;
            this.castleDiffer = castleDiffer;
            this.obeliskDeserializer = obeliskDeserializer;
            this.obeliskSerializer = obeliskSerializer;
            this.obeliskDiffer = obeliskDiffer;
            this.mines = mines;
            this.shrines = shrines;
            this.sphinxes = sphinxes;
            this.turn = turn;
            this.builtToday = builtToday;
            this.patcher = patcher;
        }

        /// <inheritdoc/>
        public SaveDocument Create(byte[] bytes, IEnumerable<HeroChunk> heroChunks, IEnumerable<PlayerChunk> playerChunks, IEnumerable<CastleChunk> castleChunks, IEnumerable<ObeliskChunk> obeliskChunks, MapObjectChunks objects)
        {
            var heroes = new HeroChunkCollection(heroChunks, this.heroDeserializer, this.heroSerializer, this.heroDiffer);
            return Homm2SaveDocuments.Create(
                bytes,
                heroes,
                new PlayerChunkCollection(playerChunks, this.playerDeserializer, this.playerSerializer, this.playerDiffer, heroId => heroes.FindById(heroId)?.Name ?? $"Hero {heroId}"),
                new CastleChunkCollection(castleChunks, this.castleDeserializer, this.castleSerializer, this.castleDiffer),
                new ObeliskChunkCollection<PlayerColor>(obeliskChunks, this.obeliskDeserializer, this.obeliskSerializer, this.obeliskDiffer),
                new RecordChunkCollection<MapObjectChunk, MineModel>(objects.Mines, this.mines),
                new RecordChunkCollection<MapObjectChunk, ShrineModel>(objects.Shrines, this.shrines),
                new RecordChunkCollection<MapObjectChunk, SphinxModel>(objects.Sphinxes, this.sphinxes),
                new RecordChunkCollection<MapObjectChunk, TurnModel>(objects.Turn, this.turn),
                new RecordChunkCollection<MapObjectChunk, BuiltTodayModel>(objects.BuiltToday, this.builtToday),
                this.patcher);
        }
    }
}
