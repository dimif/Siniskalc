namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
    using Siniskalc.Core.Entities;

    /// <summary>
    /// The army of a hero or the garrison of a castle in any game: its slots, each a stack of creatures or empty. An army is immutable:
    /// the methods that change it return a new army.
    /// </summary>
    /// <typeparam name="TCreature">The creature of the game.</typeparam>
    public interface IArmy<TCreature>
        where TCreature : class, IGameEntity
    {
        /// <summary>
        /// Gets the slots in order, <c>null</c> for an empty one.
        /// </summary>
        IReadOnlyList<ArmySlot<TCreature>?> Slots { get; }

        /// <summary>
        /// Gets the stacks the army holds, without the empty slots.
        /// </summary>
        IEnumerable<ArmySlot<TCreature>> Filled { get; }

        /// <summary>
        /// Returns the army with the slot at <paramref name="index"/> replaced, by <c>null</c> to empty it.
        /// </summary>
        IArmy<TCreature> WithSlot(int index, ArmySlot<TCreature>? slot);

        /// <summary>
        /// Returns the army with a stack of <paramref name="count"/> of <paramref name="creature"/> in the slot at <paramref name="index"/>.
        /// </summary>
        IArmy<TCreature> WithSlot(int index, TCreature creature, long count);

        /// <summary>
        /// Returns the army with the slot at <paramref name="index"/> emptied.
        /// </summary>
        IArmy<TCreature> WithoutSlot(int index);

        /// <summary>
        /// Returns the army with the contents of the two slots swapped. Swapping with an empty slot moves the stack.
        /// </summary>
        IArmy<TCreature> Swap(int index, int otherIndex);
    }
}
