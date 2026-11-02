namespace Siniskalc.Heroes.Data
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// How a game turns the number a savefile holds for a creature into the creature.
    /// </summary>
    /// <typeparam name="TCreature">The creature of the game.</typeparam>
    public interface ICreatureCodes<out TCreature>
        where TCreature : class, IGameEntity
    {
        /// <summary>
        /// Gets the creature a number that is no creature reads as: the first known one. The bytes are kept until the model says otherwise.
        /// </summary>
        TCreature Fallback { get; }

        /// <summary>
        /// Gets the creature with the given number, or <c>null</c> when the editor does not know it.
        /// </summary>
        TCreature? Find(uint code);
    }
}
