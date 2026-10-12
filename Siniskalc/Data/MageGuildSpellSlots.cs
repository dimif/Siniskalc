namespace Siniskalc.Data
{
    using System;

    /// <summary>
    /// Where the spells of the Mage Guild are in a <see cref="CastleChunk"/>, and how many of them a level has. Shared by the reader
    /// and the writer of a castle, which agree on the slots without using each other.
    /// </summary>
    public static class MageGuildSpellSlots
    {
        /// <summary>
        /// Returns the number of spells to read on a spell level (1 to 5): the count the chunk stores, but at least the five, four,
        /// three, two or one that a guild gets when it is built (the count is 0 until then), and at most
        /// <see cref="CastleLayout.SpellsPerLevelMaximum"/>.
        /// </summary>
        public static int Count(CastleChunk chunk, int level)
        {
            var stored = chunk.GetByteAt(chunk.NameEnd + CastleLayout.SpellCountsFromNameEnd + level - 1);
            return Math.Min(Math.Max(stored, CastleLayout.SpellLevelCount - level + 1), CastleLayout.SpellsPerLevelMaximum);
        }

        /// <summary>
        /// Returns the offset in the chunk of a slot of a spell level (1 to 5), or -1 if the chunk ends before it.
        /// </summary>
        public static int Offset(CastleChunk chunk, int level, int slot)
        {
            var offset = chunk.NameEnd + CastleLayout.SpellListsFromNameEnd + ((level - 1) * CastleLayout.SpellListStride) + (slot * 4);
            return offset + 4 <= chunk.Length ? offset : -1;
        }
    }
}
