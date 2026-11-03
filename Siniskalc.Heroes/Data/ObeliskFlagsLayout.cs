namespace Siniskalc.Heroes.Data
{
    /// <summary>
    /// Where the visited flags of the obelisks are in their chunk, and optionally where the chunk stores how many flags it has.
    /// </summary>
    /// <param name="FlagsOffset">The offset of the first visited-flags byte.</param>
    /// <param name="CountOffset">The offset of the stored count, or <see langword="null"/> when the number of positions is the count.</param>
    public sealed record ObeliskFlagsLayout(int FlagsOffset, int? CountOffset);
}
