namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes whose turn it is: the number of the record of the player.
    /// </summary>
    public sealed class SafeTurnSerializer : ISerializer<MapObjectChunk, TurnModel>
    {
        /// <inheritdoc/>
        public MapObjectChunk Serialize(MapObjectChunk chunk, TurnModel model)
        {
            if (chunk.GetByteAt(0) == model.Record)
            {
                return chunk;
            }

            RangeGuard.ThrowIfOutOfRange(model.Record, 0, Constants.Table.PlayerCount - 1, nameof(model));
            var bytes = chunk.ToArray();
            bytes[0] = (byte)model.Record;
            return chunk.WithBytes(bytes);
        }
    }
}
