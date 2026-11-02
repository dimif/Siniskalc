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
        /// <summary>
        /// Makes the document of a savegame from its collections.
        /// </summary>
        /// <param name="originalBytes">The bytes of the savefile as they were read.</param>
        /// <param name="heroes">The heroes.</param>
        /// <param name="players">The players.</param>
        /// <param name="castles">The castles.</param>
        /// <param name="obelisks">The obelisks.</param>
        /// <param name="mines">The mines.</param>
        /// <param name="shrines">The shrines.</param>
        /// <param name="sphinxes">The sphinxes.</param>
        /// <param name="turn">Whose turn it is.</param>
        /// <param name="builtToday">The castles that have built today.</param>
        /// <param name="patcher">Writes the edits into the bytes.</param>
        /// <param name="reconciler">Brings the chunks in line with the bytes when the savegame is reloaded.</param>
        /// <returns>The document.</returns>
        public static SaveDocument Create(byte[] originalBytes, HeroChunkCollection heroes, PlayerChunkCollection players, CastleChunkCollection castles, ObeliskChunkCollection<PlayerColor> obelisks, RecordChunkCollection<MapObjectChunk, MineModel> mines, RecordChunkCollection<MapObjectChunk, ShrineModel> shrines, RecordChunkCollection<MapObjectChunk, SphinxModel> sphinxes, RecordChunkCollection<MapObjectChunk, TurnModel> turn, RecordChunkCollection<MapObjectChunk, BuiltTodayModel> builtToday, IDocumentReconciler reconciler, ISavefileBytesPatcher patcher)
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

            return new SaveDocument(originalBytes, collections, patcher, reconciler);
        }

        /// <summary>
        /// Gets the heroes of a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of heroes.</returns>
        public static HeroChunkCollection Heroes(this SaveDocument document) => (HeroChunkCollection)document.GetCollection(Homm2ChunkKinds.Hero);

        /// <summary>
        /// Gets the players of a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of players.</returns>
        public static PlayerChunkCollection Players(this SaveDocument document) => (PlayerChunkCollection)document.GetCollection(Homm2ChunkKinds.Player);

        /// <summary>
        /// Gets the castles of a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of castles.</returns>
        public static CastleChunkCollection Castles(this SaveDocument document) => (CastleChunkCollection)document.GetCollection(Homm2ChunkKinds.Castle);

        /// <summary>
        /// Gets the mines of a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of mines.</returns>
        public static RecordChunkCollection<MapObjectChunk, MineModel> Mines(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, MineModel>)document.GetCollection(Homm2ChunkKinds.Mines);

        /// <summary>
        /// Gets the shrines of a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of shrines.</returns>
        public static RecordChunkCollection<MapObjectChunk, ShrineModel> Shrines(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, ShrineModel>)document.GetCollection(Homm2ChunkKinds.Shrines);

        /// <summary>
        /// Gets the sphinxes of a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of sphinxes.</returns>
        public static RecordChunkCollection<MapObjectChunk, SphinxModel> Sphinxes(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, SphinxModel>)document.GetCollection(Homm2ChunkKinds.Sphinxes);

        /// <summary>
        /// Gets whose turn it is in a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection that holds the turn.</returns>
        public static RecordChunkCollection<MapObjectChunk, TurnModel> Turn(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, TurnModel>)document.GetCollection(Homm2ChunkKinds.Turn);

        /// <summary>
        /// Gets the castles that have built today in a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection that holds the castles that have built today.</returns>
        public static RecordChunkCollection<MapObjectChunk, BuiltTodayModel> BuiltToday(this SaveDocument document) => (RecordChunkCollection<MapObjectChunk, BuiltTodayModel>)document.GetCollection(Homm2ChunkKinds.BuiltToday);

        /// <summary>
        /// Gets the obelisks of a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of obelisks.</returns>
        public static ObeliskChunkCollection<PlayerColor> Obelisks(this SaveDocument document) => (ObeliskChunkCollection<PlayerColor>)document.GetCollection(Homm2ChunkKinds.Obelisks);
    }
}
