namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// A small mark in front of the name of a node of the navigation tree, such as the crown of a capital.
    /// </summary>
    /// <param name="Key">What the mark tells, for the game to find it again (for instance "capital").</param>
    /// <param name="Glyph">The character shown.</param>
    /// <param name="Tooltip">What the mark means.</param>
    /// <param name="Tone">How strongly it shows.</param>
    public sealed record NavBadge(string Key, string Glyph, string Tooltip, NavBadgeTone Tone = NavBadgeTone.Muted);
}
