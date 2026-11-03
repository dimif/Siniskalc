namespace Siniskalc.Wpf.ViewModels
{
    using System.Windows;

    /// <summary>
    /// A column of a finder: its heading, the property of a row it shows and how.
    /// </summary>
    /// <param name="Header">The column heading.</param>
    /// <param name="Path">The property of a row it shows.</param>
    /// <param name="Width">The column width; a star width shares what is left.</param>
    /// <param name="Kind">How the property is shown.</param>
    /// <param name="ToolTip">Explains the column when the pointer is over its heading.</param>
    public sealed record FinderColumn(string Header, string Path, GridLength Width, FinderColumnKind Kind = FinderColumnKind.Text, string? ToolTip = null);
}
