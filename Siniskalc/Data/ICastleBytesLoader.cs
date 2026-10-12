namespace Siniskalc.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Finds the castles (towns) in a savefile and loads each as a <see cref="CastleChunk"/> named after the castle.
    /// </summary>
    public interface ICastleBytesLoader
    {
        IReadOnlyList<CastleChunk> LoadCastleChunks(byte[] bytes);
    }
}
