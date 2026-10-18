namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// How the savegames of Heroes of Might and Magic II number the creatures: zero-based, in the order of <see cref="Creature"/>.
    /// A number that is no creature reads as the Peasant.
    /// </summary>
    public sealed class CreatureCodes : ICreatureCodes<Creature>
    {
        public static CreatureCodes Instance { get; } = new();

        /// <inheritdoc/>
        public Creature Fallback => Creature.Peasant;

        /// <inheritdoc/>
        public Creature? Find(uint code) => code > byte.MaxValue ? null : Creature.FromByte((byte)code);
    }
}
