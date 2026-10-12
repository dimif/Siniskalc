namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;

    /// <summary>
    /// The five slots of an army, each a stack of creatures or empty. Compares by what it holds.
    /// </summary>
    public sealed class Homm2Army : IEquatable<Homm2Army>
    {
        private readonly Homm2ArmySlot?[] slots;

        public Homm2Army(IEnumerable<Homm2ArmySlot?> slots)
        {
            this.slots = slots.ToArray();
            if (this.slots.Length != Constants.ArmySlotCount)
            {
                throw new ArgumentException($"An army has {Constants.ArmySlotCount} slots.", nameof(slots));
            }
        }

        public static Homm2Army Empty { get; } = new(new Homm2ArmySlot?[Constants.ArmySlotCount]);

        /// <summary>
        /// Gets the slots, <c>null</c> for an empty one.
        /// </summary>
        public IReadOnlyList<Homm2ArmySlot?> Slots => this.slots;

        /// <summary>
        /// Returns the army with the slot at <paramref name="index"/> (0 to 4) replaced, by <c>null</c> to empty it.
        /// </summary>
        public Homm2Army WithSlot(int index, Homm2ArmySlot? slot)
        {
            RangeGuard.ThrowIfNotIndex(index, this.slots.Length, nameof(index));
            var copy = (Homm2ArmySlot?[])this.slots.Clone();
            copy[index] = slot;
            return new Homm2Army(copy);
        }

        public bool Equals(Homm2Army? other) => other is not null && this.slots.AsSpan().SequenceEqual(other.slots);

        public override bool Equals(object? obj) => this.Equals(obj as Homm2Army);

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
