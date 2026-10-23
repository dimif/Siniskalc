namespace Siniskalc.Homm2.Data
{
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a mine: its owner, its kind and (from the chunk) where it is.
    /// </summary>
    public sealed class MineDeserializer(ILogger<MineDeserializer> logger) : IDeserializer<MapObjectChunk, MineModel>
    {
        public MineModel Deserialize(MapObjectChunk chunk)
        {
            using var subject = logger.BeginSubject("Mine", chunk.Index.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return new(chunk.Index, (MineKind)chunk.GetByteAt(1), chunk.Position!, Bytes.ReadColor(chunk.GetByteAt(0), logger));
        }
    }
}
