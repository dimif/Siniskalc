namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Where the places of each kind a hero can visit are on the map, by their number among the places of the kind: the number of the bit a hero
    /// sets when it visits one. The loader of each game numbers the places the way its savegame does; this only holds them.
    /// </summary>
    /// <typeparam name="TKind">The kind of place of the game.</typeparam>
    public class MapPlaces<TKind>
        where TKind : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPlaces{TKind}"/> class.
        /// </summary>
        /// <param name="places">The places of each kind, with the number of each among the places of its kind, in the order of their number.</param>
        public MapPlaces(IReadOnlyDictionary<TKind, IReadOnlyList<(int Number, MapPosition Position)>> places)
        {
            this.Places = places;
        }

        /// <summary>
        /// Gets the places of a map in which none were found.
        /// </summary>
        public static MapPlaces<TKind> None { get; } = new(new Dictionary<TKind, IReadOnlyList<(int Number, MapPosition Position)>>());

        /// <summary>
        /// Gets the places of each kind, with the number of each among the places of its kind.
        /// </summary>
        protected IReadOnlyDictionary<TKind, IReadOnlyList<(int Number, MapPosition Position)>> Places { get; }

        /// <summary>
        /// Returns the places of a kind, in the order of their number.
        /// </summary>
        /// <param name="kind">The kind of place.</param>
        /// <returns>The number and the position of each place of the kind; none for a kind the map has none of.</returns>
        public IReadOnlyList<(int Number, MapPosition Position)> Of(TKind kind) => this.Places.TryGetValue(kind, out var found) ? found : [];
    }
}
