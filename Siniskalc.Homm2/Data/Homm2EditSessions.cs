namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Opens the edit session of a hero, a player or a castle of a savegame's history.
    /// </summary>
    public static class Homm2EditSessions
    {
        public static Homm2HeroEditSession CreateHeroSession(this SaveDocumentHistory history, int heroIndex) => new(history, heroIndex);

        public static Homm2PlayerEditSession CreatePlayerSession(this SaveDocumentHistory history, int playerIndex) => new(history, playerIndex);

        public static Homm2CastleEditSession CreateCastleSession(this SaveDocumentHistory history, int castleIndex) => new(history, castleIndex);
    }
}
