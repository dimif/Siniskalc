namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Immutable view of a player. Edit it with <c>with</c> expressions and write it back through
    /// <see cref="Data.PlayerEditSession.Update"/>. The list of heroes the record of the player keeps follows the owners of the heroes and is
    /// not part of it (<see cref="PlayerLists"/>).
    /// </summary>
    /// <param name="Color">The player's colour, which is also how the game names the player.</param>
    /// <param name="Resources">What the player owns.</param>
    /// <param name="Tavern">The two heroes the tavern of the player offers. Found by comparing savefiles; see docs/KNOWLEDGE_BASE.md.</param>
    /// <param name="DaysLeft">
    /// The days before the player is banished for owning no town, 1 to 7, or 0 when the player has been banished; null while the player owns a town.
    /// </param>
    public sealed record PlayerModel(PlayerColor Color, ResourceSet Resources, TavernHeroes Tavern, int? DaysLeft = null)
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
    }
}
