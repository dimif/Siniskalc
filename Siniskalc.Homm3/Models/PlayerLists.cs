namespace Siniskalc.Homm3.Models
{
    using System.Linq;
    using Siniskalc.Core.Models;

    /// <summary>
    /// The ordered hero and town lists a player's record keeps, with the selected hero. Ownership lives in the heroes and castles
    /// themselves, and in the table of hero owners: these lists follow it, so no page edits them. They are put in step when an owner changes
    /// (<see cref="Data.PlayerListsReconciler"/>).
    /// </summary>
    /// <param name="HeroIds">The ids of the heroes on the map of the player, in the order of its list (a hero garrisoned in a castle is not in it).</param>
    /// <param name="SelectedHero">The id of the hero that is selected on the map, or <c>null</c> (a town is selected or nothing).</param>
    public sealed record PlayerLists(Numbers HeroIds, int? SelectedHero)
    {
        /// <summary>Gets the towns in acquisition order.</summary>
        public Numbers TownIds { get; init; } = new([]);

        /// <summary>
        /// Adds a newly acquired town at the end or removes it without reordering the others.
        /// Giving an original town back restores its place, as for heroes, so reset and revert restore the save.
        /// </summary>
        /// <param name="townId">The global town id.</param>
        /// <param name="owns">Whether the player owns it now.</param>
        /// <param name="original">The lists as loaded or last saved.</param>
        /// <returns>The new lists, leaving selection unchanged.</returns>
        public PlayerLists WithTown(int townId, bool owns, PlayerLists original)
        {
            if (owns == this.TownIds.Contains(townId))
            {
                return this;
            }

            var others = this.TownIds.Where(id => id != townId).ToList();
            var ids = !owns ? new Numbers(others)
                : original.TownIds.Contains(townId)
                    ? new Numbers(original.TownIds.Where(id => id == townId || others.Contains(id)).Concat(others.Where(id => !original.TownIds.Contains(id))))
                    : new Numbers(others.Append(townId));
            return this with { TownIds = ids };
        }

        /// <summary>
        /// Returns the list with the hero in it or out of it. A hero that joins goes back to its place when <paramref name="original"/> has it,
        /// and after the others when it does not, as a hired hero does in the game (<c>hero_owners_02</c> to <c>03</c>); the selected hero does
        /// not change. A hero that leaves lets the heroes after it move up a place, and if it was the selected hero, the first hero left is
        /// selected (none when no hero is left), as the game does when a hero is defeated (<c>hero_owners_00</c> to <c>01</c>). Once the list
        /// is as in <paramref name="original"/>, its selected hero comes back too. A list that already says so is returned as it is. The list
        /// may come out longer than the game allows; whoever makes the change checks that.
        /// </summary>
        /// <param name="heroId">The id of the hero.</param>
        /// <param name="owns">Whether the player has the hero now.</param>
        /// <param name="original">The list as the savegame had it.</param>
        /// <returns>The new lists; this one is not changed.</returns>
        public PlayerLists WithHero(int heroId, bool owns, PlayerLists original)
        {
            if (owns == this.HeroIds.Contains(heroId))
            {
                return this;
            }

            var others = this.HeroIds.Where(id => id != heroId).ToList();
            Numbers ids;
            if (!owns)
            {
                ids = new Numbers(others);
            }
            else if (original.HeroIds.Contains(heroId))
            {
                ids = new Numbers(original.HeroIds.Where(id => id == heroId || others.Contains(id)).Concat(others.Where(id => !original.HeroIds.Contains(id))));
            }
            else
            {
                ids = new Numbers(others.Append(heroId));
            }

            if (ids.Equals(original.HeroIds))
            {
                return this with { HeroIds = ids, SelectedHero = original.SelectedHero };
            }

            var selected = !owns && this.SelectedHero == heroId ? (ids.Count > 0 ? ids[0] : null) : this.SelectedHero;
            return this with { HeroIds = ids, SelectedHero = selected };
        }
    }
}
