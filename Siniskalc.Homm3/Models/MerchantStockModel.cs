namespace Siniskalc.Homm3.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// What a merchant has in store: seven slots, each an artifact or empty (what was bought).
    /// </summary>
    /// <param name="Index">The place of the merchant among the merchants of the savegame: 0 the artifact merchants, 1 the black market.</param>
    /// <param name="Name">What the merchant is called.</param>
    /// <param name="Slots">The seven slots, in the order of the shop; <c>null</c> for an empty one.</param>
    public sealed record MerchantStockModel(int Index, string Name, IReadOnlyList<Artifact?> Slots)
    {
        /// <summary>
        /// Returns the stock with another artifact (or none) in a slot.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        /// <param name="artifact">The artifact, or <see langword="null"/> for none.</param>
        /// <returns>The new stock; this one is not changed.</returns>
        public MerchantStockModel WithSlot(int slot, Artifact? artifact)
        {
            var slots = this.Slots.ToList();
            slots[slot] = artifact;
            return this with { Slots = slots };
        }

        /// <inheritdoc/>
        public bool Equals(MerchantStockModel? other) => other is not null && this.Index == other.Index && this.Name == other.Name && this.Slots.SequenceEqual(other.Slots);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(this.Index, this.Name, this.Slots.Count);
    }
}
