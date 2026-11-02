namespace Siniskalc.Heroes.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Entities;

    /// <summary>
    /// The slots of an army, each a stack of creatures or empty. The number of slots is fixed when the army is made (five in HoMM2, seven
    /// in HoMM3) and slots are numbered from 0. Compares by what it holds.
    /// </summary>
    /// <typeparam name="TCreature">The creature of the game.</typeparam>
    public sealed class Army<TCreature> : IArmy<TCreature>, IEquatable<Army<TCreature>>
        where TCreature : class, IGameEntity
    {
        private readonly ArmySlot<TCreature>?[] slots;

        /// <summary>
        /// Initializes a new instance of the <see cref="Army{TCreature}"/> class.
        /// </summary>
        /// <param name="slots">The slots, in order; a <see langword="null"/> one is empty. They are copied.</param>
        public Army(IEnumerable<ArmySlot<TCreature>?> slots)
        {
            this.slots = slots.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Army{TCreature}"/> class with every slot empty.
        /// </summary>
        public Army(int slotCount)
            : this(new ArmySlot<TCreature>?[slotCount])
        {
        }

        /// <inheritdoc/>
        public IReadOnlyList<ArmySlot<TCreature>?> Slots => this.slots;

        /// <inheritdoc/>
        public IEnumerable<ArmySlot<TCreature>> Filled => this.slots.OfType<ArmySlot<TCreature>>();

        /// <inheritdoc/>
        public IArmy<TCreature> WithSlot(int index, ArmySlot<TCreature>? slot)
        {
            RangeGuard.ThrowIfNotIndex(index, this.slots.Length, nameof(index));
            var copy = (ArmySlot<TCreature>?[])this.slots.Clone();
            copy[index] = slot;
            return new Army<TCreature>(copy);
        }

        /// <inheritdoc/>
        public IArmy<TCreature> WithSlot(int index, TCreature creature, long count) => this.WithSlot(index, new ArmySlot<TCreature>(creature, count));

        /// <inheritdoc/>
        public IArmy<TCreature> WithoutSlot(int index) => this.WithSlot(index, null);

        /// <inheritdoc/>
        public IArmy<TCreature> Swap(int index, int otherIndex)
        {
            RangeGuard.ThrowIfNotIndex(index, this.slots.Length, nameof(index));
            RangeGuard.ThrowIfNotIndex(otherIndex, this.slots.Length, nameof(otherIndex));
            var copy = (ArmySlot<TCreature>?[])this.slots.Clone();
            (copy[index], copy[otherIndex]) = (copy[otherIndex], copy[index]);
            return new Army<TCreature>(copy);
        }

        /// <summary>
        /// Returns whether another army has the same stacks in the same slots.
        /// </summary>
        /// <param name="other">The army to compare with.</param>
        /// <returns><see langword="true"/> if every slot holds the same stack or is empty in both.</returns>
        /// <param name="obj">The object to compare with.</param>
        public bool Equals(Army<TCreature>? other) => other is not null && this.slots.AsSpan().SequenceEqual(other.slots);

        /// <summary>
        /// Returns whether another army has the same stacks in the same slots.
        /// </summary>
        /// <param name="other">The army to compare with.</param>
        /// <returns><see langword="true"/> if every slot holds the same stack or is empty in both.</returns>
        /// <param name="obj">The object to compare with.</param>
        public override bool Equals(object? obj) => this.Equals(obj as Army<TCreature>);

        /// <summary>
        /// Returns a hash code made from the stacks in their slots, so equal armies have equal hash codes.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var slot in this.slots)
            {
                hash.Add(slot);
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// Returns the stacks separated by commas, with "empty" for an empty slot.
        /// </summary>
        /// <returns>The text.</returns>
        public override string ToString() => string.Join(", ", this.slots.Select(s => s?.ToString() ?? "empty"));
    }
}
