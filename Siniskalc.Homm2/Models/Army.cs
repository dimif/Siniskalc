namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;

    /// <summary>
    /// The five slots of an army, each a stack of creatures or empty. Compares by what it holds.
    /// </summary>
    public sealed class Army : IEquatable<Army>
    {
        private readonly ArmySlot?[] slots;

        public Army(IEnumerable<ArmySlot?> slots)
        {
            this.slots = slots.ToArray();
            if (this.slots.Length != Constants.ArmySlotCount)
            {
                throw new ArgumentException($"An army has {Constants.ArmySlotCount} slots.", nameof(slots));
            }
        }

        public static Army Empty { get; } = new(new ArmySlot?[Constants.ArmySlotCount]);

        /// <summary>
        /// Gets the slots, <c>null</c> for an empty one.
        /// </summary>
        public IReadOnlyList<ArmySlot?> Slots => this.slots;

        /// <summary>
        /// Returns the army with the slot at <paramref name="index"/> (0 to 4) replaced, by <c>null</c> to empty it.
        /// </summary>
        public Army WithSlot(int index, ArmySlot? slot)
        {
            RangeGuard.ThrowIfNotIndex(index, this.slots.Length, nameof(index));
            var copy = (ArmySlot?[])this.slots.Clone();
            copy[index] = slot;
            return new Army(copy);
        }

        public bool Equals(Army? other) => other is not null && this.slots.AsSpan().SequenceEqual(other.slots);

        public override bool Equals(object? obj) => this.Equals(obj as Army);

        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var slot in this.slots)
            {
                hash.Add(slot);
            }

            return hash.ToHashCode();
        }

        public override string ToString() => string.Join(", ", this.slots.Select(s => s?.ToString() ?? "empty"));
    }
}
