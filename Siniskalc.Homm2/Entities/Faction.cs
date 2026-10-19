namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using System.Linq;
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
        public static readonly Faction Neutral = new("Neutral", 99);

        private Faction(string name, byte code)
            : base(name, code)
        {
        }

        /// <summary>
        /// Gets the six towns, which leaves out <see cref="Neutral"/>.
        /// </summary>
        public static IReadOnlyList<Faction> All => GetAll<Faction>().Where(f => f != Neutral).ToList();

        /// <summary>
        /// Returns the town of a byte of a savefile; a byte that is no town reads as <see cref="Neutral"/>.
        /// </summary>
        public static Faction FromByte(byte code) => FromCode<Faction>(code) ?? Neutral;
    }
}
