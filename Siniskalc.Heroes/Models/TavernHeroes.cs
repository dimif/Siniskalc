namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
    using Siniskalc.Core;

    /// <summary>
    /// The two heroes the tavern of a player offers for hire, by hero number. A place the tavern has no hero for is <c>null</c>.
    /// </summary>
    /// <param name="First">The number of the hero in the first place, or <c>null</c>.</param>
    /// <param name="Second">The number of the hero in the second place, or <c>null</c>.</param>
    public readonly record struct TavernHeroes(int? First, int? Second)
    {
        /// <summary>
        /// How many heroes a tavern offers.
        /// </summary>
        public const int Places = Constants.Tavern.Places;

        /// <summary>
        /// Gets a tavern with no hero in it.
        /// </summary>
        public static TavernHeroes Empty => default;

        /// <summary>
        /// Gets the hero numbers of the places, in order.
        /// </summary>
        public IReadOnlyList<int?> Ids => [this.First, this.Second];

        /// <summary>
        /// Returns whether <paramref name="heroId"/> is in one of the places.
        /// </summary>
        /// <param name="heroId">The number of the hero.</param>
        /// <returns><see langword="true"/> if the hero is in the tavern.</returns>
        public bool Contains(int heroId) => this.First == heroId || this.Second == heroId;

        /// <summary>
        /// Returns the tavern with <paramref name="heroId"/> (or nobody) in the place at <paramref name="place"/>, 0 for the first.
        /// </summary>
        /// <param name="place">The place, from 0.</param>
        /// <param name="heroId">The number of the hero, or <see langword="null"/> for nobody.</param>
        /// <returns>The new tavern; this one is not changed.</returns>
        public TavernHeroes With(int place, int? heroId)
        {
            RangeGuard.ThrowIfNotIndex(place, Places, nameof(place));
            if (heroId is { } id)
            {
                RangeGuard.ThrowIfOutOfRange(id, Constants.Hero.MinId, Constants.Hero.MaxId, nameof(heroId));
            }

            return place == 0 ? this with { First = heroId } : this with { Second = heroId };
        }
    }
}
