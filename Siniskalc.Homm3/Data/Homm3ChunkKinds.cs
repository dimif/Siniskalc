namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The kinds of struct a Heroes of Might and Magic III savefile is split into, in the order they are listed.
    /// </summary>
    public static class Homm3ChunkKinds
    {
        public static readonly ChunkKind Hero = new("Hero", 0);

        public static readonly ChunkKind Player = new("Player", 1);

        public static readonly ChunkKind Castle = new("Castle", 2);

        public static readonly ChunkKind Obelisks = new("Obelisks", 3);

        public static readonly ChunkKind Mines = new("Mines", 4);

        public static readonly ChunkKind Dwellings = new("Dwellings", 5);

        public static readonly ChunkKind Merchants = new("Merchants", 6);

        public static readonly ChunkKind HeroOwners = new("HeroOwners", 7);
    }
}
