namespace Siniskalc.Heroes.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;

    /// <summary>
    /// An immutable set of spells, kept in the order of their codes. Compares by what it holds.
    /// </summary>
    /// <typeparam name="TSpell">The spell of the game.</typeparam>
    public sealed class SpellSet<TSpell> : IReadOnlyCollection<TSpell>, IEquatable<SpellSet<TSpell>>
        where TSpell : class, IGameEntity
    {
        private readonly SortedDictionary<byte, TSpell> spells;

        public SpellSet(IEnumerable<TSpell> spells)
        {
            this.spells = new SortedDictionary<byte, TSpell>();
            foreach (var spell in spells)
            {
                this.spells[spell.Code] = spell;
            }
        }

        /// <summary>
        /// Gets the set with no spell.
        /// </summary>
        public static SpellSet<TSpell> None { get; } = new([]);

        public int Count => this.spells.Count;

        public bool Contains(TSpell spell) => this.spells.ContainsKey(spell.Code);

        /// <summary>
        /// Returns the set with the spell in it.
        /// </summary>
        public SpellSet<TSpell> With(TSpell spell) => this.Contains(spell) ? this : new SpellSet<TSpell>(this.spells.Values.Append(spell));

        /// <summary>
        /// Returns the set without the spell.
        /// </summary>
        public SpellSet<TSpell> Without(TSpell spell) => this.Contains(spell) ? new SpellSet<TSpell>(this.spells.Values.Where(s => s.Code != spell.Code)) : this;

        public IEnumerator<TSpell> GetEnumerator() => this.spells.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Equals(SpellSet<TSpell>? other) => other is not null && this.spells.Keys.SequenceEqual(other.spells.Keys);

        public override bool Equals(object? obj) => this.Equals(obj as SpellSet<TSpell>);

        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var code in this.spells.Keys)
            {
                hash.Add(code);
            }

            return hash.ToHashCode();
        }

        public override string ToString() => string.Join(", ", this.spells.Values.Select(s => s.Name));
    }
}
