namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Core;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Immutable view of a player. Edit it with <c>with</c> expressions and write it back through
    /// <see cref="Data.PlayerEditSession.Update"/>.
    /// </summary>
    /// <param name="Color">The player's colour, which is also how the game names the player.</param>
    /// <param name="Resources">What the player owns.</param>
    /// <param name="Tavern">The two heroes the tavern of the player offers. Found by comparing savefiles; see docs/KNOWLEDGE_BASE.md.</param>
    /// <param name="DaysLeft">
    /// The days before the player is banished for owning no town, 1 to 7, or 0 when the player has been banished; null while the player owns a town.
    /// </param>
    public sealed record PlayerModel(PlayerColor Color, PlayerResources Resources, TavernHeroes Tavern, int? DaysLeft = null)
    {
        /// <summary>
        /// Gets whether the player is out of the game.
        /// </summary>
        public bool IsBanished => this.DaysLeft == Constants.Player.MinDaysLeft;

        /// <summary>
        /// Gets whether a countdown to banishing is running: the player has no town and is not out yet.
        /// </summary>
        public bool HasCountdown => this.DaysLeft is > Constants.Player.MinDaysLeft;

        /// <summary>
        /// Returns the player with <paramref name="days"/> left before banishing, 1 to 7. A banished player stays banished.
        /// </summary>
        public PlayerModel WithDaysLeft(int days)
        {
            RangeGuard.ThrowIfOutOfRange(days, Constants.Player.MinDaysLeft + 1, Constants.Player.MaxDaysLeft, nameof(days));
            return this.IsBanished ? this : this with { DaysLeft = days };
        }
    }
}
