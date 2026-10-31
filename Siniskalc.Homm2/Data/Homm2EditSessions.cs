namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
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
        public static EditSession<HeroChunk, HeroModel> CreateHeroSession(this SaveDocumentHistory history, int heroIndex) =>
            new(history, Homm2ChunkKinds.Hero, heroIndex, history.Current.Heroes().Deserializer, history.Current.Heroes().Serializer);

        public static PlayerEditSession CreatePlayerSession(this SaveDocumentHistory history, int playerIndex) => new(history, playerIndex);

        /// <summary>
        /// Opens the session of the castle at <paramref name="castleIndex"/>. A change of its owner is an edit like any other: the lists of
        /// castles of the players and their days left follow it (<see cref="PlayerListsReconciler"/>).
        /// </summary>
        public static EditSession<CastleChunk, CastleModel> CreateCastleSession(this SaveDocumentHistory history, int castleIndex) =>
            new(history, Homm2ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer);

        public static EditSession<MapObjectChunk, MineModel> CreateMineSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Mines, index);

        public static EditSession<MapObjectChunk, ShrineModel> CreateShrineSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Shrines, index);

        public static EditSession<MapObjectChunk, TurnModel> CreateTurnSession(this SaveDocumentHistory history) => new(history, Homm2ChunkKinds.Turn, 0);

        public static EditSession<MapObjectChunk, BuiltTodayModel> CreateBuiltTodaySession(this SaveDocumentHistory history) => new(history, Homm2ChunkKinds.BuiltToday, 0);

        public static EditSession<MapObjectChunk, SphinxModel> CreateSphinxSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Sphinxes, index);

        public static ObeliskEditSession<PlayerColor> CreateObeliskSession(this SaveDocumentHistory history, int index = 0) => new(history, Homm2ChunkKinds.Obelisks, index);
    }
}
