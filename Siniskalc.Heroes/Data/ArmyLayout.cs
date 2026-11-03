namespace Siniskalc.Heroes.Data
{
    /// <summary>
    /// Where the stacks of an army are in a record of a savefile: a table of creature types and a table of counts, one entry for each
    /// slot. A game says how wide an entry is; an empty slot has every bit of its type set.
    /// </summary>
    /// <param name="TypeOffset">Where the table of creature types starts.</param>
    /// <param name="TypeSize">How many bytes the type of a slot takes: 1, 2 or 4.</param>
    /// <param name="CountOffset">Where the table of counts starts.</param>
    /// <param name="CountSize">How many bytes the count of a slot takes: 1, 2 or 4.</param>
    /// <param name="SlotCount">How many slots the army has.</param>
    /// <param name="MaxCount">The most creatures a stack can be written with.</param>
    public sealed record ArmyLayout(int TypeOffset, int TypeSize, int CountOffset, int CountSize, int SlotCount, long MaxCount)
    {
        /// <summary>
        /// Gets the type that marks an empty slot: every bit set.
        /// </summary>
        public uint EmptyType => this.TypeSize >= 4 ? uint.MaxValue : (1u << (8 * this.TypeSize)) - 1;

        /// <summary>
        /// Gets where the type of the slot at <paramref name="slot"/> is.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        /// <returns>The offset in the chunk of the creature type of the slot.</returns>
        public int TypeAt(int slot) => this.TypeOffset + (slot * this.TypeSize);

        /// <summary>
        /// Gets where the count of the slot at <paramref name="slot"/> is.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        /// <returns>The offset in the chunk of the creature count of the slot.</returns>
        public int CountAt(int slot) => this.CountOffset + (slot * this.CountSize);
    }
}
