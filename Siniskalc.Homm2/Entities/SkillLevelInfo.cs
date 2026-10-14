namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// A level of a secondary skill of Heroes of Might and Magic II, for what works with the levels of every game.
    /// </summary>
    /// <param name="Name">The name of the level, as the game shows it.</param>
    /// <param name="Code">The number the savegame holds for it.</param>
    /// <param name="Rank">1 for basic, 2 for advanced, 3 for expert.</param>
    public sealed record SkillLevelInfo(string Name, byte Code, int Rank) : ISkillLevel;
}
