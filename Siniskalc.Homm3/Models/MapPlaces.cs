namespace Siniskalc.Homm3.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// The shared <see cref="MapPlaces{TKind}"/> with what a map of Heroes III adds: its merchants, and the places a hero stands on. A place that a
    /// hero stands on is not found, so a kind can have fewer than it has until <see cref="WithStanding"/> adds them.
    /// </summary>
    public sealed class MapPlaces : MapPlaces<VisitedPlaceKind>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPlaces"/> class.
        /// </summary>
        /// <param name="places">The places of each kind, with the number of each among the places of its kind.</param>
        /// <param name="merchants">The merchants of the map, empty for none.</param>
        public MapPlaces(IReadOnlyDictionary<VisitedPlaceKind, IReadOnlyList<(int Number, MapPosition Position)>> places, IReadOnlyList<(int Number, MapPosition Position)> merchants)
            : base(places)
        {
            this.Merchants = merchants;
        }

        /// <summary>
        /// Gets the places of a map in which none were found.
        /// </summary>
        public static new MapPlaces None { get; } = new(new Dictionary<VisitedPlaceKind, IReadOnlyList<(int Number, MapPosition Position)>>(), []);

        /// <summary>
        /// Gets where the merchants of the map are, by their number among the merchants (the index of their stock). A merchant a hero stands on is not found.
        /// </summary>
        public IReadOnlyList<(int Number, MapPosition Position)> Merchants { get; }

        /// <summary>
        /// Returns the places with those that a hero stands on added: such a place is not found on the map, as the hero hides it, but the hero's
        /// own record says what it stands on. A place the map already has is kept as it is.
        /// </summary>
        /// <param name="standing">The places a hero stands on, which the map hides.</param>
        /// <returns>The new places; these are not changed.</returns>
        public MapPlaces WithStanding(IEnumerable<(VisitedPlaceKind Kind, int Number, MapPosition Position)> standing)
        {
            var merged = new Dictionary<VisitedPlaceKind, IReadOnlyList<(int Number, MapPosition Position)>>(this.Places);
            foreach (var group in standing.GroupBy(s => s.Kind))
            {
                var known = this.Of(group.Key).ToList();
                foreach (var hero in group.Where(s => known.All(k => k.Number != s.Number)))
                {
                    known.Add((hero.Number, hero.Position));
                }

                merged[group.Key] = known.OrderBy(k => k.Number).ToList();
            }

            return new MapPlaces(merged, this.Merchants);
        }
    }
}
