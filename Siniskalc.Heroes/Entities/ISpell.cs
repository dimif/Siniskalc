namespace Siniskalc.Heroes.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// A spell of a game, as much of it as the pages that list spells need. The cost and the school are each game's own.
    /// </summary>
    public interface ISpell : IGameEntity
    {
        /// <summary>
        /// Gets where the spell is cast.
        /// </summary>
        SpellType Type { get; }

        /// <summary>
        /// Gets the level of the spell, 1 to 5, which is the level of the Mage Guild that offers it.
        /// </summary>
        int Level { get; }
    }
}
