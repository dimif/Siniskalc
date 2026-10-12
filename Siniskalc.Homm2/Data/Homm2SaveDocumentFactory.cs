namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Builds the document of a savegame, giving each collection the deserializer, the serializer and the differ of its kind.
    /// </summary>
    public sealed class Homm2SaveDocumentFactory : IHomm2SaveDocumentFactory
    {
        private readonly IHomm2HeroDeserializer heroDeserializer;
        private readonly IHomm2HeroSerializer heroSerializer;
        private readonly IHomm2HeroDiffer heroDiffer;
        private readonly IHomm2PlayerDeserializer playerDeserializer;
        private readonly IHomm2PlayerSerializer playerSerializer;
        private readonly IHomm2PlayerDiffer playerDiffer;
        private readonly IHomm2CastleDeserializer castleDeserializer;
        private readonly IHomm2CastleSerializer castleSerializer;
        private readonly IHomm2CastleDiffer castleDiffer;
        private readonly ISavefileBytesPatcher patcher;

        public Homm2SaveDocumentFactory(
            IHomm2HeroDeserializer heroDeserializer,
            IHomm2HeroSerializer heroSerializer,
            IHomm2HeroDiffer heroDiffer,
            IHomm2PlayerDeserializer playerDeserializer,
            IHomm2PlayerSerializer playerSerializer,
            IHomm2PlayerDiffer playerDiffer,
            IHomm2CastleDeserializer castleDeserializer,
            IHomm2CastleSerializer castleSerializer,
            IHomm2CastleDiffer castleDiffer,
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
            this.patcher = patcher;
        }

        /// <inheritdoc/>
        public SaveDocument Create(byte[] bytes, IEnumerable<Homm2HeroChunk> heroChunks, IEnumerable<Homm2PlayerChunk> playerChunks, IEnumerable<Homm2CastleChunk> castleChunks)
        {
            return Homm2SaveDocuments.Create(
                bytes,
                new Homm2HeroChunkCollection(heroChunks, this.heroDeserializer, this.heroSerializer, this.heroDiffer),
                new Homm2PlayerChunkCollection(playerChunks, this.playerDeserializer, this.playerSerializer, this.playerDiffer),
                new Homm2CastleChunkCollection(castleChunks, this.castleDeserializer, this.castleSerializer, this.castleDiffer),
                this.patcher);
        }
    }
}
