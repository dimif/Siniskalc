namespace Siniskalc.ViewModels
{
    /// <summary>
    /// A level of the map to pick from: "Surface (0)", "Underground (1)", or a value this editor does not recognise.
    /// </summary>
    /// <param name="Value">The Z coordinate as the savefile holds it.</param>
    /// <param name="Label">What the list shows.</param>
    public sealed record MapLevelChoice(short Value, string Label);
}
