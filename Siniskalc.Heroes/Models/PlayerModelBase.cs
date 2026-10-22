namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// What a player is in every game of the series: a colour, a stock of resources, the heroes its taverns offer, and the countdown
    /// that puts it out of the game once it owns no town. A game's player derives from it and adds the rest.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game, which is an enumeration of its own.</typeparam>
    /// <param name="Color">The colour of the player.</param>
    /// <param name="Resources">What the player owns.</param>
    /// <param name="Tavern">The heroes the taverns of the player offer.</param>
    /// <param name="DaysLeft">
    /// The days before the player is out of the game for owning no town, <see cref="Constants.Player.MaxDaysLeft"/> at most, or 0 once it
    /// is out; <c>null</c> while the player owns a town.
    /// </param>
    public abstract record PlayerModelBase<TColor>(TColor Color, ResourceSet Resources, TavernHeroes Tavern, int? DaysLeft)
    {
        /// <summary>
        /// Gets whether the countdown has run out.
        /// </summary>
        public bool IsBanished => this.DaysLeft == Constants.Player.MinDaysLeft;

        /// <summary>
        /// Gets whether a countdown is running: the player owns no town and is not out yet.
        /// </summary>
        public bool HasCountdown => this.DaysLeft is > Constants.Player.MinDaysLeft;

        /// <summary>
        /// Returns the days left once the player has gained a town: none, as the player owns one now and its countdown stops. A banished player
        /// stays banished.
        /// </summary>
        public int? DaysLeftOnGainingATown() => this.IsBanished ? this.DaysLeft : null;

        /// <summary>
        /// Returns the days left once the player has lost a town and owns <paramref name="townsLeft"/>: a player who lost the last one starts
        /// the countdown at <see cref="Constants.Player.MaxDaysLeft"/> (or keeps the one that runs), a player who keeps a town keeps what it
        /// had, and a banished player stays banished.
        /// </summary>
        /// <param name="townsLeft">The towns the player owns after the loss.</param>
        public int? DaysLeftOnLosingATown(int townsLeft) =>
            this.IsBanished || townsLeft > 0 ? this.DaysLeft : this.DaysLeft ?? Constants.Player.MaxDaysLeft;
    }
}
