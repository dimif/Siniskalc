namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Makes the document of a savegame and reads its collections.
    /// </summary>
    public static class Homm2SaveDocuments
    {
        public static SaveDocument Create(byte[] originalBytes, Homm2HeroChunkCollection heroes, Homm2PlayerChunkCollection players, Homm2CastleChunkCollection castles, ISavefileBytesPatcher patcher)
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [Homm2ChunkKinds.Hero] = heroes,
                [Homm2ChunkKinds.Player] = players,
                [Homm2ChunkKinds.Castle] = castles,
            };

            return new SaveDocument(originalBytes, collections, patcher);
        }

        public static Homm2HeroChunkCollection Heroes(this SaveDocument document) => (Homm2HeroChunkCollection)document.GetCollection(Homm2ChunkKinds.Hero);

        public static Homm2PlayerChunkCollection Players(this SaveDocument document) => (Homm2PlayerChunkCollection)document.GetCollection(Homm2ChunkKinds.Player);

        public static Homm2CastleChunkCollection Castles(this SaveDocument document) => (Homm2CastleChunkCollection)document.GetCollection(Homm2ChunkKinds.Castle);
    }
}
