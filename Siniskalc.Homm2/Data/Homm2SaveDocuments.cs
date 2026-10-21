namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Data;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Makes the document of a savegame and reads its collections.
    /// </summary>
    public static class Homm2SaveDocuments
    {
        public static SaveDocument Create(byte[] originalBytes, HeroChunkCollection heroes, PlayerChunkCollection players, CastleChunkCollection castles, ObeliskChunkCollection<PlayerColor> obelisks, MapObjectChunkCollection<MineModel> mines, MapObjectChunkCollection<ShrineModel> shrines, MapObjectChunkCollection<SphinxModel> sphinxes, MapObjectChunkCollection<TurnModel> turn, MapObjectChunkCollection<BuiltTodayModel> builtToday, ISavefileBytesPatcher patcher)
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

        public static MapObjectChunkCollection<MineModel> Mines(this SaveDocument document) => (MapObjectChunkCollection<MineModel>)document.GetCollection(Homm2ChunkKinds.Mines);

        public static MapObjectChunkCollection<ShrineModel> Shrines(this SaveDocument document) => (MapObjectChunkCollection<ShrineModel>)document.GetCollection(Homm2ChunkKinds.Shrines);

        public static MapObjectChunkCollection<SphinxModel> Sphinxes(this SaveDocument document) => (MapObjectChunkCollection<SphinxModel>)document.GetCollection(Homm2ChunkKinds.Sphinxes);

        public static MapObjectChunkCollection<TurnModel> Turn(this SaveDocument document) => (MapObjectChunkCollection<TurnModel>)document.GetCollection(Homm2ChunkKinds.Turn);

        public static MapObjectChunkCollection<BuiltTodayModel> BuiltToday(this SaveDocument document) => (MapObjectChunkCollection<BuiltTodayModel>)document.GetCollection(Homm2ChunkKinds.BuiltToday);

        public static ObeliskChunkCollection<PlayerColor> Obelisks(this SaveDocument document) => (ObeliskChunkCollection<PlayerColor>)document.GetCollection(Homm2ChunkKinds.Obelisks);
    }
}
