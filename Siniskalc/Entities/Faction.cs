namespace Siniskalc.Entities
{
    using System.Collections.Generic;

    public sealed class Faction : Homm3Enumeration
    {
        public static readonly Faction Neutral = new("Neutral", 99);
        public static readonly Faction Castle = new("Castle", 0x00);
        public static readonly Faction Rampart = new("Rampart", 0x01);
        public static readonly Faction Tower = new("Tower", 0x02);
        public static readonly Faction Dungeon = new("Dungeon", 0x05);
        public static readonly Faction Inferno = new("Inferno", 0x03);
        public static readonly Faction Necropolis = new("Necropolis", 0x04);
        public static readonly Faction Stronghold = new("Stronghold", 0x06);
        public static readonly Faction Fortress = new("Fortress", 0x07);
        public static readonly Faction Conflux = new("Conflux", 0x08, GameVersion.ArmageddonsBlade);
        public static readonly Faction Cove = new("Cove", 0x09, GameVersion.HornOfTheAbyss);
        public static readonly Faction Factory = new("Factory", 0x0A, GameVersion.HornOfTheAbyss);
        public static readonly Faction Bulwark = new("Bulwark", 0x0B, GameVersion.HornOfTheAbyss);

        private Faction(string name, byte code, GameVersion gameVersion)
            : base(name, code, gameVersion)
        {
        }

        private Faction(string name, byte code)
            : base(name, code)
        {
        }

        public static IReadOnlyList<Faction> All => GetAll<Faction>();
    }
}
