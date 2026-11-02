namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes the seven slots of a merchant into its list. A slot whose number the editor does not know is left alone while the model still
    /// has what it reads as.
    /// </summary>
    public sealed class SafeMerchantStockSerializer(EntityCatalog catalog) : ISerializer<RecordChunk, MerchantStockModel>
    {
        /// <inheritdoc/>
        public RecordChunk Serialize(RecordChunk chunk, MerchantStockModel model)
        {
            RangeGuard.ThrowIfOutOfRange<long>(model.Slots.Count, MerchantLayout.SlotCount, MerchantLayout.SlotCount, nameof(model.Slots));
            var bytes = chunk.ToArray();
            for (var slot = 0; slot < MerchantLayout.SlotCount; slot++)
            {
                var at = MerchantLayout.SlotAt(slot);
                var current = ByteBufferHelper.ReadUInt32(bytes, at);
                var interpreted = current == MerchantLayout.Empty ? null
                    : current <= byte.MaxValue && catalog.Artifacts.Find((byte)current) is { Wearable: true } known
                        ? known : catalog.FallbackArtifact;
                if (interpreted != model.Slots[slot])
                {
                    ByteBufferHelper.WriteUInt32(bytes, at, model.Slots[slot] is { } artifact ? artifact.Code : MerchantLayout.Empty);
                }
            }

            return chunk.WithBytes(bytes);
        }
    }
}
