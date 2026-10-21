namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// A made-up creature of a made-up game, so the tests need no real game.
    /// </summary>
    internal sealed class TestCreature : AbstractEnumeration, ICreature
    {
        public static readonly TestCreature Peasant = new("Peasant", 0, null);

        public static readonly TestCreature Archer = new("Archer", 1, null);

        public static readonly TestCreature Marksman = new("Marksman", 2, 1);

        private TestCreature(string name, byte code, byte? baseCreatureCode)
            : base(name, code)
        {
            this.BaseCreatureCode = baseCreatureCode;
        }

        public int Tier => 1;

        public IFaction? Faction => null;

        public bool IsUpgrade => this.BaseCreatureCode != null;

        public byte? BaseCreatureCode { get; }
    }
}
