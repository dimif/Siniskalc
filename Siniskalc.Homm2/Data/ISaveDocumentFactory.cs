namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Data;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Builds the document of a savegame from its records.
    /// </summary>
    public interface ISaveDocumentFactory
    {
        SaveDocument Create(byte[] bytes, IEnumerable<HeroChunk> heroChunks, IEnumerable<PlayerChunk> playerChunks, IEnumerable<CastleChunk> castleChunks, IEnumerable<ObeliskChunk> obeliskChunks, MapObjectChunks objects);
    }
}
