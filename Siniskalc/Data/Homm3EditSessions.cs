namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Creates the edit sessions of a Heroes of Might and Magic III savefile on its <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public static class Homm3EditSessions
    {
        /// <summary>
        /// Creates a view onto the hero at <paramref name="heroIndex"/>; its edits are pushed onto this history.
        /// </summary>
        public static HeroEditSession CreateHeroSession(this SaveDocumentHistory history, int heroIndex) => new(history, heroIndex);

        /// <summary>
        /// Creates a view onto the player at <paramref name="playerIndex"/>; its edits are pushed onto this history.
        /// </summary>
        public static PlayerEditSession CreatePlayerSession(this SaveDocumentHistory history, int playerIndex) => new(history, playerIndex);

        /// <summary>
        /// Creates a view onto the castle at <paramref name="castleIndex"/>; its edits are pushed onto this history.
        /// </summary>
        public static CastleEditSession CreateCastleSession(this SaveDocumentHistory history, int castleIndex) => new(history, castleIndex);

        /// <summary>
        /// Creates a view onto the visited obelisks of the savefile (its only obelisk chunk, so index 0); its edits are pushed onto this history.
        /// </summary>
        public static ObeliskEditSession CreateObeliskSession(this SaveDocumentHistory history, int index = 0) => new(history, index);
    }
}
