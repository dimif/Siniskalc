namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The spells of a Mage Guild, a place for each: four for each of its five levels, the first level first, <c>null</c> for a place with
    /// none. It compares by what it holds, so that castles can be compared.
    /// </summary>
    public sealed class MageGuildSlots : IReadOnlyList<Spell?>, IEquatable<MageGuildSlots>
    {
        private readonly Spell?[] spells;

        /// <summary>
        /// Initializes a new instance of the <see cref="MageGuildSlots"/> class.
        /// </summary>
        /// <param name="spells">The spells.</param>
        public MageGuildSlots(IEnumerable<Spell?> spells)
        {
            this.spells = spells.ToArray();
        }

        /// <summary>
        /// Gets the number of spells the guild lists in all.
        /// </summary>
        public int Count => this.spells.Length;

        /// <summary>
        /// Gets the spell in a slot, or <see langword="null"/> for none.
        /// </summary>
        public Spell? this[int index] => this.spells[index];

        /// <summary>
        /// Returns the slots with the spell at <paramref name="index"/> replaced.
        /// </summary>
        /// <param name="index">The slot.</param>
        /// <param name="spell">The spell, or <see langword="null"/> for none.</param>
        /// <returns>The new slots; these are not changed.</returns>
        public MageGuildSlots With(int index, Spell? spell)
        {
            var copy = (Spell?[])this.spells.Clone();
            copy[index] = spell;
            return new MageGuildSlots(copy);
        }

        /// <inheritdoc/>
        public bool Equals(MageGuildSlots? other) => other is not null && this.spells.AsSpan().SequenceEqual(other.spells);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => this.Equals(obj as MageGuildSlots);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var spell in this.spells)
            {
                hash.Add(spell);
            }

            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public IEnumerator<Spell?> GetEnumerator() => ((IEnumerable<Spell?>)this.spells).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
