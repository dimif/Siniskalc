namespace Siniskalc.Heroes.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// A level of a secondary skill. A hero that does not have the skill has no level at all, which is <c>null</c>.
    /// </summary>
    public interface ISkillLevel : IGameEntity
    {
        /// <summary>
        /// Gets how far the skill is learned: 1 for basic, 2 for advanced, 3 for expert.
        /// </summary>
        int Rank { get; }
    }
}
