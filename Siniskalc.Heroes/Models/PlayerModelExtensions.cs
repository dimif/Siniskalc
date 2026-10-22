namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Core;

    /// <summary>
    /// Reducers of a player of any game that keep the type of the game's player.
    /// </summary>
    public static class PlayerModelExtensions
    {
        /// <summary>
        /// Returns the player with <paramref name="days"/> left before it is out of the game, 1 to <see cref="Constants.Player.MaxDaysLeft"/>.
        /// A banished player stays banished and is returned as it is.
        /// </summary>
        /// <typeparam name="TPlayer">The player of the game.</typeparam>
        /// <param name="player">The player.</param>
        /// <param name="days">The days left.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="days"/> is not 1 to <see cref="Constants.Player.MaxDaysLeft"/>.</exception>
        public static TPlayer WithDaysLeft<TPlayer>(this TPlayer player, int days)
            where TPlayer : BanishablePlayerBase
        {
            RangeGuard.ThrowIfOutOfRange(days, Constants.Player.MinDaysLeft + 1, Constants.Player.MaxDaysLeft, nameof(days));
            return player.IsBanished ? player : player with { DaysLeft = days };
        }
    }
}
