namespace Siniskalc.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// A button that sits among the tiles of the obelisks, such as "Visit all".
    /// </summary>
    public sealed record ObeliskActionViewModel(string Label, string ToolTip, ICommand Command);
}
