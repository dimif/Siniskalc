namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// A faction of Heroes of Might and Magic II, for what works with the factions of every game.
    /// </summary>
    /// <param name="Name">The name of the faction.</param>
    /// <param name="Code">The number the savegame holds for it.</param>
    public sealed record FactionInfo(string Name, byte Code) : IFaction;
}
