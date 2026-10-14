namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using FactionKind = Siniskalc.Homm2.Entities.Faction;

    /// <summary>
    /// A creature of Heroes of Might and Magic II. The code is the number the game keeps in an army slot (0xFF is an empty slot): the
    /// Peasant is 0 and the Black Dragon is 37. The faction is the town the creature is recruited in, or <c>null</c> for the neutral ones.
    /// Names, levels and weekly growth come from the creature tables of the open source engine fheroes2 and are checked against the
    /// savegames for the Peasant, the Archer and the Black Dragon and the weekly growth of a Knight castle (12 and 8, 26 and 18 with a well).
    /// </summary>
    public sealed class Creature : Homm2Enumeration
    {
        public static readonly Creature Peasant = new("Peasant", 0, FactionKind.Knight, 1, 12);
        public static readonly Creature Archer = new("Archer", 1, FactionKind.Knight, 2, 8);
        public static readonly Creature Ranger = new("Ranger", 2, FactionKind.Knight, 2, 8);
        public static readonly Creature Pikeman = new("Pikeman", 3, FactionKind.Knight, 3, 5);
        public static readonly Creature VeteranPikeman = new("Veteran Pikeman", 4, FactionKind.Knight, 3, 5);
        public static readonly Creature Swordsman = new("Swordsman", 5, FactionKind.Knight, 4, 4);
        public static readonly Creature MasterSwordsman = new("Master Swordsman", 6, FactionKind.Knight, 4, 4);
        public static readonly Creature Cavalry = new("Cavalry", 7, FactionKind.Knight, 5, 3);
        public static readonly Creature Champion = new("Champion", 8, FactionKind.Knight, 5, 3);
        public static readonly Creature Paladin = new("Paladin", 9, FactionKind.Knight, 6, 2);
        public static readonly Creature Crusader = new("Crusader", 10, FactionKind.Knight, 6, 2);
        public static readonly Creature Goblin = new("Goblin", 11, FactionKind.Barbarian, 1, 10);
        public static readonly Creature Orc = new("Orc", 12, FactionKind.Barbarian, 2, 8);
        public static readonly Creature OrcChief = new("Orc Chief", 13, FactionKind.Barbarian, 2, 8);
        public static readonly Creature Wolf = new("Wolf", 14, FactionKind.Barbarian, 3, 5);
        public static readonly Creature Ogre = new("Ogre", 15, FactionKind.Barbarian, 4, 4);
        public static readonly Creature OgreLord = new("Ogre Lord", 16, FactionKind.Barbarian, 4, 4);
        public static readonly Creature Troll = new("Troll", 17, FactionKind.Barbarian, 5, 3);
        public static readonly Creature WarTroll = new("War Troll", 18, FactionKind.Barbarian, 5, 3);
        public static readonly Creature Cyclops = new("Cyclops", 19, FactionKind.Barbarian, 6, 2);
        public static readonly Creature Sprite = new("Sprite", 20, FactionKind.Sorceress, 1, 8);
        public static readonly Creature Dwarf = new("Dwarf", 21, FactionKind.Sorceress, 2, 6);
        public static readonly Creature BattleDwarf = new("Battle Dwarf", 22, FactionKind.Sorceress, 2, 6);
        public static readonly Creature Elf = new("Elf", 23, FactionKind.Sorceress, 3, 4);
        public static readonly Creature GrandElf = new("Grand Elf", 24, FactionKind.Sorceress, 3, 4);
        public static readonly Creature Druid = new("Druid", 25, FactionKind.Sorceress, 4, 3);
        public static readonly Creature GreaterDruid = new("Greater Druid", 26, FactionKind.Sorceress, 4, 3);
        public static readonly Creature Unicorn = new("Unicorn", 27, FactionKind.Sorceress, 5, 2);
        public static readonly Creature Phoenix = new("Phoenix", 28, FactionKind.Sorceress, 6, 1);
        public static readonly Creature Centaur = new("Centaur", 29, FactionKind.Warlock, 1, 8);
        public static readonly Creature Gargoyle = new("Gargoyle", 30, FactionKind.Warlock, 2, 6);
        public static readonly Creature Griffin = new("Griffin", 31, FactionKind.Warlock, 3, 4);
        public static readonly Creature Minotaur = new("Minotaur", 32, FactionKind.Warlock, 4, 3);
        public static readonly Creature MinotaurKing = new("Minotaur King", 33, FactionKind.Warlock, 4, 3);
        public static readonly Creature Hydra = new("Hydra", 34, FactionKind.Warlock, 5, 2);
        public static readonly Creature GreenDragon = new("Green Dragon", 35, FactionKind.Warlock, 6, 1);
        public static readonly Creature RedDragon = new("Red Dragon", 36, FactionKind.Warlock, 6, 1);
        public static readonly Creature BlackDragon = new("Black Dragon", 37, FactionKind.Warlock, 6, 1);
        public static readonly Creature Halfling = new("Halfling", 38, FactionKind.Wizard, 1, 8);
        public static readonly Creature Boar = new("Boar", 39, FactionKind.Wizard, 2, 6);
        public static readonly Creature IronGolem = new("Iron Golem", 40, FactionKind.Wizard, 3, 4);
        public static readonly Creature SteelGolem = new("Steel Golem", 41, FactionKind.Wizard, 3, 4);
        public static readonly Creature Roc = new("Roc", 42, FactionKind.Wizard, 4, 3);
        public static readonly Creature Mage = new("Mage", 43, FactionKind.Wizard, 5, 2);
        public static readonly Creature Archmage = new("Archmage", 44, FactionKind.Wizard, 5, 2);
        public static readonly Creature Giant = new("Giant", 45, FactionKind.Wizard, 6, 1);
        public static readonly Creature Titan = new("Titan", 46, FactionKind.Wizard, 6, 1);
        public static readonly Creature Skeleton = new("Skeleton", 47, FactionKind.Necromancer, 1, 8);
        public static readonly Creature Zombie = new("Zombie", 48, FactionKind.Necromancer, 2, 6);
        public static readonly Creature MutantZombie = new("Mutant Zombie", 49, FactionKind.Necromancer, 2, 6);
        public static readonly Creature Mummy = new("Mummy", 50, FactionKind.Necromancer, 3, 4);
        public static readonly Creature RoyalMummy = new("Royal Mummy", 51, FactionKind.Necromancer, 3, 4);
        public static readonly Creature Vampire = new("Vampire", 52, FactionKind.Necromancer, 4, 3);
        public static readonly Creature VampireLord = new("Vampire Lord", 53, FactionKind.Necromancer, 4, 3);
        public static readonly Creature Lich = new("Lich", 54, FactionKind.Necromancer, 5, 2);
        public static readonly Creature PowerLich = new("Power Lich", 55, FactionKind.Necromancer, 5, 2);
        public static readonly Creature BoneDragon = new("Bone Dragon", 56, FactionKind.Necromancer, 6, 1);
        public static readonly Creature Rogue = new("Rogue", 57, null, 1, 8);
        public static readonly Creature Nomad = new("Nomad", 58, null, 2, 4);
        public static readonly Creature Ghost = new("Ghost", 59, null, 3, 3);
        public static readonly Creature Genie = new("Genie", 60, null, 6, 2);
        public static readonly Creature Medusa = new("Medusa", 61, null, 4, 5);
        public static readonly Creature EarthElemental = new("Earth Elemental", 62, null, 4, 4);
        public static readonly Creature AirElemental = new("Air Elemental", 63, null, 4, 4);
        public static readonly Creature FireElemental = new("Fire Elemental", 64, null, 4, 4);
        public static readonly Creature WaterElemental = new("Water Elemental", 65, null, 4, 4);

        private Creature(string name, byte code, Faction? faction, int level, int growth)
            : base(name, code)
        {
            this.Faction = faction;
            this.Level = level;
            this.Growth = growth;
        }

        /// <summary>
        /// Gets the town the creature is recruited in, or <c>null</c> for a neutral creature.
        /// </summary>
        public Faction? Faction { get; }

        /// <summary>
        /// Gets the level of the creature among those of its faction, from 1 to 6.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets how many creatures its dwelling grows each week.
        /// </summary>
        public int Growth { get; }

        public static IReadOnlyList<Creature> All => GetAll<Creature>();

        public static IReadOnlyList<Creature> ForFaction(Faction faction) => All.Where(c => c.Faction == faction).ToList();

        public static Creature? FromByte(byte code) => FromCode<Creature>(code);

        public static string NameOf(byte code) => FromByte(code)?.Name ?? $"Unknown creature {code}";
    }
}
