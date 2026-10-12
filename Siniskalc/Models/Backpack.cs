namespace Siniskalc.Models
{
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// Immutable contents of a hero's backpack. Empty slots are <c>null</c>.
    /// </summary>
    public sealed class Backpack
    {
        private readonly BackpackSlot?[] artifacts;

        public Backpack(BackpackSlot?[] artifacts)
        {
            this.artifacts = (BackpackSlot?[])artifacts.Clone();
        }

        public IReadOnlyList<BackpackSlot?> All => this.artifacts;

        /// <summary>
        /// Returns a new backpack with <paramref name="artifact"/> in slot <paramref name="index"/> (or the slot emptied).
        /// <paramref name="spell"/> is the spell of a Spell Scroll.
        /// </summary>
        public Backpack WithSlot(int index, Artifact? artifact, Spell? spell = null)
        {
            var artifacts = (BackpackSlot?[])this.artifacts.Clone();
            artifacts[index] = artifact == null ? null : new BackpackSlot(index, artifact, artifact == Artifact.SpellScroll ? spell : null);
            return new Backpack(artifacts);
        }
    }
}
