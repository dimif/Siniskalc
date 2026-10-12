namespace Siniskalc.Entities
{
    using System.Collections.Generic;

    public sealed class Spell : Homm3Enumeration
    {
        public static readonly Spell SummonBoat = new("Summon Boat", 0, SpellSchool.Water, 1, SpellType.Adventure);
        public static readonly Spell ScuttleBoat = new("Scuttle Boat", 1, SpellSchool.Water, 2, SpellType.Adventure);
        public static readonly Spell Visions = new("Visions", 2, SpellSchool.All, 2, SpellType.Adventure);
        public static readonly Spell ViewEarth = new("View Earth", 3, SpellSchool.Earth, 1, SpellType.Adventure);
        public static readonly Spell Disguise = new("Disguise", 4, SpellSchool.Wind, 2, SpellType.Adventure);
        public static readonly Spell ViewAir = new("View Air", 5, SpellSchool.Wind, 1, SpellType.Adventure);
        public static readonly Spell Fly = new("Fly", 6, SpellSchool.Wind, 5, SpellType.Adventure);
        public static readonly Spell WaterWalk = new("Water Walk", 7, SpellSchool.Water, 4, SpellType.Adventure);
        public static readonly Spell DimensionDoor = new("Dimension Door", 8, SpellSchool.Wind, 5, SpellType.Adventure);
        public static readonly Spell TownPortal = new("Town Portal", 9, SpellSchool.Earth, 4, SpellType.Adventure);
        public static readonly Spell QuickSand = new("Quick Sand", 10, SpellSchool.Earth, 2, SpellType.Combat);
        public static readonly Spell LandMine = new("Land Mine", 11, SpellSchool.Fire, 3, SpellType.Combat);
        public static readonly Spell ForceField = new("Force Field", 12, SpellSchool.Earth, 3, SpellType.Combat);
        public static readonly Spell FireWall = new("Fire Wall", 13, SpellSchool.Fire, 2, SpellType.Combat);
        public static readonly Spell Earthquake = new("Earthquake", 14, SpellSchool.Earth, 3, SpellType.Combat);
        public static readonly Spell MagicArrow = new("Magic Arrow", 15, SpellSchool.All, 1, SpellType.Combat);
        public static readonly Spell IceBolt = new("Ice Bolt", 16, SpellSchool.Water, 2, SpellType.Combat);
        public static readonly Spell LightningBolt = new("Lightning Bolt", 17, SpellSchool.Wind, 2, SpellType.Combat);
        public static readonly Spell Implosion = new("Implosion", 18, SpellSchool.Earth, 5, SpellType.Combat);
        public static readonly Spell ChainLightning = new("Chain Lightning", 19, SpellSchool.Wind, 4, SpellType.Combat);
        public static readonly Spell FrostRing = new("Frost Ring", 20, SpellSchool.Water, 3, SpellType.Combat);
        public static readonly Spell Fireball = new("Fireball", 21, SpellSchool.Fire, 3, SpellType.Combat);
        public static readonly Spell Inferno = new("Inferno", 22, SpellSchool.Fire, 4, SpellType.Combat);
        public static readonly Spell MeteorShower = new("Meteor Shower", 23, SpellSchool.Earth, 4, SpellType.Combat);
        public static readonly Spell DeathRipple = new("Death Ripple", 24, SpellSchool.Earth, 2, SpellType.Combat);
        public static readonly Spell DestroyUndead = new("Destroy Undead", 25, SpellSchool.Wind, 3, SpellType.Combat);
        public static readonly Spell Armageddon = new("Armageddon", 26, SpellSchool.Fire, 4, SpellType.Combat);
        public static readonly Spell Shield = new("Shield", 27, SpellSchool.Earth, 1, SpellType.Combat);
        public static readonly Spell AirShield = new("Air Shield", 28, SpellSchool.Wind, 3, SpellType.Combat);
        public static readonly Spell FireShield = new("Fire Shield", 29, SpellSchool.Fire, 4, SpellType.Combat);
        public static readonly Spell ProtectionFromAir = new("Protection from Air", 30, SpellSchool.Wind, 2, SpellType.Combat);
        public static readonly Spell ProtectionFromFire = new("Protection from Fire", 31, SpellSchool.Fire, 1, SpellType.Combat);
        public static readonly Spell ProtectionFromWater = new("Protection from Water", 32, SpellSchool.Water, 1, SpellType.Combat);
        public static readonly Spell ProtectionFromEarth = new("Protection from Earth", 33, SpellSchool.Earth, 3, SpellType.Combat);
        public static readonly Spell AntiMagic = new("Anti Magic", 34, SpellSchool.Earth, 3, SpellType.Combat);
        public static readonly Spell Dispel = new("Dispel", 35, SpellSchool.Water, 1, SpellType.Combat);
        public static readonly Spell MagicMirror = new("Magic Mirror", 36, SpellSchool.Wind, 5, SpellType.Combat);
        public static readonly Spell Cure = new("Cure", 37, SpellSchool.Water, 1, SpellType.Combat);
        public static readonly Spell Resurrection = new("Resurrection", 38, SpellSchool.Earth, 4, SpellType.Combat);
        public static readonly Spell AnimateDead = new("Animate Dead", 39, SpellSchool.Earth, 3, SpellType.Combat);
        public static readonly Spell Sacrifice = new("Sacrifice", 40, SpellSchool.Fire, 5, SpellType.Combat);
        public static readonly Spell Bless = new("Bless", 41, SpellSchool.Water, 1, SpellType.Combat);
        public static readonly Spell Curse = new("Curse", 42, SpellSchool.Fire, 1, SpellType.Combat);
        public static readonly Spell Bloodlust = new("Bloodlust", 43, SpellSchool.Fire, 1, SpellType.Combat);
        public static readonly Spell Precision = new("Precision", 44, SpellSchool.Wind, 2, SpellType.Combat);
        public static readonly Spell Weakness = new("Weakness", 45, SpellSchool.Water, 2, SpellType.Combat);
        public static readonly Spell StoneSkin = new("Stone Skin", 46, SpellSchool.Earth, 1, SpellType.Combat);
        public static readonly Spell DisruptingRay = new("Disrupting Ray", 47, SpellSchool.Wind, 2, SpellType.Combat);
        public static readonly Spell Prayer = new("Prayer", 48, SpellSchool.Water, 4, SpellType.Combat);
        public static readonly Spell Mirth = new("Mirth", 49, SpellSchool.Water, 3, SpellType.Combat);
        public static readonly Spell Sorrow = new("Sorrow", 50, SpellSchool.Earth, 4, SpellType.Combat);
        public static readonly Spell Fortune = new("Fortune", 51, SpellSchool.Wind, 2, SpellType.Combat);
        public static readonly Spell Misfortune = new("Misfortune", 52, SpellSchool.Fire, 3, SpellType.Combat);
        public static readonly Spell Haste = new("Haste", 53, SpellSchool.Wind, 1, SpellType.Combat);
        public static readonly Spell Slow = new("Slow", 54, SpellSchool.Earth, 1, SpellType.Combat);
        public static readonly Spell Slayer = new("Slayer", 55, SpellSchool.Fire, 4, SpellType.Combat);
        public static readonly Spell Frenzy = new("Frenzy", 56, SpellSchool.Fire, 4, SpellType.Combat);
        public static readonly Spell TitansLightningBolt = new("Titan's Lightning Bolt", 57, SpellSchool.Wind, 5, SpellType.Combat);
        public static readonly Spell Counterstrike = new("Counterstrike", 58, SpellSchool.Wind, 4, SpellType.Combat);
        public static readonly Spell Berserk = new("Berserk", 59, SpellSchool.Fire, 4, SpellType.Combat);
        public static readonly Spell Hypnotize = new("Hypnotize", 60, SpellSchool.Wind, 3, SpellType.Combat);
        public static readonly Spell Forgetfulness = new("Forgetfulness", 61, SpellSchool.Water, 3, SpellType.Combat);
        public static readonly Spell Blind = new("Blind", 62, SpellSchool.Fire, 2, SpellType.Combat);
        public static readonly Spell Teleport = new("Teleport", 63, SpellSchool.Water, 3, SpellType.Combat);
        public static readonly Spell RemoveObstacle = new("Remove Obstacle", 64, SpellSchool.Water, 2, SpellType.Combat);
        public static readonly Spell Clone = new("Clone", 65, SpellSchool.Water, 4, SpellType.Combat);
        public static readonly Spell SummonFireElemental = new("Summon Fire Elemental", 66, SpellSchool.Fire, 5, SpellType.Combat);
        public static readonly Spell SummonEarthElemental = new("Summon Earth Elemental", 67, SpellSchool.Earth, 5, SpellType.Combat);
        public static readonly Spell SummonWaterElemental = new("Summon Water Elemental", 68, SpellSchool.Water, 5, SpellType.Combat);
        public static readonly Spell SummonAirElemental = new("Summon Air Elemental", 69, SpellSchool.Wind, 5, SpellType.Combat);

        private Spell(string name, byte code, SpellSchool school, int level, SpellType type)
            : this(name, code, GameVersion.RestorationOfErathia, school, level, type)
        {
        }

        private Spell(string name, byte code, GameVersion gameVersion, SpellSchool school, int level, SpellType type)
            : base(name, code, gameVersion)
        {
            this.School = school;
            this.Level = level;
            this.Type = type;
        }

        public static IReadOnlyList<Spell> All => GetAll<Spell>();

        public SpellSchool School { get; }

        public int Level { get; }

        public SpellType Type { get; }

        /// <summary>
        /// Gets whether a Mage Guild can hold the spell. Titan's Lightning Bolt belongs to the Titan only.
        /// </summary>
        public bool CanBeInMageGuild => this != TitansLightningBolt;
    }
}
