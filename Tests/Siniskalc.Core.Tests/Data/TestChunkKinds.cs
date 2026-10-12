namespace Siniskalc.Core.Tests.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Two kinds of struct for a made-up game, so the tests of the document need no real game.
    /// </summary>
    internal static class TestChunkKinds
    {
        public static readonly ChunkKind Unit = new("Unit", 0);

        public static readonly ChunkKind Building = new("Building", 1);
    }
}
