namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Lists the slots of a merchant that changed.
    /// </summary>
    public sealed class MerchantStockDiffer : IMerchantStockDiffer
    {
        private const string NoArtifact = "(sold)";

        public IReadOnlyList<PendingChange> Diff(int index, MerchantStockModel original, MerchantStockModel current)
        {
            var changes = new ChangeSink<MerchantStockModel>((change, revert) => ChunkPendingChange.For<MerchantStockChunk, MerchantStockModel>(Homm3ChunkKinds.Merchants, current.Name, index, change, revert));
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
