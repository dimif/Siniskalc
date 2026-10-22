namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes which castles have built today: sets or clears the bit of each castle and leaves the other bits as they are.
    /// </summary>
    public sealed class SafeBuiltTodaySerializer : ISerializer<MapObjectChunk, BuiltTodayModel>
    {
        /// <inheritdoc/>
        public MapObjectChunk Serialize(MapObjectChunk chunk, BuiltTodayModel model)
        {
            var bytes = chunk.ToArray();
            for (var castle = 0; castle < Constants.Table.CastleSlotCount; castle++)
            {
                var mask = (byte)(1 << (castle % 8));
                bytes[castle / 8] = model.Has(castle) ? (byte)(bytes[castle / 8] | mask) : (byte)(bytes[castle / 8] & ~mask);
            }

            return bytes.SequenceEqual(chunk.ToArray()) ? chunk : chunk.WithBytes(bytes);
        }
    }
}
