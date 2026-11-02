namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// The countdown every player of the series has, whatever the colours of its game: once it owns no town, the player has
    /// <see cref="Constants.Player.MaxDaysLeft"/> days to own one again before it is out of the game. It knows no colour, so that a reducer
    /// such as <see cref="PlayerModelExtensions.WithDaysLeft"/> can keep the type of a game's player. A game's player derives from
    /// <see cref="PlayerModelBase{TColor}"/>, which derives from it.
    /// </summary>
    /// <param name="DaysLeft">
    /// The days before the player is out of the game for owning no town, <see cref="Constants.Player.MaxDaysLeft"/> at most, or 0 once it
    /// is out; <c>null</c> while the player owns a town.
    /// </param>
    public abstract record BanishablePlayerBase(int? DaysLeft)
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
        /// <returns>The days left, or <see langword="null"/> if the player is not counting down.</returns>
        public int? DaysLeftOnGainingATown() => this.IsBanished ? this.DaysLeft : null;

        /// <summary>
        /// Returns the days left once the player has lost a town and owns <paramref name="townsLeft"/>: a player who lost the last one starts
        /// the countdown at <see cref="Constants.Player.MaxDaysLeft"/> (or keeps the one that runs), a player who keeps a town keeps what it
        /// had, and a banished player stays banished.
        /// </summary>
        /// <param name="townsLeft">The towns the player owns after the loss.</param>
        /// <returns>The days left, or <see langword="null"/> if the player still owns a town.</returns>
        public int? DaysLeftOnLosingATown(int townsLeft) =>
            this.IsBanished || townsLeft > 0 ? this.DaysLeft : this.DaysLeft ?? Constants.Player.MaxDaysLeft;
    }
}
