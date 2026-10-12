namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;

    /// <summary>
    /// Creates a <see cref="SaveDocument"/> for a Heroes of Might and Magic III savefile and gives typed access to its heroes, players and castles.
    /// </summary>
    public static class Homm3SaveDocuments
    {
        public static SaveDocument Create(byte[] originalBytes, HeroChunkCollection heroes, PlayerChunkCollection players, CastleChunkCollection castles, ObeliskChunkCollection obelisks, ISavefileBytesPatcher patcher)
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [Homm3ChunkKinds.Hero] = heroes,
                [Homm3ChunkKinds.Player] = players,
                [Homm3ChunkKinds.Castle] = castles,
                [Homm3ChunkKinds.Obelisks] = obelisks,
            };

            return new SaveDocument(originalBytes, collections, patcher);
        }

        public static HeroChunkCollection Heroes(this SaveDocument document) => (HeroChunkCollection)document.GetCollection(Homm3ChunkKinds.Hero);

        /// <summary>
        /// Gets the players, in colour order. Empty when the savefile's players are not known.
        /// </summary>
        public static PlayerChunkCollection Players(this SaveDocument document) => (PlayerChunkCollection)document.GetCollection(Homm3ChunkKinds.Player);

        /// <summary>
        /// Gets the castles whose name is stored in the savefile.
        /// </summary>
        public static CastleChunkCollection Castles(this SaveDocument document) => (CastleChunkCollection)document.GetCollection(Homm3ChunkKinds.Castle);

        /// <summary>
        /// Gets the visited obelisks: one chunk when the savefile has obelisks to show, otherwise none.
        /// </summary>
        public static ObeliskChunkCollection Obelisks(this SaveDocument document) => (ObeliskChunkCollection)document.GetCollection(Homm3ChunkKinds.Obelisks);

        /// <summary>
        /// Gets the game version that wrote the savefile.
        /// </summary>
        public static GameVersion GameVersion(this SaveDocument document) => document.Heroes().Offsets.GameVersion;

        public static SaveDocument WithHero(this SaveDocument document, int index, HeroChunk chunk) => document.WithChunk(Homm3ChunkKinds.Hero, index, chunk);

        public static SaveDocument WithPlayer(this SaveDocument document, int index, PlayerChunk chunk) => document.WithChunk(Homm3ChunkKinds.Player, index, chunk);

        public static SaveDocument WithCastle(this SaveDocument document, int index, CastleChunk chunk) => document.WithChunk(Homm3ChunkKinds.Castle, index, chunk);
    }
}
