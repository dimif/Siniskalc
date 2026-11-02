namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Lists the slots of a merchant that changed.
    /// </summary>
    public sealed class MerchantStockDiffer : IDiffer<MerchantStockModel>
    {
        private const string NoArtifact = "(sold)";

        /// <summary>
        /// Lists what an edit changed in the stock of a merchant.
        /// </summary>
        /// <param name="index">The place in its list, from 0.</param>
        /// <param name="original">The model as it was read from the savefile.</param>
        /// <param name="current">The model after the edit.</param>
        /// <returns>The changes, none when the two are the same.</returns>
        public IReadOnlyList<PendingChange> Diff(int index, MerchantStockModel original, MerchantStockModel current)
        {
            var changes = new ChangeSink<MerchantStockModel>((change, revert) => ChunkPendingChange.For<RecordChunk, MerchantStockModel>(Homm3ChunkKinds.Merchants, current.Name, index, change, revert));
            for (var slot = 0; slot < current.Slots.Count; slot++)
            {
                var number = slot;
                if (original.Slots[slot] != current.Slots[slot])
                {
                    changes.Add(ChangeAreas.General, $"Slot {slot + 1}", original.Slots[slot]?.Name ?? NoArtifact, current.Slots[slot]?.Name ?? NoArtifact, stock => stock.WithSlot(number, original.Slots[number]));
                }
            }

            return changes.All;
        }
    }
}
