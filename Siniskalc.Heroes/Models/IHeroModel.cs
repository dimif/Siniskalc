namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Core.Entities;
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
    /// <typeparam name="TCreature">The creature of the game.</typeparam>
    public interface IHeroModel<TSecondarySkill, TSecondarySkills, TCreature>
        where TSecondarySkill : ISecondarySkill
        where TSecondarySkills : ISecondarySkills<TSecondarySkill, TSecondarySkills>
        where TCreature : class, IGameEntity
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

        /// <summary>
        /// Gets the army of the hero.
        /// </summary>
        IArmy<TCreature> Army { get; }

        /// <summary>
        /// Gets the current spell points of the hero.
        /// </summary>
        int SpellPoints { get; }
    }
}
