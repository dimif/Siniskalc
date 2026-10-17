namespace Siniskalc.Heroes.Entities
{
    /// <summary>
    /// A faction of a game whose factions are an enum, for what works with the factions of every game.
    /// </summary>
    /// <param name="Name">The name of the faction.</param>
    /// <param name="Code">The number the savegame holds for it.</param>
    public sealed record FactionInfo(string Name, byte Code) : IFaction;
}
