namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A player of Heroes of Might and Magic II as the savegame has it. There is a record for each of the six colours, whether the colour
    /// is in the game or not. The lists of heroes and castles the record also keeps follow the owners of the heroes and the castles, and are
    /// not part of the player (<see cref="PlayerLists"/>).
    /// </summary>
    /// <param name="Color">The colour of the player, which is its place in the table of players.</param>
    /// <param name="Resources">The stock of resources.</param>
    /// <param name="Income">What the player earns each day, worked out by the game at the start of the day (mines, castles and artifacts).</param>
    /// <param name="DaysLeft">How many days the player has left to own a castle or a town before being out of the game, or <c>null</c> while he owns one.</param>
    /// <param name="Tavern">The heroes the taverns offer.</param>
    public sealed record PlayerModel(
        PlayerColor Color,
        ResourceSet Resources,
        ResourceSet Income,
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
        /// Returns the player once it has gained a castle or a town: its countdown stops. A banished player stays banished.
        /// </summary>
        /// <returns>The new player; this one is not changed.</returns>
        public PlayerModel WithTownGained() => this with { DaysLeft = this.DaysLeftOnGainingATown() };

        /// <summary>
        /// Returns the player once it has lost a castle or a town and owns <paramref name="townsLeft"/>: losing the last one starts the countdown
        /// of <see cref="DaysToOwnACastle"/> days. A banished player stays banished.
        /// </summary>
        /// <param name="townsLeft">The castles and towns the player owns after the loss.</param>
        /// <returns>The new player; this one is not changed.</returns>
        public PlayerModel WithTownLost(int townsLeft) => this with { DaysLeft = this.DaysLeftOnLosingATown(townsLeft) };
    }
}
