namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Makes the document of a savegame and reads its collections.
    /// </summary>
    public static class Homm2SaveDocuments
    {
        public static SaveDocument Create(byte[] originalBytes, HeroChunkCollection heroes, PlayerChunkCollection players, CastleChunkCollection castles, ObeliskChunkCollection<PlayerColor> obelisks, RecordChunkCollection<MapObjectChunk, MineModel> mines, RecordChunkCollection<MapObjectChunk, ShrineModel> shrines, RecordChunkCollection<MapObjectChunk, SphinxModel> sphinxes, RecordChunkCollection<MapObjectChunk, TurnModel> turn, RecordChunkCollection<MapObjectChunk, BuiltTodayModel> builtToday, ISavefileBytesPatcher patcher)
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [Homm2ChunkKinds.Hero] = heroes,
                [Homm2ChunkKinds.Player] = players,
                [Homm2ChunkKinds.Castle] = castles,
                [Homm2ChunkKinds.Obelisks] = obelisks,
                [Homm2ChunkKinds.Mines] = mines,
                [Homm2ChunkKinds.Shrines] = shrines,
                [Homm2ChunkKinds.Sphinxes] = sphinxes,
                [Homm2ChunkKinds.Turn] = turn,
                [Homm2ChunkKinds.BuiltToday] = builtToday,
            };

            return new SaveDocument(originalBytes, collections, patcher);
        }

        public static HeroChunkCollection Heroes(this SaveDocument document) => (HeroChunkCollection)document.GetCollection(Homm2ChunkKinds.Hero);

        public static PlayerChunkCollection Players(this SaveDocument document) => (PlayerChunkCollection)document.GetCollection(Homm2ChunkKinds.Player);

        public static CastleChunkCollection Castles(this SaveDocument document) => (CastleChunkCollection)document.GetCollection(Homm2ChunkKinds.Castle);

        public static RecordChunkCollection<MapObjectChunk, MineModel> Mines(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, MineModel>)document.GetCollection(Homm2ChunkKinds.Mines);

        public static RecordChunkCollection<MapObjectChunk, ShrineModel> Shrines(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, ShrineModel>)document.GetCollection(Homm2ChunkKinds.Shrines);

        public static RecordChunkCollection<MapObjectChunk, SphinxModel> Sphinxes(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, SphinxModel>)document.GetCollection(Homm2ChunkKinds.Sphinxes);

        public static RecordChunkCollection<MapObjectChunk, TurnModel> Turn(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, TurnModel>)document.GetCollection(Homm2ChunkKinds.Turn);

        public static RecordChunkCollection<MapObjectChunk, BuiltTodayModel> BuiltToday(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, BuiltTodayModel>)document.GetCollection(Homm2ChunkKinds.BuiltToday);

        public static ObeliskChunkCollection<PlayerColor> Obelisks(this SaveDocument document) => (ObeliskChunkCollection<PlayerColor>)document.GetCollection(Homm2ChunkKinds.Obelisks);
    }
}
