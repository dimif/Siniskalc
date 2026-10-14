namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The spells a hero has learned, one flag for each of the 65 spells in the savegame. The hero can cast them only while it has the
    /// Magic Book (see <see cref="Artifacts.HasMagicBook"/>). Compares by what it holds.
    /// </summary>
    public sealed class SpellBook : IEquatable<SpellBook>
    {
        private readonly SortedSet<byte> codes;

        public SpellBook(IEnumerable<Spell> spells)
        {
            this.codes = new SortedSet<byte>(spells.Select(s => s.Code));
        }

        public static SpellBook None { get; } = new([]);

        /// <summary>
        /// Gets the spells, in the order of their numbers.
        /// </summary>
        public IEnumerable<Spell> Spells => this.codes.Select(c => Spell.FromByte(c)!);

        public int Count => this.codes.Count;

        public bool Contains(Spell spell) => this.codes.Contains(spell.Code);

        public SpellBook With(Spell spell) => this.Contains(spell) ? this : new SpellBook(this.Spells.Append(spell));

        public SpellBook Without(Spell spell) => this.Contains(spell) ? new SpellBook(this.Spells.Where(s => s != spell)) : this;

        public bool Equals(SpellBook? other) => other is not null && this.codes.SetEquals(other.codes);

        public override bool Equals(object? obj) => this.Equals(obj as SpellBook);

        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var code in this.codes)
            {
                hash.Add(code);
            }

            return hash.ToHashCode();
        }

        public override string ToString() => string.Join(", ", this.Spells.Select(s => s.Name));
    }
}
