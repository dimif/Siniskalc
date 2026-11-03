namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes the spell of a shrine into its tile: the number of the spell shifted left by 3, with the 3 low bits of the tile kept. A shrine
    /// whose spell is not known (a hero stands on it) is left alone.
    /// </summary>
    public sealed class SafeShrineSerializer : ISerializer<MapObjectChunk, ShrineModel>
    {
        /// <inheritdoc/>
        public MapObjectChunk Serialize(MapObjectChunk chunk, ShrineModel model)
        {
            var hidden = (chunk.Tag & MapObjectsReader.HiddenShrine) != 0;
            if (hidden || model.SpellCode < 0 || model.SpellCode == chunk.GetUInt16At(0) >> MapObjectLayout.ShrineSpellShift)
            {
                return chunk;
            }

            RangeGuard.ThrowIfOutOfRange<long>(model.SpellCode, 0, byte.MaxValue, nameof(model));
            var bytes = chunk.ToArray();
            ByteBufferHelper.WriteUInt16(bytes, 0, (ushort)((model.SpellCode << MapObjectLayout.ShrineSpellShift) | (chunk.GetUInt16At(0) & ((1 << MapObjectLayout.ShrineSpellShift) - 1))));
            return chunk.WithBytes(bytes);
        }
    }
}
