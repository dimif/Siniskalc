namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
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
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public interface IHeroModel<TSecondarySkill, TSecondarySkills, TCreature, TColor>
        where TSecondarySkill : ISecondarySkill
        where TSecondarySkills : ISecondarySkills<TSecondarySkill, TSecondarySkills>
        where TCreature : class, IGameEntity
        where TColor : class, IPlayerColor
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
        /// Gets the player that owns the hero, or the colour <c>Neutral</c> when no player does.
        /// </summary>
        TColor Owner { get; }

        /// <summary>
        /// Gets where the hero stands and which way it faces.
        /// </summary>
        HeroPosition Position { get; }

        /// <summary>
        /// Gets the tile the hero is going to, or <see cref="MapPosition.None"/>.
        /// </summary>
        MapPosition Destination { get; }

        /// <summary>
        /// Gets the level of the hero and its experience.
        /// </summary>
        HeroLevel HeroLevel { get; }

        /// <summary>
        /// Gets the movement points the hero has left today and the ones of a full day.
        /// </summary>
        MovementPoints MovementPoints { get; }

        /// <summary>
        /// Gets whether the hero carries a spell book; without one the spells it knows have no effect.
        /// </summary>
        bool HasSpellBook { get; }

        /// <summary>
        /// Gets the spells the hero has learned, not counting the ones an artifact lets it cast.
        /// </summary>
        IReadOnlyCollection<ISpell> LearnedSpells { get; }

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
