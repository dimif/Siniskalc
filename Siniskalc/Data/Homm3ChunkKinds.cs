namespace Siniskalc.Data
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
    }
}
