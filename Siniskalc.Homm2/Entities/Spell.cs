namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// A spell of Heroes of Might and Magic II. The code is the number of the spell in the game (1 to 65): the spells of a hero's
    /// spell book are one byte each at that number, and a Mage Guild lists its spells as the number minus one.
    /// Names and spell point costs come from the spell tables of the open source engine fheroes2 and are checked against the
    /// savegames for Cure, Steelskin, Magic Arrow, Disrupting Ray, View Resources and View Artifacts.
    /// </summary>
    public sealed class Spell : Homm2Enumeration
    {
        public static readonly Spell Fireball = new("Fireball", 1, 9, SpellType.Combat);
        public static readonly Spell Fireblast = new("Fireblast", 2, 15, SpellType.Combat);
        public static readonly Spell LightningBolt = new("Lightning Bolt", 3, 7, SpellType.Combat);
        public static readonly Spell ChainLightning = new("Chain Lightning", 4, 15, SpellType.Combat);
        public static readonly Spell Teleport = new("Teleport", 5, 9, SpellType.Combat);
        public static readonly Spell Cure = new("Cure", 6, 6, SpellType.Combat);
        public static readonly Spell MassCure = new("Mass Cure", 7, 15, SpellType.Combat);
        public static readonly Spell Resurrect = new("Resurrect", 8, 12, SpellType.Combat);
        public static readonly Spell ResurrectTrue = new("Resurrect True", 9, 15, SpellType.Combat);
        public static readonly Spell Haste = new("Haste", 10, 3, SpellType.Combat);
        public static readonly Spell MassHaste = new("Mass Haste", 11, 10, SpellType.Combat);
        public static readonly Spell Slow = new("Slow", 12, 3, SpellType.Combat);
        public static readonly Spell MassSlow = new("Mass Slow", 13, 15, SpellType.Combat);
        public static readonly Spell Blind = new("Blind", 14, 6, SpellType.Combat);
        public static readonly Spell Bless = new("Bless", 15, 3, SpellType.Combat);
        public static readonly Spell MassBless = new("Mass Bless", 16, 12, SpellType.Combat);
        public static readonly Spell Stoneskin = new("Stoneskin", 17, 3, SpellType.Combat);
        public static readonly Spell Steelskin = new("Steelskin", 18, 6, SpellType.Combat);
        public static readonly Spell Curse = new("Curse", 19, 3, SpellType.Combat);
        public static readonly Spell MassCurse = new("Mass Curse", 20, 12, SpellType.Combat);
        public static readonly Spell HolyWord = new("Holy Word", 21, 9, SpellType.Combat);
        public static readonly Spell HolyShout = new("Holy Shout", 22, 12, SpellType.Combat);
        public static readonly Spell AntiMagic = new("Anti-Magic", 23, 7, SpellType.Combat);
        public static readonly Spell DispelMagic = new("Dispel Magic", 24, 5, SpellType.Combat);
        public static readonly Spell MassDispel = new("Mass Dispel", 25, 12, SpellType.Combat);
        public static readonly Spell MagicArrow = new("Magic Arrow", 26, 3, SpellType.Combat);
        public static readonly Spell Berserker = new("Berserker", 27, 12, SpellType.Combat);
        public static readonly Spell Armageddon = new("Armageddon", 28, 20, SpellType.Combat);
        public static readonly Spell ElementalStorm = new("Elemental Storm", 29, 15, SpellType.Combat);
        public static readonly Spell MeteorShower = new("Meteor Shower", 30, 15, SpellType.Combat);
        public static readonly Spell Paralyze = new("Paralyze", 31, 9, SpellType.Combat);
        public static readonly Spell Hypnotize = new("Hypnotize", 32, 15, SpellType.Combat);
        public static readonly Spell ColdRay = new("Cold Ray", 33, 6, SpellType.Combat);
        public static readonly Spell ColdRing = new("Cold Ring", 34, 9, SpellType.Combat);
        public static readonly Spell DisruptingRay = new("Disrupting Ray", 35, 7, SpellType.Combat);
        public static readonly Spell DeathRipple = new("Death Ripple", 36, 6, SpellType.Combat);
        public static readonly Spell DeathWave = new("Death Wave", 37, 10, SpellType.Combat);
        public static readonly Spell DragonSlayer = new("Dragon Slayer", 38, 6, SpellType.Combat);
        public static readonly Spell BloodLust = new("Blood Lust", 39, 3, SpellType.Combat);
        public static readonly Spell AnimateDead = new("Animate Dead", 40, 10, SpellType.Combat);
        public static readonly Spell MirrorImage = new("Mirror Image", 41, 25, SpellType.Combat);
        public static readonly Spell Shield = new("Shield", 42, 3, SpellType.Combat);
        public static readonly Spell MassShield = new("Mass Shield", 43, 7, SpellType.Combat);
        public static readonly Spell SummonEarthElemental = new("Summon Earth Elemental", 44, 30, SpellType.Combat);
        public static readonly Spell SummonAirElemental = new("Summon Air Elemental", 45, 30, SpellType.Combat);
        public static readonly Spell SummonFireElemental = new("Summon Fire Elemental", 46, 30, SpellType.Combat);
        public static readonly Spell SummonWaterElemental = new("Summon Water Elemental", 47, 30, SpellType.Combat);
        public static readonly Spell Earthquake = new("Earthquake", 48, 15, SpellType.Combat);
        public static readonly Spell ViewMines = new("View Mines", 49, 1, SpellType.Adventure);
        public static readonly Spell ViewResources = new("View Resources", 50, 1, SpellType.Adventure);
        public static readonly Spell ViewArtifacts = new("View Artifacts", 51, 2, SpellType.Adventure);
        public static readonly Spell ViewTowns = new("View Towns", 52, 2, SpellType.Adventure);
        public static readonly Spell ViewHeroes = new("View Heroes", 53, 2, SpellType.Adventure);
        public static readonly Spell ViewAll = new("View All", 54, 3, SpellType.Adventure);
        public static readonly Spell IdentifyHero = new("Identify Hero", 55, 3, SpellType.Adventure);
        public static readonly Spell SummonBoat = new("Summon Boat", 56, 5, SpellType.Adventure);
        public static readonly Spell DimensionDoor = new("Dimension Door", 57, 10, SpellType.Adventure);
        public static readonly Spell TownGate = new("Town Gate", 58, 10, SpellType.Adventure);
        public static readonly Spell TownPortal = new("Town Portal", 59, 20, SpellType.Adventure);
        public static readonly Spell Visions = new("Visions", 60, 6, SpellType.Adventure);
        public static readonly Spell Haunt = new("Haunt", 61, 8, SpellType.Adventure);
        public static readonly Spell SetEarthGuardian = new("Set Earth Guardian", 62, 15, SpellType.Adventure);
        public static readonly Spell SetAirGuardian = new("Set Air Guardian", 63, 15, SpellType.Adventure);
        public static readonly Spell SetFireGuardian = new("Set Fire Guardian", 64, 15, SpellType.Adventure);
        public static readonly Spell SetWaterGuardian = new("Set Water Guardian", 65, 15, SpellType.Adventure);

        private Spell(string name, byte code, int spellPoints, SpellType type)
            : base(name, code)
        {
            this.SpellPoints = spellPoints;
            this.Type = type;
        }

        /// <summary>
        /// Gets how many spell points casting the spell costs.
        /// </summary>
        public int SpellPoints { get; }

        /// <summary>
        /// Gets where the spell is cast. The spells 1 to 48 (Fireball to Earthquake) are cast in battle, 49 to 65 (View Mines to Set Water Guardian) on the adventure map.
        /// </summary>
        public SpellType Type { get; }

        public static IReadOnlyList<Spell> All => GetAll<Spell>();

        public static Spell? FromByte(byte code) => FromCode<Spell>(code);

        public static string NameOf(byte code) => FromByte(code)?.Name ?? $"Unknown spell {code}";
    }
}
