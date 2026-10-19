namespace Siniskalc.Heroes.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// A colour a player can have in a game. Each game has its own colours and its own codes for them; what the colours look like in
    /// the editor is up to <see cref="Siniskalc.Heroes.ViewModels.PlayerPalette"/>.
    /// </summary>
    public interface IPlayerColor : IGameEntity
    {
        /// <summary>
        /// Gets whether this is the owner of what no player owns: a castle, a hero or a mine nobody has.
        /// </summary>
        bool IsNeutral { get; }
    }
}
