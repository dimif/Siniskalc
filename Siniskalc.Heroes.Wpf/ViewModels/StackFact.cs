namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// One line of what a stack of creatures is worth, such as "Stack HP" and "1,200".
    /// </summary>
    public sealed record StackFact(string Label, string Value, string ToolTip = "");
}
