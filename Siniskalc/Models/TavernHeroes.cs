namespace Siniskalc.Models
{
    using System.Collections.Generic;
    using Siniskalc.Core;

    /// <summary>
    /// The two heroes the tavern of a player offers for hire, by hero id. A place the tavern has no hero for is <c>null</c>.
    /// </summary>
    /// <param name="First">The id of the hero in the first place, or <c>null</c>.</param>
    /// <param name="Second">The id of the hero in the second place, or <c>null</c>.</param>
    public sealed record TavernHeroes(int? First, int? Second)
    {
        /// <summary>
        /// How many heroes a tavern offers.
        /// </summary>
        public const int Places = 2;

        /// <summary>
        /// Gets a tavern with no hero in it.
        /// </summary>
        public static TavernHeroes Empty { get; } = new(null, null);

        /// <summary>
        /// Gets the hero ids of the places, in order.
        /// </summary>
        public IReadOnlyList<int?> Ids => [this.First, this.Second];

        /// <summary>
        /// Returns whether <paramref name="heroId"/> is in one of the places.
        /// </summary>
        public bool Contains(int heroId) => this.First == heroId || this.Second == heroId;

        /// <summary>
        /// Returns the tavern with <paramref name="heroId"/> (or nobody) in the place at <paramref name="place"/>, 0 for the first.
        /// </summary>
        public TavernHeroes With(int place, int? heroId)
        {
            RangeGuard.ThrowIfNotIndex(place, Places, nameof(place));
            if (heroId is { } id)
            {
                RangeGuard.ThrowIfOutOfRange(id, Constants.Player.MinHeroId, Constants.Player.MaxHeroId, nameof(heroId));
            }

            return place == 0 ? this with { First = heroId } : this with { Second = heroId };
        }
    }
}
