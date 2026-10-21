namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
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
        private readonly MapObjectKit<MineModel> mines;
        private readonly MapObjectKit<ShrineModel> shrines;
        private readonly MapObjectKit<SphinxModel> sphinxes;
        private readonly MapObjectKit<TurnModel> turn;
        private readonly MapObjectKit<BuiltTodayModel> builtToday;
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
            MapObjectKit<MineModel> mines,
            MapObjectKit<ShrineModel> shrines,
            MapObjectKit<SphinxModel> sphinxes,
            MapObjectKit<TurnModel> turn,
            MapObjectKit<BuiltTodayModel> builtToday,
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
                new MapObjectChunkCollection<MineModel>(objects.Mines, this.mines),
                new MapObjectChunkCollection<ShrineModel>(objects.Shrines, this.shrines),
                new MapObjectChunkCollection<SphinxModel>(objects.Sphinxes, this.sphinxes),
                new MapObjectChunkCollection<TurnModel>(objects.Turn, this.turn),
                new MapObjectChunkCollection<BuiltTodayModel>(objects.BuiltToday, this.builtToday),
                this.patcher);
        }
    }
}
