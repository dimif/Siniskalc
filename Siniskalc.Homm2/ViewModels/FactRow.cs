namespace Siniskalc.Homm2.ViewModels
{
    /// <summary>
    /// One line of a card: what it is, its value, and a note that is shown smaller and muted after the value.
    /// </summary>
    public sealed record FactRow(string Label, string Value, string Note = "");
}
