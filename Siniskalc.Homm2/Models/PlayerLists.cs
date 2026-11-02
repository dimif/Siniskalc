namespace Siniskalc.Homm2.Models
{
    using System.Linq;
    using Siniskalc.Core.Models;

    /// <summary>
    /// The lists of heroes and castles that the record of a player keeps, with the hero and the castle that are selected on the map. The
    /// owner of a hero or a castle is kept in the hero or the castle itself: these lists follow it, so no page edits them. They are put in
    /// step when an owner changes (<see cref="Data.PlayerListsReconciler"/>).
    /// </summary>
    /// <param name="HeroIds">The numbers of the heroes the player has, in the order of the list.</param>
    /// <param name="SelectedHero">The hero that is selected on the map, or <c>null</c> (a castle is selected or nothing).</param>
    /// <param name="CastleIds">The numbers of the castles and towns the player has, in the order of the list.</param>
    /// <param name="SelectedCastle">The castle that is selected, or <c>null</c>.</param>
    public sealed record PlayerLists(Numbers HeroIds, int? SelectedHero, Numbers CastleIds, int? SelectedCastle)
    {
        /// <summary>
        /// Gets whether the player has a hero or a castle.
        /// </summary>
        public bool OwnsAnything => this.HeroIds.Count > 0 || this.CastleIds.Count > 0;

        /// <summary>
        /// Returns the lists with the hero in the list of heroes or out of it. A hero that joins goes back to its place when
        /// <paramref name="original"/> has it, and after the others when it does not; a hero that leaves takes the selection with it. Once
        /// the list is as in <paramref name="original"/>, its selected hero comes back too. Lists that already say so are returned as they are. A
        /// list may come out longer than the game allows; whoever makes the change refuses it (<see cref="Data.PlayerListsReconciler"/>).
        /// </summary>
        /// <param name="heroId">The number of the hero.</param>
        /// <param name="owns">Whether the player has the hero now.</param>
        /// <param name="original">The lists as the savegame had them.</param>
        /// <returns>The new lists; these are not changed.</returns>
        public PlayerLists WithHero(int heroId, bool owns, PlayerLists original)
        {
            if (owns == this.HeroIds.Contains(heroId))
            {
                return this;
            }

            var ids = Place(this.HeroIds, heroId, owns, original.HeroIds);
            var selected = ids.Equals(original.HeroIds) ? original.SelectedHero : !owns && this.SelectedHero == heroId ? null : this.SelectedHero;
            return this with { HeroIds = ids, SelectedHero = selected };
        }

        /// <summary>
        /// Returns the lists with the castle in the list of castles or out of it, in the way of <see cref="WithHero"/>.
        /// </summary>
        /// <param name="castleIndex">The number of the castle.</param>
        /// <param name="owns">Whether the player has the castle now.</param>
        /// <param name="original">The lists as the savegame had them.</param>
        /// <returns>The new lists; these are not changed.</returns>
        public PlayerLists WithCastle(int castleIndex, bool owns, PlayerLists original)
        {
            if (owns == this.CastleIds.Contains(castleIndex))
            {
                return this;
            }

            var ids = Place(this.CastleIds, castleIndex, owns, original.CastleIds);
            var selected = ids.Equals(original.CastleIds) ? original.SelectedCastle : !owns && this.SelectedCastle == castleIndex ? null : this.SelectedCastle;
            return this with { CastleIds = ids, SelectedCastle = selected };
        }

        // The list without the item, or with it: where the original list has it (the other items keeping their order, those the original has
        // first), or after the others when the original has not.
        private static Numbers Place(Numbers list, int item, bool owns, Numbers original)
        {
            var others = list.Where(id => id != item).ToList();
            if (!owns)
            {
                return new Numbers(others);
            }

            return original.Contains(item)
                ? new Numbers(original.Where(id => id == item || others.Contains(id)).Concat(others.Where(id => !original.Contains(id))))
                : new Numbers(others.Append(item));
        }
    }
}
