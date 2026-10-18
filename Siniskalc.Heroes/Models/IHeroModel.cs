namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Represents common elements of a hero across multiple game versions.
    /// </summary>
    /// <typeparam name="TSecondarySkill">
    /// The game-specific type of secondary skills.
    /// </typeparam>
    /// <typeparam name="TSecondarySkills">
    /// The type of secondary skills collection.
    /// </typeparam>
    public interface IHeroModel<TSecondarySkill, TSecondarySkills>
        where TSecondarySkill : ISecondarySkill
        where TSecondarySkills : ISecondarySkills<TSecondarySkill, TSecondarySkills>
    {
        /// <summary>
        /// Gets the name of the hero.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the unique identifier associated with hero.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the primary skills of the hero.
        /// </summary>
        PrimarySkills PrimarySkills { get; }

        /// <summary>
        /// Gets the secondary skills of the hero.
        /// </summary>
        TSecondarySkills SecondarySkills { get; }
    }
}
