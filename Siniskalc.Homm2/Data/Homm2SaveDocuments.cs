namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Makes the document of a savegame and reads its collections.
    /// </summary>
    public static class Homm2SaveDocuments
    {
        public static SaveDocument Create(byte[] originalBytes, HeroChunkCollection heroes, PlayerChunkCollection players, CastleChunkCollection castles, ObeliskChunkCollection obelisks, ISavefileBytesPatcher patcher)
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [Homm2ChunkKinds.Hero] = heroes,
                [Homm2ChunkKinds.Player] = players,
                [Homm2ChunkKinds.Castle] = castles,
                [Homm2ChunkKinds.Obelisks] = obelisks,
            };

            return new SaveDocument(originalBytes, collections, patcher);
        }

        public static HeroChunkCollection Heroes(this SaveDocument document) => (HeroChunkCollection)document.GetCollection(Homm2ChunkKinds.Hero);

        public static PlayerChunkCollection Players(this SaveDocument document) => (PlayerChunkCollection)document.GetCollection(Homm2ChunkKinds.Player);

        public static CastleChunkCollection Castles(this SaveDocument document) => (CastleChunkCollection)document.GetCollection(Homm2ChunkKinds.Castle);

        public static ObeliskChunkCollection Obelisks(this SaveDocument document) => (ObeliskChunkCollection)document.GetCollection(Homm2ChunkKinds.Obelisks);
    }
}
