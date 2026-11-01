namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Makes the row of a kind of place a hero remembers having visited. The game says which bit of the kind's number each place of the
    /// map has and how the number is read and written; the row only knows the bits.
    /// </summary>
    public static class VisitedPlaceRows
    {
        /// <summary>
        /// Makes the row of a kind of place.
        /// </summary>
        /// <param name="name">The name of the kind.</param>
        /// <param name="toolTip">What visiting a place of the kind gives.</param>
        /// <param name="places">The places of the map: the bit each has in the number of the kind (the first place is usually 1) and where it is.</param>
        /// <param name="getMask">Gets the number of the kind, a bit for each place the hero visited.</param>
        /// <param name="editMask">Changes the number of the kind, given what it is when the change is applied.</param>
        public static VisitedPlaceRowViewModel Create(
            string name,
            string toolTip,
            IEnumerable<(int Number, MapPosition Position)> places,
            Func<uint> getMask,
            Action<Func<uint, uint>> editMask)
        {
            var found = places.ToList();
            var toggles = found
                .Select(place => new ToggleRowViewModel(
                    $"At {MapLevelText.Place(place.Position.X, place.Position.Y)}",
                    () => (getMask() & (1u << place.Number)) != 0,
                    visited => editMask(mask => visited ? mask | (1u << place.Number) : mask & ~(1u << place.Number)),
                    toolTip: "Tick it when the hero has visited this one: it gives nothing a second time. Not tried in the game."))
                .ToList();
            var allOnMap = found.Aggregate(0u, (mask, place) => mask | (1u << place.Number));
            return new VisitedPlaceRowViewModel(
                name,
                () => System.Numerics.BitOperations.PopCount(getMask()),
                () => editMask(_ => 0),
                () => editMask(mask => mask | allOnMap),
                toolTip,
                toggles);
        }
    }
}
