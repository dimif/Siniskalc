namespace Siniskalc.Models
{
    using Siniskalc.Entities;

    /// <summary>
    /// Immutable view of a player. Edit it with <c>with</c> expressions and write it back through
    /// <see cref="Data.PlayerEditSession.Update"/>.
    /// </summary>
    /// <param name="Color">The player's colour, which is also how the game names the player.</param>
    /// <param name="Resources">What the player owns.</param>
    /// <param name="Tavern">The two heroes the tavern of the player offers. Found by comparing savefiles; see docs/KNOWLEDGE_BASE.md.</param>
    public sealed record PlayerModel(PlayerColor Color, PlayerResources Resources, TavernHeroes Tavern);
}
