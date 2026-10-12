namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The fourteen artifact slots of a hero, each an artifact or empty. The slots are a plain list that fills from the first: they are not
    /// tied to a part of the body, and there is no backpack. A hero has a spell book while one of the slots holds the Magic Book.
    /// Compares by what it holds.
    /// </summary>
    public sealed class Homm2Artifacts : IEquatable<Homm2Artifacts>
    {
        private readonly Homm2Artifact?[] slots;

        public Homm2Artifacts(IEnumerable<Homm2Artifact?> slots)
        {
            this.slots = slots.ToArray();
            if (this.slots.Length != Constants.Hero.ArtifactSlotCount)
            {
                throw new ArgumentException($"A hero has {Constants.Hero.ArtifactSlotCount} artifact slots.", nameof(slots));
            }
        }

        public static Homm2Artifacts None { get; } = new(new Homm2Artifact?[Constants.Hero.ArtifactSlotCount]);

        /// <summary>
        /// Gets the slots, <c>null</c> for an empty one.
        /// </summary>
        public IReadOnlyList<Homm2Artifact?> Slots => this.slots;

        public bool HasMagicBook => this.slots.Contains(Homm2Artifact.MagicBook);

        public bool IsFull => this.slots.All(s => s != null);

        public Homm2Artifacts WithSlot(int index, Homm2Artifact? artifact)
        {
            RangeGuard.ThrowIfNotIndex(index, this.slots.Length, nameof(index));
            var copy = (Homm2Artifact?[])this.slots.Clone();
            copy[index] = artifact;
            return new Homm2Artifacts(copy);
        }

        /// <summary>
        /// Returns the artifacts with <paramref name="artifact"/> in the first empty slot.
        /// </summary>
        /// <exception cref="InvalidOperationException">Every slot is taken.</exception>
        public Homm2Artifacts Add(Homm2Artifact artifact)
        {
            var free = Array.IndexOf(this.slots, null);
            return free < 0 ? throw new InvalidOperationException("Every artifact slot is taken.") : this.WithSlot(free, artifact);
        }

        /// <summary>
        /// Returns the artifacts without <paramref name="artifact"/> (the first slot that holds it is emptied).
        /// </summary>
        public Homm2Artifacts Remove(Homm2Artifact artifact)
        {
            var index = Array.IndexOf(this.slots, artifact);
            return index < 0 ? this : this.WithSlot(index, null);
        }

        public bool Equals(Homm2Artifacts? other) => other is not null && this.slots.AsSpan().SequenceEqual(other.slots);

        public override bool Equals(object? obj) => this.Equals(obj as Homm2Artifacts);

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
