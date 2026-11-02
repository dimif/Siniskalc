namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The kinds of struct a Heroes of Might and Magic II savegame is made of, in the order its pending changes are listed.
    /// </summary>
    public static class Homm2ChunkKinds
    {
        /// <summary>
        /// The kind of the chunks of the heroes.
        /// </summary>
        public static readonly ChunkKind Hero = new("Hero", 0);

        /// <summary>
        /// The kind of the chunks of the players.
        /// </summary>
        public static readonly ChunkKind Player = new("Player", 1);

        /// <summary>
        /// The kind of the chunks of the castles.
        /// </summary>
        public static readonly ChunkKind Castle = new("Castle", 2);

        /// <summary>
        /// The kind of the chunk of the obelisks.
        /// </summary>
        public static readonly ChunkKind Obelisks = new("Obelisks", 3);

        /// <summary>
        /// The kind of the chunks of the mines.
        /// </summary>
        public static readonly ChunkKind Mines = new("Mines", 4);

        /// <summary>
        /// The kind of the chunks of the shrines.
        /// </summary>
        public static readonly ChunkKind Shrines = new("Shrines", 5);

        /// <summary>
        /// The kind of the chunks of the sphinxes.
        /// </summary>
        public static readonly ChunkKind Sphinxes = new("Sphinxes", 6);

        /// <summary>
        /// The kind of the chunk of whose turn it is.
        /// </summary>
        public static readonly ChunkKind Turn = new("Turn", 7);

        /// <summary>
        /// The kind of the chunk of the castles that have built today.
        /// </summary>
        public static readonly ChunkKind BuiltToday = new("Built this turn", 8);
    }
}
