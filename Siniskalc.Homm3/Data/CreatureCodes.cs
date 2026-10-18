namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// How the savefiles of Heroes of Might and Magic III number the creatures. A number that is no creature the editor knows reads as
    /// the first creature of the list.
    /// </summary>
    public sealed class CreatureCodes : ICreatureCodes<Creature>
    {
        public static CreatureCodes Instance { get; } = new();

        /// <inheritdoc/>
        public Creature Fallback => Creature.All[0];

        /// <inheritdoc/>
        public Creature? Find(uint code) => code > byte.MaxValue ? null : Creature.FromByte((byte)code);
    }
}
