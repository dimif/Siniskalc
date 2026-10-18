namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of the places a hero remembers having visited, in any game: a row for each kind of place, which opens to the places of that kind with
    /// where each is (just the coordinates, as the row says what it is), and boxes for what the hero used once.
    /// </summary>
    /// <param name="Title">The heading of the card, such as "Visited once per hero".</param>
    /// <param name="Kinds">The kinds of place (forts, gazebos...), each with its places.</param>
    /// <param name="Others">What the hero used once or this week and is not a place of the map, such as the arena.</param>
    public sealed record VisitedPlacesCardViewModel(string Title, IReadOnlyList<VisitedPlaceRowViewModel> Kinds, IReadOnlyList<ToggleRowViewModel> Others);
}
