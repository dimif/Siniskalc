namespace Siniskalc.Wpf.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// A line of the recent files menu: a file, a note that there are none, a separator or the command that clears the list.
    /// </summary>
    public sealed class RecentMenuItemViewModel
    {
        public RecentMenuItemViewModel(RecentMenuItemKind kind, string header, string? toolTip = null, ICommand? command = null)
        {
            this.Kind = kind;
            this.Header = header;
            this.ToolTip = toolTip;
            this.Command = command;
        }

        public RecentMenuItemKind Kind { get; }

        /// <summary>
        /// Gets the text of the line, with the access key a menu uses in front of it.
        /// </summary>
        public string Header { get; }

        public string? ToolTip { get; }

        public ICommand? Command { get; }

        public bool IsSeparator => this.Kind == RecentMenuItemKind.Separator;

        public bool IsEnabled => this.Kind != RecentMenuItemKind.Empty;
    }
}
