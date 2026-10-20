namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Opens the edit session of a hero, a player or a castle of a savegame's history.
    /// </summary>
    public static class Homm2EditSessions
    {
        public static HeroEditSession CreateHeroSession(this SaveDocumentHistory history, int heroIndex) => new(history, heroIndex);

        public static PlayerEditSession CreatePlayerSession(this SaveDocumentHistory history, int playerIndex) => new(history, playerIndex);

        public static CastleEditSession CreateCastleSession(this SaveDocumentHistory history, int castleIndex) => new(history, castleIndex);

        public static MapObjectEditSession<MineModel> CreateMineSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Mines, index);

        public static MapObjectEditSession<ShrineModel> CreateShrineSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Shrines, index);

        public static MapObjectEditSession<TurnModel> CreateTurnSession(this SaveDocumentHistory history) => new(history, Homm2ChunkKinds.Turn, 0);

        public static MapObjectEditSession<SphinxModel> CreateSphinxSession(this SaveDocumentHistory history, int index) => new(history, Homm2ChunkKinds.Sphinxes, index);

        public static ObeliskEditSession<PlayerColor> CreateObeliskSession(this SaveDocumentHistory history, int index = 0) => new(history, Homm2ChunkKinds.Obelisks, index);
    }
}
