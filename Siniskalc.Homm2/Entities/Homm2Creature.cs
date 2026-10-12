namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A creature of Heroes of Might and Magic II. The code is the number the game keeps in an army slot (0xFF is an empty slot): the
    /// Peasant is 0 and the Black Dragon is 37. The race is the town the creature is recruited in, or <c>null</c> for the neutral ones.
    /// Names, levels and weekly growth come from the creature tables of the open source engine fheroes2 and are checked against the
    /// savegames for the Peasant, the Archer and the Black Dragon and the weekly growth of a Knight castle (12 and 8, 26 and 18 with a well).
    /// </summary>
    public sealed class Homm2Creature : Homm2Enumeration
    {
        public static readonly Homm2Creature Peasant = new("Peasant", 0, Homm2Race.Knight, 1, 12);
        public static readonly Homm2Creature Archer = new("Archer", 1, Homm2Race.Knight, 2, 8);
        public static readonly Homm2Creature Ranger = new("Ranger", 2, Homm2Race.Knight, 2, 8);
        public static readonly Homm2Creature Pikeman = new("Pikeman", 3, Homm2Race.Knight, 3, 5);
        public static readonly Homm2Creature VeteranPikeman = new("Veteran Pikeman", 4, Homm2Race.Knight, 3, 5);
        public static readonly Homm2Creature Swordsman = new("Swordsman", 5, Homm2Race.Knight, 4, 4);
        public static readonly Homm2Creature MasterSwordsman = new("Master Swordsman", 6, Homm2Race.Knight, 4, 4);
        public static readonly Homm2Creature Cavalry = new("Cavalry", 7, Homm2Race.Knight, 5, 3);
        public static readonly Homm2Creature Champion = new("Champion", 8, Homm2Race.Knight, 5, 3);
        public static readonly Homm2Creature Paladin = new("Paladin", 9, Homm2Race.Knight, 6, 2);
        public static readonly Homm2Creature Crusader = new("Crusader", 10, Homm2Race.Knight, 6, 2);
        public static readonly Homm2Creature Goblin = new("Goblin", 11, Homm2Race.Barbarian, 1, 10);
        public static readonly Homm2Creature Orc = new("Orc", 12, Homm2Race.Barbarian, 2, 8);
        public static readonly Homm2Creature OrcChief = new("Orc Chief", 13, Homm2Race.Barbarian, 2, 8);
        public static readonly Homm2Creature Wolf = new("Wolf", 14, Homm2Race.Barbarian, 3, 5);
        public static readonly Homm2Creature Ogre = new("Ogre", 15, Homm2Race.Barbarian, 4, 4);
        public static readonly Homm2Creature OgreLord = new("Ogre Lord", 16, Homm2Race.Barbarian, 4, 4);
        public static readonly Homm2Creature Troll = new("Troll", 17, Homm2Race.Barbarian, 5, 3);
        public static readonly Homm2Creature WarTroll = new("War Troll", 18, Homm2Race.Barbarian, 5, 3);
        public static readonly Homm2Creature Cyclops = new("Cyclops", 19, Homm2Race.Barbarian, 6, 2);
        public static readonly Homm2Creature Sprite = new("Sprite", 20, Homm2Race.Sorceress, 1, 8);
        public static readonly Homm2Creature Dwarf = new("Dwarf", 21, Homm2Race.Sorceress, 2, 6);
        public static readonly Homm2Creature BattleDwarf = new("Battle Dwarf", 22, Homm2Race.Sorceress, 2, 6);
        public static readonly Homm2Creature Elf = new("Elf", 23, Homm2Race.Sorceress, 3, 4);
        public static readonly Homm2Creature GrandElf = new("Grand Elf", 24, Homm2Race.Sorceress, 3, 4);
        public static readonly Homm2Creature Druid = new("Druid", 25, Homm2Race.Sorceress, 4, 3);
        public static readonly Homm2Creature GreaterDruid = new("Greater Druid", 26, Homm2Race.Sorceress, 4, 3);
        public static readonly Homm2Creature Unicorn = new("Unicorn", 27, Homm2Race.Sorceress, 5, 2);
        public static readonly Homm2Creature Phoenix = new("Phoenix", 28, Homm2Race.Sorceress, 6, 1);
        public static readonly Homm2Creature Centaur = new("Centaur", 29, Homm2Race.Warlock, 1, 8);
        public static readonly Homm2Creature Gargoyle = new("Gargoyle", 30, Homm2Race.Warlock, 2, 6);
        public static readonly Homm2Creature Griffin = new("Griffin", 31, Homm2Race.Warlock, 3, 4);
        public static readonly Homm2Creature Minotaur = new("Minotaur", 32, Homm2Race.Warlock, 4, 3);
        public static readonly Homm2Creature MinotaurKing = new("Minotaur King", 33, Homm2Race.Warlock, 4, 3);
        public static readonly Homm2Creature Hydra = new("Hydra", 34, Homm2Race.Warlock, 5, 2);
        public static readonly Homm2Creature GreenDragon = new("Green Dragon", 35, Homm2Race.Warlock, 6, 1);
        public static readonly Homm2Creature RedDragon = new("Red Dragon", 36, Homm2Race.Warlock, 6, 1);
        public static readonly Homm2Creature BlackDragon = new("Black Dragon", 37, Homm2Race.Warlock, 6, 1);
        public static readonly Homm2Creature Halfling = new("Halfling", 38, Homm2Race.Wizard, 1, 8);
        public static readonly Homm2Creature Boar = new("Boar", 39, Homm2Race.Wizard, 2, 6);
        public static readonly Homm2Creature IronGolem = new("Iron Golem", 40, Homm2Race.Wizard, 3, 4);
        public static readonly Homm2Creature SteelGolem = new("Steel Golem", 41, Homm2Race.Wizard, 3, 4);
        public static readonly Homm2Creature Roc = new("Roc", 42, Homm2Race.Wizard, 4, 3);
        public static readonly Homm2Creature Mage = new("Mage", 43, Homm2Race.Wizard, 5, 2);
        public static readonly Homm2Creature Archmage = new("Archmage", 44, Homm2Race.Wizard, 5, 2);
        public static readonly Homm2Creature Giant = new("Giant", 45, Homm2Race.Wizard, 6, 1);
        public static readonly Homm2Creature Titan = new("Titan", 46, Homm2Race.Wizard, 6, 1);
        public static readonly Homm2Creature Skeleton = new("Skeleton", 47, Homm2Race.Necromancer, 1, 8);
        public static readonly Homm2Creature Zombie = new("Zombie", 48, Homm2Race.Necromancer, 2, 6);
        public static readonly Homm2Creature MutantZombie = new("Mutant Zombie", 49, Homm2Race.Necromancer, 2, 6);
        public static readonly Homm2Creature Mummy = new("Mummy", 50, Homm2Race.Necromancer, 3, 4);
        public static readonly Homm2Creature RoyalMummy = new("Royal Mummy", 51, Homm2Race.Necromancer, 3, 4);
        public static readonly Homm2Creature Vampire = new("Vampire", 52, Homm2Race.Necromancer, 4, 3);
        public static readonly Homm2Creature VampireLord = new("Vampire Lord", 53, Homm2Race.Necromancer, 4, 3);
        public static readonly Homm2Creature Lich = new("Lich", 54, Homm2Race.Necromancer, 5, 2);
        public static readonly Homm2Creature PowerLich = new("Power Lich", 55, Homm2Race.Necromancer, 5, 2);
        public static readonly Homm2Creature BoneDragon = new("Bone Dragon", 56, Homm2Race.Necromancer, 6, 1);
        public static readonly Homm2Creature Rogue = new("Rogue", 57, null, 1, 8);
        public static readonly Homm2Creature Nomad = new("Nomad", 58, null, 2, 4);
        public static readonly Homm2Creature Ghost = new("Ghost", 59, null, 3, 3);
        public static readonly Homm2Creature Genie = new("Genie", 60, null, 6, 2);
        public static readonly Homm2Creature Medusa = new("Medusa", 61, null, 4, 5);
        public static readonly Homm2Creature EarthElemental = new("Earth Elemental", 62, null, 4, 4);
        public static readonly Homm2Creature AirElemental = new("Air Elemental", 63, null, 4, 4);
        public static readonly Homm2Creature FireElemental = new("Fire Elemental", 64, null, 4, 4);
        public static readonly Homm2Creature WaterElemental = new("Water Elemental", 65, null, 4, 4);

        private Homm2Creature(string name, byte code, Homm2Race? race, int level, int growth)
            : base(name, code)
        {
            this.Race = race;
            this.Level = level;
            this.Growth = growth;
        }

        /// <summary>
        /// Gets the town the creature is recruited in, or <c>null</c> for a neutral creature.
        /// </summary>
        public Homm2Race? Race { get; }

        /// <summary>
        /// Gets the level of the creature among those of its race, from 1 to 6.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets how many creatures its dwelling grows each week.
        /// </summary>
        public int Growth { get; }

        public static IReadOnlyList<Homm2Creature> All => GetAll<Homm2Creature>();

        public static IReadOnlyList<Homm2Creature> ForRace(Homm2Race race) => All.Where(c => c.Race == race).ToList();

        public static Homm2Creature? FromByte(byte code) => FromCode<Homm2Creature>(code);

        public static string NameOf(byte code) => FromByte(code)?.Name ?? $"Unknown creature {code}";
    }
}
