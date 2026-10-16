namespace Siniskalc.Heroes.Entities
{
    /// <summary>
    /// Something a creature can do or suffer beyond the plain attack and defense of its stats. Each game has its own set of them
    /// (<c>Siniskalc.Homm3.Entities.CreatureAbility</c>, <c>Siniskalc.Homm2.Entities.CreatureAbility</c>), as the sets differ.
    /// </summary>
    public interface ICreatureAbility
    {
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this is the ability to shoot, so that the number of shots (<see cref="CreatureInfo.Shots"/>) is shown with it.
        /// </summary>
        bool IsRanged { get; }
    }
}
