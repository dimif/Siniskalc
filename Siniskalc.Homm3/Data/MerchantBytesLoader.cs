namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Finds the lists of the merchants by what they look like (see <see cref="MerchantLayout"/>): fourteen 32 bit numbers in a row, each an
    /// artifact the editor knows or 0xFFFFFFFF, behind a 32 bit 2, in the first bytes of the savefile.
    /// </summary>
    public sealed class MerchantBytesLoader : IMerchantBytesLoader
    {
        private static readonly string[] Names = ["Artifact merchants", "Black market"];

        /// <inheritdoc/>
        public IReadOnlyList<RecordChunk> LoadMerchantChunks(byte[] bytes)
        {
            var last = Math.Min(bytes.Length - (2 * MerchantLayout.ListSize), MerchantLayout.SearchLength);
            for (var at = sizeof(uint); at <= last; at++)
            {
                if (BitConverter.ToUInt32(bytes, at - sizeof(uint)) == MerchantLayout.MarkerBefore && LooksLikeTheLists(bytes, at))
                {
                    return Enumerable.Range(0, Names.Length)
                        .Select(i => new RecordChunk(Names[i], bytes.AsSpan(at + (i * MerchantLayout.ListSize), MerchantLayout.ListSize).ToArray(), at + (i * MerchantLayout.ListSize), i))
                        .ToList();
                }
            }

            return [];
        }

        private static bool LooksLikeTheLists(byte[] bytes, int at)
        {
            var stocked = 0;
            for (var slot = 0; slot < 2 * MerchantLayout.SlotCount; slot++)
            {
                var code = BitConverter.ToUInt32(bytes, at + MerchantLayout.SlotAt(slot));
                if (code == MerchantLayout.Empty)
                {
                    continue;
                }

                if (code > byte.MaxValue || !Artifact.All.ContainsKey((byte)code))
                {
                    return false;
                }

                stocked++;
            }

            return stocked >= MerchantLayout.MinimumStocked;
        }
    }
}
