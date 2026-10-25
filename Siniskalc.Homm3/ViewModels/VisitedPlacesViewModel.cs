namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The places of the permanent bonuses a hero visited, by kind, with the places of the map to forget one by one: the card of the Places tab
    /// of a hero, built from the shared <see cref="VisitedPlacesCardViewModel"/>. Call <see cref="Refresh"/> after the hero changed.
    /// </summary>
    public sealed class VisitedPlacesViewModel
    {
        private readonly Func<HeroModel> getHero;
        private readonly Action<Func<HeroModel, HeroModel>> update;
        private readonly MapPlaces places;

        public VisitedPlacesViewModel(Func<HeroModel> getHero, Action<Func<HeroModel, HeroModel>> update, MapPlaces places, GameVersion gameVersion)
        {
            this.getHero = getHero;
            this.update = update;
            this.places = places;
            this.PlacesVisited = new VisitedPlacesCardViewModel(
                "Visited once per hero",
                VisitedPlaceKind.All.Where(kind => !kind.IsHotaOnly || gameVersion == GameVersion.HornOfTheAbyss).Select(this.VisitedRow).ToList(),
                []);
        }

        public VisitedPlacesCardViewModel PlacesVisited { get; }

        public void Refresh()
        {
            foreach (var row in this.PlacesVisited.Kinds)
            {
                row.Refresh();
            }
        }

        private VisitedPlaceRowViewModel VisitedRow(VisitedPlaceKind kind)
        {
            var found = this.places.Of(kind);
            var toggles = found
                .Select(place => new ToggleRowViewModel(
                    $"At {MapLevelText.Place(place.Position.X, place.Position.Y)}",
                    () => (this.getHero().PlaceVisits.Of(kind) & (1u << place.Number)) != 0,
                    visited => this.update(h => h with { PlaceVisits = h.PlaceVisits.With(kind, visited ? h.PlaceVisits.Of(kind) | (1u << place.Number) : h.PlaceVisits.Of(kind) & ~(1u << place.Number)) }),
                    toolTip: "Tick it when the hero has visited this one: it gives nothing a second time. Not tried in the game."))
                .ToList();
            var allOnMap = found.Aggregate(0u, (mask, place) => mask | (1u << place.Number));
            return new(
                kind.Name,
                () => System.Numerics.BitOperations.PopCount(this.getHero().PlaceVisits.Of(kind)),
                () => this.update(h => h with { PlaceVisits = h.PlaceVisits.With(kind, 0) }),
                () => this.update(h => h with { PlaceVisits = h.PlaceVisits.With(kind, h.PlaceVisits.Of(kind) | allOnMap) }),
                kind.ToolTip,
                toggles);
        }
    }
}
