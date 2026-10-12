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
    }
}
