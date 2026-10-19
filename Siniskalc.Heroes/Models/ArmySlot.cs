namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// A stack of creatures in the army of a hero or the garrison of a castle.
    /// </summary>
    /// <typeparam name="TCreature">The creature of the game.</typeparam>
    /// <param name="Creature">The creature of the stack.</param>
    /// <param name="Count">How many of them the stack holds.</param>
    public sealed record ArmySlot<TCreature>(TCreature Creature, long Count)
        where TCreature : class, IGameEntity
    {
        /// <inheritdoc/>
        public override string ToString() => $"{this.Creature.Name} × {this.Count:N0}";
    }
}
