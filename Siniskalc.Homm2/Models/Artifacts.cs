namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The fourteen artifact slots of a hero, each an artifact or empty. The slots are a plain list that fills from the first: they are not
    /// tied to a part of the body, and there is no backpack. A hero has a spell book while one of the slots holds the Magic Book.
    /// Compares by what it holds.
    /// </summary>
    public sealed class Artifacts : IEquatable<Artifacts>
    {
        private readonly Artifact?[] slots;

        public Artifacts(IEnumerable<Artifact?> slots)
        {
            this.slots = slots.ToArray();
            if (this.slots.Length != Constants.Hero.ArtifactSlotCount)
            {
                throw new ArgumentException($"A hero has {Constants.Hero.ArtifactSlotCount} artifact slots.", nameof(slots));
            }
        }

        public static Artifacts None { get; } = new(new Artifact?[Constants.Hero.ArtifactSlotCount]);

        /// <summary>
        /// Gets the slots, <c>null</c> for an empty one.
        /// </summary>
        public IReadOnlyList<Artifact?> Slots => Array.AsReadOnly(this.slots);

        /// <summary>
        /// Gets what the artifacts together add to the primary skills of the hero.
        /// </summary>
        public PrimarySkillBonus TotalBonus => this.slots.Where(s => s != null).Aggregate(PrimarySkillBonus.None, (sum, a) => sum + a!.PrimarySkillBonus);

        public bool HasMagicBook => this.slots.Any(a => a?.IsSpellBook == true);

        public bool IsFull => this.slots.All(s => s != null);

        public Artifacts WithSlot(int index, Artifact? artifact)
        {
            RangeGuard.ThrowIfNotIndex(index, this.slots.Length, nameof(index));
            var copy = (Artifact?[])this.slots.Clone();
            copy[index] = artifact;
            return new Artifacts(copy);
        }

        /// <summary>
        /// Returns the artifacts with <paramref name="artifact"/> in the first empty slot.
        /// </summary>
        /// <exception cref="GameRuleException">Every slot is taken.</exception>
        public Artifacts Add(Artifact artifact)
        {
            var free = Array.IndexOf(this.slots, null);
            return free < 0 ? throw new GameRuleException("Every artifact slot is taken.") : this.WithSlot(free, artifact);
        }

        /// <summary>
        /// Returns the artifacts without <paramref name="artifact"/> (the first slot that holds it is emptied).
        /// </summary>
        public Artifacts Remove(Artifact artifact)
        {
            var index = Array.IndexOf(this.slots, artifact);
            return index < 0 ? this : this.WithSlot(index, null);
        }

        public bool Equals(Artifacts? other) => other is not null && this.slots.AsSpan().SequenceEqual(other.slots);

        public override bool Equals(object? obj) => this.Equals(obj as Artifacts);

        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var slot in this.slots)
            {
                hash.Add(slot);
            }

            return hash.ToHashCode();
        }

        public override string ToString() => string.Join(", ", this.slots.Where(s => s != null).Select(s => s!.Name));
    }
}
