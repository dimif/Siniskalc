namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The kinds of struct a Heroes of Might and Magic II savegame is made of, in the order its pending changes are listed.
    /// </summary>
    public static class Homm2ChunkKinds
    {
        public static readonly ChunkKind Hero = new("Hero", 0);

        public static readonly ChunkKind Player = new("Player", 1);

        public static readonly ChunkKind Castle = new("Castle", 2);

        public static readonly ChunkKind Obelisks = new("Obelisks", 3);

        public static readonly ChunkKind Mines = new("Mines", 4);

        public static readonly ChunkKind Shrines = new("Shrines", 5);

        public static readonly ChunkKind Sphinxes = new("Sphinxes", 6);

        public static readonly ChunkKind Turn = new("Turn", 7);
    }
}
