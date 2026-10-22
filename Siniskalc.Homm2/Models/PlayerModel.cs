namespace Siniskalc.Homm2.Models
{
    using System.Linq;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A player of Heroes of Might and Magic II as the savegame has it. There is a record for each of the six colours, whether the colour
    /// is in the game or not.
    /// </summary>
    /// <param name="Color">The colour of the player, which is its place in the table of players.</param>
    /// <param name="Resources">The stock of resources.</param>
    /// <param name="Income">What the player earns each day, worked out by the game at the start of the day (mines, castles and artifacts).</param>
    /// <param name="HeroIds">The numbers of the heroes the player has.</param>
    /// <param name="SelectedHero">The hero that is selected on the map, or <c>null</c> (a castle is selected or nothing).</param>
    /// <param name="CastleIds">The numbers of the castles and towns the player has.</param>
    /// <param name="SelectedCastle">The castle that is selected, or <c>null</c>.</param>
    /// <param name="DaysLeft">How many days the player has left to own a castle or a town before being out of the game, or <c>null</c> while he owns one.</param>
    /// <param name="Tavern">The heroes the taverns offer.</param>
    public sealed record PlayerModel(
        PlayerColor Color,
        ResourceSet Resources,
        ResourceSet Income,
        Numbers HeroIds,
        int? SelectedHero,
        Numbers CastleIds,
        int? SelectedCastle,
        int? DaysLeft,
        TavernHeroes Tavern)
        : PlayerModelBase<PlayerColor>(Color, Resources, Tavern, DaysLeft)
    {
        /// <summary>
        /// The days a player has to own a castle again once the last one is lost (7 in the savegames: the day after the last castle of
        /// Blue was taken the count was 7, and the player is out of the game when it reaches 0).
        /// </summary>
        public const int DaysToOwnACastle = Heroes.Constants.Player.MaxDaysLeft;

        /// <summary>
        /// Returns the player with the hero in its list of heroes.
        /// </summary>
        /// <exception cref="GameRuleException">The player has the most heroes the list holds.</exception>
        public PlayerModel WithHero(int heroId)
        {
            if (this.HeroIds.Contains(heroId))
            {
                return this;
            }

            return this.HeroIds.Count >= Constants.Player.HeroListSize
                ? throw new GameRuleException($"A player has at most {Constants.Player.HeroListSize} heroes.")
                : this with { HeroIds = new Numbers(this.HeroIds.Append(heroId)) };
        }

        /// <summary>
        /// Returns the player without the hero in its list of heroes (and without it as the selected hero).
        /// </summary>
        public PlayerModel WithoutHero(int heroId) => this.HeroIds.Contains(heroId)
            ? this with { HeroIds = new Numbers(this.HeroIds.Where(h => h != heroId)), SelectedHero = this.SelectedHero == heroId ? null : this.SelectedHero }
            : this;

        /// <summary>
        /// Returns the player with the castle in its list of castles. The days left to own a castle are cleared, as the player owns one.
        /// </summary>
        /// <exception cref="GameRuleException">The player has the most castles the list holds.</exception>
        public PlayerModel WithCastle(int castleIndex)
        {
            if (this.CastleIds.Contains(castleIndex))
            {
                return this;
            }

            return this.CastleIds.Count >= Constants.Player.CastleListSize
                ? throw new GameRuleException($"A player has at most {Constants.Player.CastleListSize} castles.")
                : this with { CastleIds = new Numbers(this.CastleIds.Append(castleIndex)), DaysLeft = null };
        }

        /// <summary>
        /// Returns the player without the castle in its list of castles. A player left with none starts counting the days to own a castle.
        /// </summary>
        public PlayerModel WithoutCastle(int castleIndex)
        {
            if (!this.CastleIds.Contains(castleIndex))
            {
                return this;
            }

            var castles = new Numbers(this.CastleIds.Where(c => c != castleIndex));
            return this with
            {
                CastleIds = castles,
                SelectedCastle = this.SelectedCastle == castleIndex ? null : this.SelectedCastle,
                DaysLeft = castles.Count == 0 ? this.DaysLeft ?? DaysToOwnACastle : this.DaysLeft,
            };
        }
    }
}
