namespace Siniskalc.Homm3.Models
{
    using System.Collections.Generic;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Immutable contents of a hero's backpack. Empty slots are <c>null</c>.
    /// </summary>
    public sealed class Backpack
    {
        private readonly BackpackSlot?[] artifacts;

        /// <summary>
        /// Initializes a new instance of the <see cref="Backpack"/> class.
        /// </summary>
        /// <param name="artifacts">The artifacts.</param>
        public Backpack(BackpackSlot?[] artifacts)
        {
            this.artifacts = (BackpackSlot?[])artifacts.Clone();
        }

        /// <summary>
        /// Gets the slots of the backpack, <see langword="null"/> for an empty one.
        /// </summary>
        public IReadOnlyList<BackpackSlot?> All => this.artifacts;

        /// <summary>
        /// Returns a new backpack with <paramref name="artifact"/> in slot <paramref name="index"/> (or the slot emptied).
        /// <paramref name="spell"/> is the spell of a Spell Scroll.
        /// </summary>
        /// <param name="index">The slot, from 0.</param>
        /// <param name="artifact">The artifact, or <see langword="null"/> to empty the slot.</param>
        /// <param name="spell">The spell, when the artifact is a scroll.</param>
        /// <returns>The new backpack; this one is not changed.</returns>
        public Backpack WithSlot(int index, Artifact? artifact, Spell? spell = null)
        {
            var artifacts = (BackpackSlot?[])this.artifacts.Clone();
            artifacts[index] = artifact == null ? null : new BackpackSlot(index, artifact, artifact.IsScroll ? spell : null);
            return new Backpack(artifacts);
        }
    }
}
