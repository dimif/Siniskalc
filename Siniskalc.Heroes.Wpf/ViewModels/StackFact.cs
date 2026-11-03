namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// One line of what a stack of creatures is worth, such as "Stack HP" and "1,200". A note has no label and is shown smaller, under the lines.
    /// </summary>
    public sealed record StackFact(string Label, string Value, string ToolTip = "", bool IsNote = false);
}
