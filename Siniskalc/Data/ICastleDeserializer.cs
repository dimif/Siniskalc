namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Reads a castle from its chunk.
    /// </summary>
    public interface ICastleDeserializer : IDeserializer<CastleChunk, CastleModel>
    {
        Faction? DeserializeFaction(CastleChunk chunk);

        PlayerColor? DeserializeOwner(CastleChunk chunk);

        Position DeserializePosition(CastleChunk chunk);

        Position? DeserializeShipyardLocation(CastleChunk chunk);

        Army DeserializeGarrison(CastleChunk chunk);

        IReadOnlySet<Building> DeserializeBuiltBuildings(CastleChunk chunk);

        IReadOnlySet<Building> DeserializeCurrentBuildings(CastleChunk chunk);
    }
}
