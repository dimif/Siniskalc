namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Creates a <see cref="SaveDocument"/> for a Heroes of Might and Magic III savefile and gives typed access to its heroes, players and castles.
    /// </summary>
    public static class Homm3SaveDocuments
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
        /// <param name="dwellings">The dwellings.</param>
        /// <param name="merchants">The merchants.</param>
        /// <param name="heroOwners">The owners of the heroes.</param>
        /// <param name="turn">Whose turn it is.</param>
        /// <param name="reconciler">Brings the chunks in line with the bytes when the savegame is reloaded.</param>
        /// <param name="patcher">Writes the edits into the bytes.</param>
        /// <param name="inactivePlayers">Hidden player records retained for town ownership edits.</param>
        /// <param name="townMerchant">The stock shared by every town Artifact Merchant.</param>
        /// <returns>The document.</returns>
        public static SaveDocument Create(byte[] originalBytes, HeroChunkCollection heroes, PlayerChunkCollection players, CastleChunkCollection castles, ObeliskChunkCollection<PlayerColor> obelisks, RecordChunkCollection<RecordChunk, MineModel> mines, RecordChunkCollection<RecordChunk, DwellingModel> dwellings, RecordChunkCollection<RecordChunk, MerchantStockModel> merchants, RecordChunkCollection<RecordChunk, HeroOwnersModel> heroOwners, RecordChunkCollection<RecordChunk, TurnModel> turn, IDocumentReconciler reconciler, ISavefileBytesPatcher patcher, PlayerChunkCollection inactivePlayers, RecordChunkCollection<RecordChunk, MerchantStockModel> townMerchant)
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [Homm3ChunkKinds.Hero] = heroes,
                [Homm3ChunkKinds.Player] = players,
                [Homm3ChunkKinds.Castle] = castles,
                [Homm3ChunkKinds.Obelisks] = obelisks,
                [Homm3ChunkKinds.Mines] = mines,
                [Homm3ChunkKinds.Dwellings] = dwellings,
                [Homm3ChunkKinds.Merchants] = merchants,
                [Homm3ChunkKinds.HeroOwners] = heroOwners,
                [Homm3ChunkKinds.Turn] = turn,
                [Homm3ChunkKinds.InactivePlayer] = inactivePlayers,
                [Homm3ChunkKinds.TownMerchant] = townMerchant,
            };

            return new SaveDocument(originalBytes, collections, patcher, reconciler);
        }

        /// <summary>
        /// Gets the heroes of a document.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of heroes.</returns>
        public static HeroChunkCollection Heroes(this SaveDocument document) => (HeroChunkCollection)document.GetCollection(Homm3ChunkKinds.Hero);

        /// <summary>
        /// Gets the players, in colour order. Empty when the savefile's players are not known.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of players.</returns>
        public static PlayerChunkCollection Players(this SaveDocument document) => (PlayerChunkCollection)document.GetCollection(Homm3ChunkKinds.Player);

        /// <summary>Gets hidden player records retained for ownership bookkeeping.</summary>
        /// <param name="document">The document.</param>
        /// <returns>The inactive player records.</returns>
        public static PlayerChunkCollection InactivePlayers(this SaveDocument document) => (PlayerChunkCollection)document.GetCollection(Homm3ChunkKinds.InactivePlayer);

        /// <summary>
        /// Gets the castles whose name is stored in the savefile.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of castles.</returns>
        public static CastleChunkCollection Castles(this SaveDocument document) => (CastleChunkCollection)document.GetCollection(Homm3ChunkKinds.Castle);

        /// <summary>
        /// Gets the visited obelisks: one chunk when the savefile has obelisks to show, otherwise none.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of obelisks.</returns>
        public static ObeliskChunkCollection<PlayerColor> Obelisks(this SaveDocument document) => (ObeliskChunkCollection<PlayerColor>)document.GetCollection(Homm3ChunkKinds.Obelisks);

        /// <summary>
        /// Gets the mines, in the order of the table of their records.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of mines.</returns>
        public static RecordChunkCollection<RecordChunk, MineModel> Mines(this SaveDocument document) => (RecordChunkCollection<RecordChunk, MineModel>)document.GetCollection(Homm3ChunkKinds.Mines);

        /// <summary>
        /// Gets the dwellings, in the order of the table of their records.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of dwellings.</returns>
        public static RecordChunkCollection<RecordChunk, DwellingModel> Dwellings(this SaveDocument document) => (RecordChunkCollection<RecordChunk, DwellingModel>)document.GetCollection(Homm3ChunkKinds.Dwellings);

        /// <summary>
        /// Gets the adventure-map Black Markets' stocks in type-7 object-number order, not the shared town Artifact Merchant stock.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection of merchants.</returns>
        public static RecordChunkCollection<RecordChunk, MerchantStockModel> Merchants(this SaveDocument document) => (RecordChunkCollection<RecordChunk, MerchantStockModel>)document.GetCollection(Homm3ChunkKinds.Merchants);

        /// <summary>Gets the single stock shared by all town Artifact Merchants, or none when its layout is not recognized.</summary>
        /// <param name="document">The document.</param>
        /// <returns>The shared stock collection.</returns>
        public static RecordChunkCollection<RecordChunk, MerchantStockModel> TownMerchant(this SaveDocument document) => (RecordChunkCollection<RecordChunk, MerchantStockModel>)document.GetCollection(Homm3ChunkKinds.TownMerchant);

        /// <summary>
        /// Gets the table of who owns each hero (<see cref="HeroOwnersLayout"/>): one chunk, or none when the savefile has no table the editor knows.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection that holds the owners of the heroes.</returns>
        public static RecordChunkCollection<RecordChunk, HeroOwnersModel> HeroOwners(this SaveDocument document) => (RecordChunkCollection<RecordChunk, HeroOwnersModel>)document.GetCollection(Homm3ChunkKinds.HeroOwners);

        /// <summary>
        /// Gets whose turn it is: one chunk when the setup of the players and its human flags are found, otherwise none.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The collection that holds whose turn it is.</returns>
        public static RecordChunkCollection<RecordChunk, TurnModel> Turn(this SaveDocument document) => (RecordChunkCollection<RecordChunk, TurnModel>)document.GetCollection(Homm3ChunkKinds.Turn);

        /// <summary>
        /// Gets the game version that wrote the savefile.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        public static GameVersion GameVersion(this SaveDocument document) => document.Heroes().Offsets.GameVersion;

        /// <summary>
        /// Gets the catalog the document was read with.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <returns>The catalog.</returns>
        public static EntityCatalog Catalog(this SaveDocument document) => document.Heroes().Catalog;

        /// <summary>
        /// Replaces the chunk of a hero.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <param name="index">The number of the hero.</param>
        /// <param name="chunk">The new chunk.</param>
        /// <returns>The new document; this one is not changed.</returns>
        public static SaveDocument WithHero(this SaveDocument document, int index, HeroChunk chunk) => document.WithChunk(Homm3ChunkKinds.Hero, index, chunk);

        /// <summary>
        /// Replaces the chunk of a player.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <param name="index">The place of the player's record, from 0.</param>
        /// <param name="chunk">The new chunk.</param>
        /// <returns>The new document; this one is not changed.</returns>
        public static SaveDocument WithPlayer(this SaveDocument document, int index, PlayerChunk chunk) => document.WithChunk(Homm3ChunkKinds.Player, index, chunk);

        /// <summary>
        /// Replaces the chunk of a castle.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <param name="index">The number of the castle.</param>
        /// <param name="chunk">The new chunk.</param>
        /// <returns>The new document; this one is not changed.</returns>
        public static SaveDocument WithCastle(this SaveDocument document, int index, CastleChunk chunk) => document.WithChunk(Homm3ChunkKinds.Castle, index, chunk);

        /// <summary>
        /// Replaces the chunk of the owners of the heroes.
        /// </summary>
        /// <param name="document">The document of the savegame.</param>
        /// <param name="chunk">The new chunk.</param>
        /// <returns>The new document; this one is not changed.</returns>
        public static SaveDocument WithHeroOwners(this SaveDocument document, RecordChunk chunk) => document.WithChunk(Homm3ChunkKinds.HeroOwners, 0, chunk);
    }
}
