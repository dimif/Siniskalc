namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes the owner and the creatures that can be hired of a dwelling into its record. The creature and the tile are not edited. An owner
    /// byte that is no colour is left alone while the model says Neutral, as it reads.
    /// </summary>
    public sealed class SafeDwellingSerializer : ISerializer<RecordChunk, DwellingModel>
    {
        /// <inheritdoc/>
        public RecordChunk Serialize(RecordChunk chunk, DwellingModel model)
        {
            var bytes = chunk.ToArray();
            if (PlayerColor.FromByte(bytes[DwellingLayout.Owner]) != model.Owner)
            {
                bytes[DwellingLayout.Owner] = (byte)model.Owner.Code;
            }

            if (ByteBufferHelper.ReadUInt32(bytes, DwellingLayout.Stock) != model.Stock)
            {
                RangeGuard.ThrowIfOutOfRange(model.Stock, 0, Constants.Army.MaxStackSize, nameof(model.Stock));
                ByteBufferHelper.WriteUInt32(bytes, DwellingLayout.Stock, (uint)model.Stock);
            }

            return chunk.WithBytes(bytes);
        }
    }
}
