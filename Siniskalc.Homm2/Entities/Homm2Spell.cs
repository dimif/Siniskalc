namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// A spell of Heroes of Might and Magic II. The code is the number of the spell in the game (1 to 65): the spells of a hero's
    /// spell book are one byte each at that number, and a Mage Guild lists its spells as the number minus one.
    /// Names and spell point costs come from the spell tables of the open source engine fheroes2 and are checked against the
    /// savegames for Cure, Steelskin, Magic Arrow, Disrupting Ray, View Resources and View Artifacts.
    /// </summary>
    public sealed class Homm2Spell : Homm2Enumeration
    {
        public static readonly Homm2Spell Fireball = new("Fireball", 1, 9);
        public static readonly Homm2Spell Fireblast = new("Fireblast", 2, 15);
        public static readonly Homm2Spell LightningBolt = new("Lightning Bolt", 3, 7);
        public static readonly Homm2Spell ChainLightning = new("Chain Lightning", 4, 15);
        public static readonly Homm2Spell Teleport = new("Teleport", 5, 9);
        public static readonly Homm2Spell Cure = new("Cure", 6, 6);
        public static readonly Homm2Spell MassCure = new("Mass Cure", 7, 15);
        public static readonly Homm2Spell Resurrect = new("Resurrect", 8, 12);
        public static readonly Homm2Spell ResurrectTrue = new("Resurrect True", 9, 15);
        public static readonly Homm2Spell Haste = new("Haste", 10, 3);
        public static readonly Homm2Spell MassHaste = new("Mass Haste", 11, 10);
        public static readonly Homm2Spell Slow = new("Slow", 12, 3);
        public static readonly Homm2Spell MassSlow = new("Mass Slow", 13, 15);
        public static readonly Homm2Spell Blind = new("Blind", 14, 6);
        public static readonly Homm2Spell Bless = new("Bless", 15, 3);
        public static readonly Homm2Spell MassBless = new("Mass Bless", 16, 12);
        public static readonly Homm2Spell Stoneskin = new("Stoneskin", 17, 3);
        public static readonly Homm2Spell Steelskin = new("Steelskin", 18, 6);
        public static readonly Homm2Spell Curse = new("Curse", 19, 3);
        public static readonly Homm2Spell MassCurse = new("Mass Curse", 20, 12);
        public static readonly Homm2Spell HolyWord = new("Holy Word", 21, 9);
        public static readonly Homm2Spell HolyShout = new("Holy Shout", 22, 12);
        public static readonly Homm2Spell AntiMagic = new("Anti-Magic", 23, 7);
        public static readonly Homm2Spell DispelMagic = new("Dispel Magic", 24, 5);
        public static readonly Homm2Spell MassDispel = new("Mass Dispel", 25, 12);
        public static readonly Homm2Spell MagicArrow = new("Magic Arrow", 26, 3);
        public static readonly Homm2Spell Berserker = new("Berserker", 27, 12);
        public static readonly Homm2Spell Armageddon = new("Armageddon", 28, 20);
        public static readonly Homm2Spell ElementalStorm = new("Elemental Storm", 29, 15);
        public static readonly Homm2Spell MeteorShower = new("Meteor Shower", 30, 15);
        public static readonly Homm2Spell Paralyze = new("Paralyze", 31, 9);
        public static readonly Homm2Spell Hypnotize = new("Hypnotize", 32, 15);
        public static readonly Homm2Spell ColdRay = new("Cold Ray", 33, 6);
        public static readonly Homm2Spell ColdRing = new("Cold Ring", 34, 9);
        public static readonly Homm2Spell DisruptingRay = new("Disrupting Ray", 35, 7);
        public static readonly Homm2Spell DeathRipple = new("Death Ripple", 36, 6);
        public static readonly Homm2Spell DeathWave = new("Death Wave", 37, 10);
        public static readonly Homm2Spell DragonSlayer = new("Dragon Slayer", 38, 6);
        public static readonly Homm2Spell BloodLust = new("Blood Lust", 39, 3);
        public static readonly Homm2Spell AnimateDead = new("Animate Dead", 40, 10);
        public static readonly Homm2Spell MirrorImage = new("Mirror Image", 41, 25);
        public static readonly Homm2Spell Shield = new("Shield", 42, 3);
        public static readonly Homm2Spell MassShield = new("Mass Shield", 43, 7);
        public static readonly Homm2Spell SummonEarthElemental = new("Summon Earth Elemental", 44, 30);
        public static readonly Homm2Spell SummonAirElemental = new("Summon Air Elemental", 45, 30);
        public static readonly Homm2Spell SummonFireElemental = new("Summon Fire Elemental", 46, 30);
        public static readonly Homm2Spell SummonWaterElemental = new("Summon Water Elemental", 47, 30);
        public static readonly Homm2Spell Earthquake = new("Earthquake", 48, 15);
        public static readonly Homm2Spell ViewMines = new("View Mines", 49, 1);
        public static readonly Homm2Spell ViewResources = new("View Resources", 50, 1);
        public static readonly Homm2Spell ViewArtifacts = new("View Artifacts", 51, 2);
        public static readonly Homm2Spell ViewTowns = new("View Towns", 52, 2);
        public static readonly Homm2Spell ViewHeroes = new("View Heroes", 53, 2);
        public static readonly Homm2Spell ViewAll = new("View All", 54, 3);
        public static readonly Homm2Spell IdentifyHero = new("Identify Hero", 55, 3);
        public static readonly Homm2Spell SummonBoat = new("Summon Boat", 56, 5);
        public static readonly Homm2Spell DimensionDoor = new("Dimension Door", 57, 10);
        public static readonly Homm2Spell TownGate = new("Town Gate", 58, 10);
        public static readonly Homm2Spell TownPortal = new("Town Portal", 59, 20);
        public static readonly Homm2Spell Visions = new("Visions", 60, 6);
        public static readonly Homm2Spell Haunt = new("Haunt", 61, 8);
        public static readonly Homm2Spell SetEarthGuardian = new("Set Earth Guardian", 62, 15);
        public static readonly Homm2Spell SetAirGuardian = new("Set Air Guardian", 63, 15);
        public static readonly Homm2Spell SetFireGuardian = new("Set Fire Guardian", 64, 15);
        public static readonly Homm2Spell SetWaterGuardian = new("Set Water Guardian", 65, 15);

        private Homm2Spell(string name, byte code, int spellPoints)
            : base(name, code)
        {
            this.SpellPoints = spellPoints;
        }

        /// <summary>
        /// Gets how many spell points casting the spell costs.
        /// </summary>
        public int SpellPoints { get; }

        public static IReadOnlyList<Homm2Spell> All => GetAll<Homm2Spell>();

        public static Homm2Spell? FromByte(byte code) => FromCode<Homm2Spell>(code);

        public static string NameOf(byte code) => FromByte(code)?.Name ?? $"Unknown spell {code}";
    }
}
