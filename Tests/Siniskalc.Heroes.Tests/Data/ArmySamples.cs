namespace Siniskalc.Heroes.Tests.Data
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Data;

    /// <summary>
    /// A made-up creature and a made-up way to number them, so the army tests need no real game.
    /// </summary>
    internal static class ArmySamples
    {
        public static readonly Creature Peasant = new("Peasant", 0);

        public static readonly Creature Archer = new("Archer", 1);

        public static readonly Codes Numbering = new();

        /// <summary>Types of one byte and counts of two bytes, five slots (the shape of HoMM2).</summary>
        public static readonly ArmyLayout Narrow = new(0, 1, 5, 2, 5, 1000);

        /// <summary>Types and counts of four bytes, three slots (the shape of HoMM3).</summary>
        public static readonly ArmyLayout Wide = new(0, 4, 12, 4, 3, uint.MaxValue);

        internal sealed record Creature(string Name, byte Code) : IGameEntity;

        internal sealed class Codes : ICreatureCodes<Creature>
        {
            public Creature Fallback => Peasant;

            public Creature? Find(uint code) => code switch { 0 => Peasant, 1 => Archer, _ => null };
        }
    }
}
