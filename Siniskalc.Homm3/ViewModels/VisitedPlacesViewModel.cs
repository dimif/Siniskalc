namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

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
                getHero().HeroClass.Catalog.VisitedPlaces.All.Where(kind => kind.IsAvailableIn(gameVersion)).Select(this.VisitedRow).ToList(),
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

        private VisitedPlaceRowViewModel VisitedRow(VisitedPlaceKind kind) => VisitedPlaceRows.Create(
            kind.Name,
            kind.ToolTip,
            this.places.Of(kind).Select(place => (place.Number, place.Position)),
            () => this.getHero().PlaceVisits.Of(kind),
            edit => this.update(h => h with { PlaceVisits = h.PlaceVisits.With(kind, edit(h.PlaceVisits.Of(kind))) }));
    }
}
