namespace Siniskalc.Wpf.Views
{
    using System.Collections.Generic;

    /// <summary>
    /// One line of the grid of the hex viewer: an address, sixteen bytes and their text.
    /// Lines in the collection are transient (see HexRowList), and two lines with the same position are equal.
    /// </summary>
    internal sealed class HexRow(int index, string address, IReadOnlyList<HexCell> cells, string ascii)
    {
        /// <summary>
        /// Gets the position of the line in the grid.
        /// </summary>
        public int Index { get; } = index;

        /// <summary>
        /// Gets the offset of the first byte of the line, in hex and in decimal.
        /// </summary>
        public string Address { get; } = address;

        /// <summary>
        /// Gets the sixteen bytes.
        /// </summary>
        public IReadOnlyList<HexCell> Cells { get; } = cells;

        /// <summary>
        /// Gets the bytes as text, with a dot for what cannot be shown.
        /// </summary>
        public string Ascii { get; } = ascii;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is HexRow other && other.Index == this.Index;

        /// <inheritdoc/>
        public override int GetHashCode() => this.Index;
    }
}
