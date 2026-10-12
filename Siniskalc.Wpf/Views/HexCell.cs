namespace Siniskalc.Wpf.Views
{
    using System.Windows.Media;

    /// <summary>
    /// One byte of a <see cref="HexRow"/>, as the grid of the hex viewer shows it.
    /// </summary>
    /// <param name="Text">The byte in hex, or nothing past the end of the savefile.</param>
    /// <param name="Background">The colour of the region the byte is in, or null when it is not known.</param>
    /// <param name="ToolTip">Which fields the byte is in, and what changed, or null when there is nothing to say.</param>
    /// <param name="IsChanged">Whether the byte differs from the savefile as loaded.</param>
    internal sealed record HexCell(string Text, Brush? Background, string? ToolTip, bool IsChanged);
}
