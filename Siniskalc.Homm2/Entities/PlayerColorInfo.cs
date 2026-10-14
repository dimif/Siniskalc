namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// A colour of a player of Heroes of Might and Magic II, for what works with the colours of every game.
    /// </summary>
    /// <param name="Name">The name of the colour.</param>
    /// <param name="Code">The number the savegame holds for it.</param>
    public sealed record PlayerColorInfo(string Name, byte Code) : IPlayerColor;
}
