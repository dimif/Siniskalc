namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Opens the edit session of a hero, a player or a castle of a savegame's history.
    /// </summary>
    public static class Homm2EditSessions
    {
        /// <summary>
        /// Opens the session of the hero at <paramref name="heroIndex"/>. A change of its owner is an edit like any other: the lists of heroes
        /// of the players follow it (<see cref="PlayerListsReconciler"/>).
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="heroIndex">The number of the hero.</param>
        /// <returns>The session.</returns>
        public static EditSession<HeroChunk, HeroModel> CreateHeroSession(this SaveDocumentHistory history, int heroIndex) =>
            new(history, Homm2ChunkKinds.Hero, heroIndex, history.Current.Heroes().Deserializer, history.Current.Heroes().Serializer);

        /// <summary>
        /// Starts an edit session of a player.
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="playerIndex">The place of the player's record, from 0.</param>
        /// <returns>The session.</returns>
        public static PlayerEditSession CreatePlayerSession(this SaveDocumentHistory history, int playerIndex) => new(history, playerIndex);

        /// <summary>
        /// Opens the session of the castle at <paramref name="castleIndex"/>. A change of its owner is an edit like any other: the lists of
        /// castles of the players and their days left follow it (<see cref="PlayerListsReconciler"/>).
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="castleIndex">The number of the castle.</param>
        /// <returns>The session.</returns>
        public static EditSession<CastleChunk, CastleModel> CreateCastleSession(this SaveDocumentHistory history, int castleIndex) =>
            new(history, Homm2ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer);

        /// <summary>
        /// Starts an edit session of a mine.
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="index">The place of the mine in the list, from 0.</param>
        /// <returns>The session.</returns>
        public static EditSession<MapObjectChunk, MineModel> CreateMineSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Mines, index);

        /// <summary>
        /// Starts an edit session of a shrine.
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="index">The place of the shrine in the list, from 0.</param>
        /// <returns>The session.</returns>
        public static EditSession<MapObjectChunk, ShrineModel> CreateShrineSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Shrines, index);

        /// <summary>
        /// Starts an edit session of whose turn it is.
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <returns>The session.</returns>
        public static EditSession<MapObjectChunk, TurnModel> CreateTurnSession(this SaveDocumentHistory history) => new(history, Homm2ChunkKinds.Turn, 0);

        /// <summary>
        /// Starts an edit session of the castles that have built today.
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <returns>The session.</returns>
        public static EditSession<MapObjectChunk, BuiltTodayModel> CreateBuiltTodaySession(this SaveDocumentHistory history) => new(history, Homm2ChunkKinds.BuiltToday, 0);

        /// <summary>
        /// Starts an edit session of a sphinx.
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="index">The place of the sphinx in the list, from 0.</param>
        /// <returns>The session.</returns>
        public static EditSession<MapObjectChunk, SphinxModel> CreateSphinxSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Sphinxes, index);

        /// <summary>
        /// Starts an edit session of the obelisks.
        /// </summary>
        /// <param name="history">The history of the savegame.</param>
        /// <returns>The session.</returns>
        /// <param name="index">The place of the chunk of obelisks; there is one.</param>
        public static EditSession<ObeliskChunk, ObeliskVisits<PlayerColor>> CreateObeliskSession(this SaveDocumentHistory history, int index = 0) => new(history, Homm2ChunkKinds.Obelisks, index);
    }
}
