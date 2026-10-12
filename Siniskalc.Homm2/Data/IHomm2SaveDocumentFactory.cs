namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Builds the document of a savegame from its records.
    /// </summary>
    public interface IHomm2SaveDocumentFactory
    {
        SaveDocument Create(byte[] bytes, IEnumerable<Homm2HeroChunk> heroChunks, IEnumerable<Homm2PlayerChunk> playerChunks, IEnumerable<Homm2CastleChunk> castleChunks);
    }
}
