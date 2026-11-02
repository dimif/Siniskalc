namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of the places a hero remembers having visited, in any game: a row for each kind of place, which opens to the places of that kind with
    /// where each is (just the coordinates, as the row says what it is), and boxes for what the hero used once. Two commands at the top
    /// make the hero forget everything or remember everything.
    /// </summary>
    /// <param name="Title">The heading of the card, such as "Visited once per hero".</param>
    /// <param name="Kinds">The kinds of place (forts, gazebos...), each with its places.</param>
    /// <param name="Others">What the hero used once or this week and is not a place of the map, such as the arena.</param>
    public sealed record VisitedPlacesCardViewModel(string Title, IReadOnlyList<VisitedPlaceRowViewModel> Kinds, IReadOnlyList<ToggleRowViewModel> Others)
    {
        /// <summary>
        /// Gets the command that makes the hero forget every place of every kind and unticks the boxes ("All none").
        /// </summary>
        public ICommand ForgetAllCommand { get; } = new DelegateCommand(_ =>
        {
            foreach (var kind in Kinds.Where(k => k.ForgetCommand.CanExecute(null)))
            {
                kind.ForgetCommand.Execute(null);
            }

            foreach (var other in Others.Where(o => o.IsChecked))
            {
                other.IsChecked = false;
            }
        });

        /// <summary>
        /// Gets the command that makes the hero remember every place of the map and ticks the boxes ("All visited").
        /// </summary>
        public ICommand RememberAllCommand { get; } = new DelegateCommand(_ =>
        {
            foreach (var kind in Kinds.Where(k => k.RememberCommand.CanExecute(null)))
            {
                kind.RememberCommand.Execute(null);
            }

            foreach (var other in Others.Where(o => !o.IsChecked))
            {
                other.IsChecked = true;
            }
        });
    }
}
