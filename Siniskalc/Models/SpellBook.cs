namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// Immutable spells of a hero: whether the hero has a spell book, the spells it has learned, and the spells artifacts let
    /// it cast. Learned spells are what can be edited. The spells artifacts grant follow the equipped artifacts (the hero
    /// sets them with <see cref="WithGrants"/>), so they are never edited here. A few spells are available that nothing explains
    /// (a scroll, say); those are kept as they are. Edits return a new instance.
    /// </summary>
    public class SpellBook
    {
        private static readonly IReadOnlyDictionary<Spell, string> NoGrants = new Dictionary<Spell, string>();

        private readonly ISet<Spell> learnedSpells;

        private readonly IReadOnlyDictionary<Spell, string> grantedSpells;

        private readonly ISet<Spell> otherAvailableSpells;

        /// <param name="learnedSpells">The spells in the spell book.</param>
        /// <param name="otherAvailableSpells">Spells that are available although they are not learned and no artifact grants them.</param>
        /// <param name="hasSpellBook">Whether the hero carries a spell book. Without one, learned spells have no effect.</param>
        public SpellBook(ISet<Spell> learnedSpells, ISet<Spell> otherAvailableSpells, bool hasSpellBook = false)
            : this(learnedSpells, NoGrants, false, otherAvailableSpells, hasSpellBook)
        {
        }

        private SpellBook(ISet<Spell> learnedSpells, IReadOnlyDictionary<Spell, string> grantedSpells, bool isSpellBookGranted, ISet<Spell> otherAvailableSpells, bool hasSpellBook)
        {
            this.learnedSpells = learnedSpells;
            this.grantedSpells = grantedSpells;
            this.IsSpellBookGranted = isSpellBookGranted;
            this.otherAvailableSpells = otherAvailableSpells;
            this.HasSpellBook = hasSpellBook;
        }

        /// <summary>
        /// Gets whether the hero carries a spell book. Without one, learned spells have no effect.
        /// </summary>
        public bool HasSpellBook { get; }

        /// <summary>
        /// Gets whether an equipped artifact gives the hero a spell book (Titan's Thunder), so the book cannot be taken away.
        /// </summary>
        public bool IsSpellBookGranted { get; }

        public ISet<Spell> GetAll => this.learnedSpells.Concat(this.grantedSpells.Keys).Concat(this.otherAvailableSpells).ToHashSet();

        public bool IsLearned(Spell spell) => this.learnedSpells.Contains(spell);

        /// <summary>
        /// Gets whether the hero can cast the spell: it is learned, an artifact grants it, or it is available for another reason.
        /// </summary>
        public bool IsAvailable(Spell spell) => this.learnedSpells.Contains(spell) || this.grantedSpells.ContainsKey(spell) || this.otherAvailableSpells.Contains(spell);

        /// <summary>
        /// Gets where the spell comes from when it is not learned: the artifacts that grant it, such as "Sea Captain's Hat", or
        /// "Unknown source". Empty when the spell is learned only, or not available.
        /// </summary>
        public string Source(Spell spell)
        {
            if (this.grantedSpells.TryGetValue(spell, out var artifacts))
            {
                return artifacts;
            }

            return this.otherAvailableSpells.Contains(spell) && !this.learnedSpells.Contains(spell) ? "Unknown source" : string.Empty;
        }

        /// <summary>
        /// Returns a copy in which <paramref name="spell"/> is learned or forgotten.
        /// </summary>
        public SpellBook WithLearned(Spell spell, bool learned)
        {
            return this.WithLearned([spell], learned);
        }

        /// <summary>
        /// Returns a copy in which every one of <paramref name="spells"/> is learned or forgotten.
        /// </summary>
        public SpellBook WithLearned(IEnumerable<Spell> spells, bool learned)
        {
            var updated = new HashSet<Spell>(this.learnedSpells);
            foreach (var spell in spells)
            {
                if (learned)
                {
                    updated.Add(spell);
                }
                else
                {
                    updated.Remove(spell);
                }
            }

            return new SpellBook(updated, this.grantedSpells, this.IsSpellBookGranted, this.otherAvailableSpells, this.HasSpellBook);
        }

        /// <summary>
        /// Returns a copy that has, or has not, a spell book.
        /// </summary>
        public SpellBook WithHasSpellBook(bool hasSpellBook)
        {
            return hasSpellBook == this.HasSpellBook
                ? this
                : new SpellBook(this.learnedSpells, this.grantedSpells, this.IsSpellBookGranted, this.otherAvailableSpells, hasSpellBook);
        }

        /// <summary>
        /// Returns a copy whose spells from artifacts are <paramref name="grantedSpells"/> (spell to the names of the artifacts
        /// that give it), and that knows whether an artifact gives the hero a spell book. Returns this book when nothing changes.
        /// </summary>
        public SpellBook WithGrants(IReadOnlyDictionary<Spell, string> grantedSpells, bool isSpellBookGranted)
        {
            var same = isSpellBookGranted == this.IsSpellBookGranted
                && grantedSpells.Count == this.grantedSpells.Count
                && grantedSpells.All(kv => this.grantedSpells.TryGetValue(kv.Key, out var names) && names == kv.Value);

            return same
                ? this
                : new SpellBook(this.learnedSpells, grantedSpells, isSpellBookGranted, this.otherAvailableSpells, this.HasSpellBook);
        }

        /// <summary>
        /// Returns a copy whose "other" available spells are exactly <paramref name="spells"/>.
        /// </summary>
        public SpellBook WithOtherAvailable(IEnumerable<Spell> spells)
        {
            return new SpellBook(this.learnedSpells, this.grantedSpells, this.IsSpellBookGranted, new HashSet<Spell>(spells), this.HasSpellBook);
        }
    }
}
