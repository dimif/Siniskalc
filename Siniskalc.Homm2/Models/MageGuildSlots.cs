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

        public MageGuildSlots(IEnumerable<Spell?> spells)
        {
            this.spells = spells.ToArray();
        }

        public int Count => this.spells.Length;

        public Spell? this[int index] => this.spells[index];

        /// <summary>
        /// Returns the slots with the spell at <paramref name="index"/> replaced.
        /// </summary>
        public MageGuildSlots With(int index, Spell? spell)
        {
            var copy = (Spell?[])this.spells.Clone();
            copy[index] = spell;
            return new MageGuildSlots(copy);
        }

        public bool Equals(MageGuildSlots? other) => other is not null && this.spells.AsSpan().SequenceEqual(other.spells);

        public override bool Equals(object? obj) => this.Equals(obj as MageGuildSlots);

        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var spell in this.spells)
            {
                hash.Add(spell);
            }

            return hash.ToHashCode();
        }

        public IEnumerator<Spell?> GetEnumerator() => ((IEnumerable<Spell?>)this.spells).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
