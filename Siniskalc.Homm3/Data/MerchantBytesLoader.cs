namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Finds the lists of the merchants by what they look like (see <see cref="MerchantLayout"/>): a 32 bit number of merchants and, behind it, that
    /// many lists of seven 32 bit numbers, each an artifact the editor knows or 0xFFFFFFFF, in the first bytes of the savefile.
    /// </summary>
    public sealed class MerchantBytesLoader(EntityCatalog catalog) : IMerchantBytesLoader
    {
        /// <inheritdoc/>
        public IReadOnlyList<RecordChunk> LoadMerchantChunks(byte[] bytes)
        {
            var last = Math.Min(bytes.Length - MerchantLayout.ListSize, MerchantLayout.SearchLength);
            for (var at = sizeof(uint); at <= last; at++)
            {
                var count = BitConverter.ToUInt32(bytes, at - sizeof(uint));
                if (count is >= 1 and <= MerchantLayout.MaxMerchants && at + (count * MerchantLayout.ListSize) <= bytes.Length && this.LooksLikeTheLists(bytes, at, (int)count))
                {
                    return Enumerable.Range(0, (int)count)
                        .Select(i => new RecordChunk(NameOf(i, (int)count), bytes.AsSpan(at + (i * MerchantLayout.ListSize), MerchantLayout.ListSize).ToArray(), at + (i * MerchantLayout.ListSize), i))
                        .ToList();
                }
            }

            return [];
        }

        // The game calls every merchant a black market.
        private string NameOf(int index, int count) => count == 1 ? catalog.MapMetadata.MerchantName : $"{catalog.MapMetadata.MerchantName} {index + 1}";

        private bool LooksLikeTheLists(byte[] bytes, int at, int count)
        {
            var stocked = 0;
            for (var slot = 0; slot < count * MerchantLayout.SlotCount; slot++)
            {
                var code = BitConverter.ToUInt32(bytes, at + MerchantLayout.SlotAt(slot));
                if (code == MerchantLayout.Empty)
                {
                    continue;
                }

                if (code > byte.MaxValue || catalog.Artifacts.Find((byte)code) is not { Wearable: true })
                {
                    return false;
                }

                stocked++;
            }

            return stocked >= MerchantLayout.MinimumStocked;
        }
    }
}
