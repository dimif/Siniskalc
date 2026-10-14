namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A stack of creatures in the army of a hero or the garrison of a castle.
    /// </summary>
    public sealed record ArmySlot(Creature Creature, int Count)
    {
        public override string ToString() => $"{this.Creature.Name} × {this.Count:N0}";
    }
}
