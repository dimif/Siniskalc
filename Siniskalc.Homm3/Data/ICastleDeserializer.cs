namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads a castle from its chunk.
    /// </summary>
    public interface ICastleDeserializer : IDeserializer<CastleChunk, CastleModel>
    {
        Faction? DeserializeFaction(CastleChunk chunk);

        PlayerColor? DeserializeOwner(CastleChunk chunk);

        MapPosition DeserializePosition(CastleChunk chunk);

        MapPosition? DeserializeShipyardLocation(CastleChunk chunk);

        Army<Creature> DeserializeGarrison(CastleChunk chunk);

        IReadOnlySet<Building> DeserializeBuiltBuildings(CastleChunk chunk);

        IReadOnlySet<Building> DeserializeCurrentBuildings(CastleChunk chunk);
    }
}
