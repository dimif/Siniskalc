namespace Siniskalc.Homm3.Models
{
    using System.Linq;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Immutable view of a player. Edit it with <c>with</c> expressions and write it back through
    /// <see cref="Data.PlayerEditSession.Update"/>.
    /// </summary>
    /// <param name="Color">The player's colour, which is also how the game names the player.</param>
    /// <param name="Resources">What the player owns.</param>
    /// <param name="Tavern">The two heroes the tavern of the player offers. Found by comparing savefiles; see docs/KNOWLEDGE_BASE.md.</param>
    /// <param name="HeroIds">The ids of the heroes on the map of the player, in the order of its list (a hero garrisoned in a castle is not in it).</param>
    /// <param name="SelectedHero">The id of the hero that is selected on the map, or <c>null</c> (a town is selected or nothing).</param>
    /// <param name="DaysLeft">
    /// The days before the player is banished for owning no town, 1 to 7, or 0 when the player has been banished; null while the player owns a town.
    /// </param>
    public sealed record PlayerModel(PlayerColor Color, ResourceSet Resources, TavernHeroes Tavern, Numbers HeroIds, int? SelectedHero, int? DaysLeft = null)
        : PlayerModelBase<PlayerColor>(Color, Resources, Tavern, DaysLeft)
    {
        /// <summary>
        /// Returns the player once it has gained a town: its countdown stops. A banished player stays banished.
        /// </summary>
        public PlayerModel WithTownGained() => this with { DaysLeft = this.DaysLeftOnGainingATown() };

        /// <summary>
        /// Returns the player once it has lost a town and owns <paramref name="townsLeft"/>: losing the last one starts the countdown of
        /// <see cref="Constants.Player.MaxDaysLeft"/> days. A banished player stays banished.
        /// </summary>
        /// <param name="townsLeft">The towns the player owns after the loss.</param>
        public PlayerModel WithTownLost(int townsLeft) => this with { DaysLeft = this.DaysLeftOnLosingATown(townsLeft) };

        /// <summary>
        /// Returns the player once it has gained the hero: the hero goes to the first free place of the list, after the others, as a hired hero
        /// does in the game (<c>hero_owners_02</c> to <c>03</c>); the selected hero does not change.
        /// </summary>
        /// <exception cref="GameRuleException">The list of the player is full.</exception>
        public PlayerModel WithHero(int heroId)
        {
            if (this.HeroIds.Contains(heroId))
            {
                return this;
            }

            return this.HeroIds.Count >= Constants.Player.HeroListSize
                ? throw new GameRuleException($"{this.Color} has the most heroes a player can have on the map ({Constants.Player.HeroListSize}).")
                : this with { HeroIds = new Numbers(this.HeroIds.Append(heroId)) };
        }

        /// <summary>
        /// Returns the player once it has lost the hero: the heroes after it move up a place, and if it was the selected hero, the first hero
        /// left is selected (none when no hero is left), as the game does when a hero is defeated (<c>hero_owners_00</c> to <c>01</c>).
        /// </summary>
        public PlayerModel WithoutHero(int heroId)
        {
            if (!this.HeroIds.Contains(heroId))
            {
                return this;
            }

            var left = new Numbers(this.HeroIds.Where(id => id != heroId));
            var selected = this.SelectedHero == heroId ? (left.Count > 0 ? left[0] : null) : this.SelectedHero;
            return this with { HeroIds = left, SelectedHero = selected };
        }

        /// <summary>
        /// Returns the player with the hero back in its list or out of it, as it was in <paramref name="original"/>, and the other heroes left
        /// where they are: the heroes of <paramref name="original"/> keep its order and the heroes gained since follow. When that gives the list
        /// of <paramref name="original"/>, its selected hero comes back too. Puts back what <see cref="WithHero"/> and <see cref="WithoutHero"/>
        /// did for one hero.
        /// </summary>
        /// <param name="heroId">The hero whose place is put back.</param>
        /// <param name="original">The player as the savegame had it.</param>
        /// <exception cref="GameRuleException">The hero goes back to a list that has become full.</exception>
        public PlayerModel WithHeroAsIn(int heroId, PlayerModel original)
        {
            var had = original.HeroIds.Contains(heroId);
            var others = this.HeroIds.Where(id => id != heroId).ToList();
            var ids = new Numbers(original.HeroIds.Where(id => (id == heroId && had) || others.Contains(id)).Concat(others.Where(id => !original.HeroIds.Contains(id))));
            if (ids.Count > Constants.Player.HeroListSize)
            {
                throw new GameRuleException($"{this.Color} has the most heroes a player can have on the map ({Constants.Player.HeroListSize}).");
            }

            if (ids.Equals(original.HeroIds))
            {
                return this with { HeroIds = ids, SelectedHero = original.SelectedHero };
            }

            var selected = this.SelectedHero == heroId && !had ? (ids.Count > 0 ? ids[0] : null) : this.SelectedHero;
            return this with { HeroIds = ids, SelectedHero = selected };
        }
    }
}
