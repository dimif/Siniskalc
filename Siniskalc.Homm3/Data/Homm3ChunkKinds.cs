namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The kinds of struct a Heroes of Might and Magic III savefile is split into, in the order they are listed.
    /// </summary>
    public static class Homm3ChunkKinds
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
        /// The kind of the chunks of the dwellings.
        /// </summary>
        public static readonly ChunkKind Dwellings = new("Dwellings", 5);

        /// <summary>
        /// The kind of the chunks of the merchants.
        /// </summary>
        public static readonly ChunkKind Merchants = new("Merchants", 6);

        /// <summary>
        /// The kind of the chunk of the owners of the heroes.
        /// </summary>
        public static readonly ChunkKind HeroOwners = new("HeroOwners", 7);

        /// <summary>
        /// The kind of the chunk of whose turn it is.
        /// </summary>
        public static readonly ChunkKind Turn = new("Turn", 8);
    }
}
