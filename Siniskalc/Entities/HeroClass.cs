namespace Siniskalc.Entities
{
    using System.Collections.Generic;

    public sealed class HeroClass : Homm3Enumeration
    {
        public static readonly HeroClass Knight = new("Knight", 0x00, HeroClassType.Might, Faction.Castle);
        public static readonly HeroClass Cleric = new("Cleric", 0x01, HeroClassType.Magic, Faction.Castle);
        public static readonly HeroClass Ranger = new("Ranger", 0x02, HeroClassType.Might, Faction.Rampart);
        public static readonly HeroClass Druid = new("Druid", 0x03, HeroClassType.Magic, Faction.Rampart);
        public static readonly HeroClass Alchemist = new("Alchemist", 0x04, HeroClassType.Might, Faction.Tower);
        public static readonly HeroClass Wizard = new("Wizard", 0x05, HeroClassType.Magic, Faction.Tower);
        public static readonly HeroClass Demoniac = new("Demoniac", 0x06, HeroClassType.Might, Faction.Inferno);
        public static readonly HeroClass Heretic = new("Heretic", 0x07, HeroClassType.Magic, Faction.Inferno);
        public static readonly HeroClass DeathKnight = new("Death Knight", 0x08, HeroClassType.Might, Faction.Necropolis);
        public static readonly HeroClass Necromancer = new("Necromancer", 0x09, HeroClassType.Magic, Faction.Necropolis);
        public static readonly HeroClass Overlord = new("Overlord", 0x0A, HeroClassType.Might, Faction.Dungeon);
        public static readonly HeroClass Warlock = new("Warlock", 0x0B, HeroClassType.Magic, Faction.Dungeon);
        public static readonly HeroClass Barbarian = new("Barbarian", 0x0C, HeroClassType.Might, Faction.Stronghold);
        public static readonly HeroClass BattleMage = new("Battle Mage", 0x0D, HeroClassType.Magic, Faction.Stronghold);
        public static readonly HeroClass Beastmaster = new("Beastmaster", 0x0E, HeroClassType.Might, Faction.Fortress);
        public static readonly HeroClass Witch = new("Witch", 0x0F, HeroClassType.Magic, Faction.Fortress);
        public static readonly HeroClass Planeswalker = new("Planeswalker", 0x10, HeroClassType.Might, GameVersion.ArmageddonsBlade, Faction.Conflux);
        public static readonly HeroClass Elementalist = new("Elementalist", 0x11, HeroClassType.Magic, GameVersion.ArmageddonsBlade, Faction.Conflux);
        public static readonly HeroClass Captain = new("Captain", 0x12, HeroClassType.Might, GameVersion.HornOfTheAbyss, Faction.Cove);
        public static readonly HeroClass Navigator = new("Navigator", 0x13, HeroClassType.Magic, GameVersion.HornOfTheAbyss, Faction.Cove);
        public static readonly HeroClass Mercenary = new("Mercenary", 0x14, HeroClassType.Might, GameVersion.HornOfTheAbyss, Faction.Factory);
        public static readonly HeroClass Artificier = new("Artificier", 0x15, HeroClassType.Magic, GameVersion.HornOfTheAbyss, Faction.Factory);
        public static readonly HeroClass Chieftain = new("Chieftain", 0x16, HeroClassType.Might, GameVersion.HornOfTheAbyss, Faction.Bulwark);
        public static readonly HeroClass Elder = new("Elder", 0x17, HeroClassType.Magic, GameVersion.HornOfTheAbyss, Faction.Bulwark);

        private HeroClass(string name, byte code, HeroClassType type, GameVersion gameVersion, Faction faction)
            : base(name, code, gameVersion)
        {
            this.Type = type;
            this.Faction = faction;
        }

        private HeroClass(string name, byte code, HeroClassType type, Faction faction)
            : this(name, code, type, GameVersion.RestorationOfErathia, faction)
        {
        }

        public static IReadOnlyList<HeroClass> All => GetAll<HeroClass>();

        public Faction Faction { get; }

        public HeroClassType Type { get; }

        /// <summary>
        /// Returns the class with this code. A code this editor does not know (a class of a newer game version) reads as
        /// <see cref="Knight"/>, the first known class, so the hero still loads; the serializer leaves the byte alone while the hero stays a Knight.
        /// </summary>
        public static HeroClass FromByte(byte value) => FromCode<HeroClass>(value) ?? Knight;
    }
}
