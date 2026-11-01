namespace Siniskalc.Wpf.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// A line of the recent files menu: a file, a note that there are none, a separator or the command that clears the list.
    /// </summary>
    public sealed class RecentMenuItemViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecentMenuItemViewModel"/> class.
        /// </summary>
        /// <param name="kind">What the item is: a file, a separator, the command to clear the list, or the note that the list is empty.</param>
        /// <param name="header">The text of the item.</param>
        /// <param name="toolTip">The tooltip, usually the path of the file.</param>
        /// <param name="command">What the item does when picked.</param>
        public RecentMenuItemViewModel(RecentMenuItemKind kind, string header, string? toolTip = null, ICommand? command = null)
        {
            this.Kind = kind;
            this.Header = header;
            this.ToolTip = toolTip;
            this.Command = command;
        }

        /// <summary>
        /// Gets what the item is.
        /// </summary>
        public RecentMenuItemKind Kind { get; }

        /// <summary>
        /// Gets the text of the line, with the access key a menu uses in front of it.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Gets the tooltip, or <see langword="null"/> for none.
        /// </summary>
        public string? ToolTip { get; }

        /// <summary>
        /// Gets what the item does when picked, or <see langword="null"/> for an item that does nothing.
        /// </summary>
        public ICommand? Command { get; }

        /// <summary>
        /// Gets a value indicating whether the item is a separator line.
        /// </summary>
        public bool IsSeparator => this.Kind == RecentMenuItemKind.Separator;

        /// <summary>
        /// Gets a value indicating whether the user can pick the item; the note that the list is empty cannot be picked.
        /// </summary>
        public bool IsEnabled => this.Kind != RecentMenuItemKind.Empty;
    }
}
