namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The six towns (and the classes of the heroes that belong to them), in the order of the savegame: 0 for a Knight.
    /// </summary>
    public sealed class Faction : Homm2Enumeration, IFaction
    {
        public static readonly Faction Knight = new("Knight", 0);
        public static readonly Faction Barbarian = new("Barbarian", 1);
        public static readonly Faction Sorceress = new("Sorceress", 2);
        public static readonly Faction Warlock = new("Warlock", 3);
        public static readonly Faction Wizard = new("Wizard", 4);
        public static readonly Faction Necromancer = new("Necromancer", 5);

        private Faction(string name, byte code)
            : base(name, code)
        {
        }

        public static IReadOnlyList<Faction> All => GetAll<Faction>();

        public static Faction? FromByte(byte code) => FromCode<Faction>(code);
    }
}
