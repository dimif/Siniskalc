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

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellSet{TSpell}"/> class.
        /// </summary>
        /// <param name="spells">The spells; one with the same code as an earlier one is left out.</param>
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

        /// <summary>
        /// Gets the number of spells in the set.
        /// </summary>
        public int Count => this.spells.Count;

        /// <summary>
        /// Returns whether the set holds a spell.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <returns><see langword="true"/> if a spell with the same code is in the set.</returns>
        public bool Contains(TSpell spell) => this.spells.ContainsKey(spell.Code);

        /// <summary>
        /// Returns the set with the spell in it.
        /// </summary>
        /// <param name="spell">The spell to add.</param>
        /// <returns>The set; a new one unless the spell is already in it.</returns>
        public SpellSet<TSpell> With(TSpell spell) => this.Contains(spell) ? this : new SpellSet<TSpell>(this.spells.Values.Append(spell));

        /// <summary>
        /// Returns the set without the spell.
        /// </summary>
        /// <param name="spell">The spell to take out.</param>
        /// <returns>The set; a new one unless the spell is not in it.</returns>
        public SpellSet<TSpell> Without(TSpell spell) => this.Contains(spell) ? new SpellSet<TSpell>(this.spells.Values.Where(s => s.Code != spell.Code)) : this;

        /// <summary>
        /// Returns an enumerator over the spells.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<TSpell> GetEnumerator() => this.spells.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns whether another set holds the same spells.
        /// </summary>
        /// <param name="other">The set to compare with.</param>
        /// <returns><see langword="true"/> if both hold the same spells.</returns>
        /// <param name="obj">The object to compare with.</param>
        public bool Equals(SpellSet<TSpell>? other) => other is not null && this.spells.Keys.SequenceEqual(other.spells.Keys);

        /// <summary>
        /// Returns whether another set holds the same spells.
        /// </summary>
        /// <param name="other">The set to compare with.</param>
        /// <returns><see langword="true"/> if both hold the same spells.</returns>
        /// <param name="obj">The object to compare with.</param>
        public override bool Equals(object? obj) => this.Equals(obj as SpellSet<TSpell>);

        /// <summary>
        /// Returns a hash code made from the codes of the spells, so equal sets have equal hash codes.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var code in this.spells.Keys)
            {
                hash.Add(code);
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// Returns the names of the spells separated by commas.
        /// </summary>
        /// <returns>The text.</returns>
        public override string ToString() => string.Join(", ", this.spells.Values.Select(s => s.Name));
    }
}
