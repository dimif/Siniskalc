namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using FactionKind = Siniskalc.Homm2.Entities.Faction;

    /// <summary>
    /// A creature of Heroes of Might and Magic II. The code is the number the game keeps in an army slot (0xFF is an empty slot): the
    /// Peasant is 0 and the Black Dragon is 37. The faction is the town the creature is recruited in, or <c>null</c> for the neutral ones.
    /// Names, tiers, stats, costs and weekly growth come from the creature tables of the open source engine fheroes2 and the growth is checked against the
    /// savegames for the Peasant, the Archer and the Black Dragon and the weekly growth of a Knight castle (12 and 8, 26 and 18 with a well).
    /// </summary>
    public sealed class Creature : Homm2Enumeration, ICreature
    {
        public static readonly Creature Peasant = new("Peasant", 0, FactionKind.Knight, 1, new() { Atk = 1, Def = 1, MinDamage = 1, MaxDamage = 1, Health = 1, Speed = 2, Growth = 12, Gold = 20 }, dwelling: Dwellings.Dwelling1);
        public static readonly Creature Archer = new("Archer", 1, FactionKind.Knight, 2, new() { Atk = 5, Def = 3, MinDamage = 2, MaxDamage = 3, Health = 10, Speed = 2, Growth = 8, Gold = 150, Shots = 12, Special = [CreatureAbility.Ranged] }, dwelling: Dwellings.Dwelling2);
        public static readonly Creature Ranger = new("Ranger", 2, FactionKind.Knight, 2, new() { Atk = 5, Def = 3, MinDamage = 2, MaxDamage = 3, Health = 10, Speed = 4, Growth = 8, Gold = 200, Shots = 24, Special = [CreatureAbility.Ranged, CreatureAbility.DoubleShot] }, 1, dwelling: Dwellings.Dwelling2Upgraded);
        public static readonly Creature Pikeman = new("Pikeman", 3, FactionKind.Knight, 3, new() { Atk = 5, Def = 9, MinDamage = 3, MaxDamage = 4, Health = 15, Speed = 4, Growth = 5, Gold = 200 }, dwelling: Dwellings.Dwelling3);
        public static readonly Creature VeteranPikeman = new("Veteran Pikeman", 4, FactionKind.Knight, 3, new() { Atk = 5, Def = 9, MinDamage = 3, MaxDamage = 4, Health = 20, Speed = 5, Growth = 5, Gold = 250 }, 3, dwelling: Dwellings.Dwelling3Upgraded);
        public static readonly Creature Swordsman = new("Swordsman", 5, FactionKind.Knight, 4, new() { Atk = 7, Def = 9, MinDamage = 4, MaxDamage = 6, Health = 25, Speed = 4, Growth = 4, Gold = 250 }, dwelling: Dwellings.Dwelling4);
        public static readonly Creature MasterSwordsman = new("Master Swordsman", 6, FactionKind.Knight, 4, new() { Atk = 7, Def = 9, MinDamage = 4, MaxDamage = 6, Health = 30, Speed = 5, Growth = 4, Gold = 300 }, 5, dwelling: Dwellings.Dwelling4Upgraded);
        public static readonly Creature Cavalry = new("Cavalry", 7, FactionKind.Knight, 5, new() { Atk = 10, Def = 9, MinDamage = 5, MaxDamage = 10, Health = 30, Speed = 6, Growth = 3, Gold = 300 }, dwelling: Dwellings.Dwelling5);
        public static readonly Creature Champion = new("Champion", 8, FactionKind.Knight, 5, new() { Atk = 10, Def = 9, MinDamage = 5, MaxDamage = 10, Health = 40, Speed = 7, Growth = 3, Gold = 375 }, 7, dwelling: Dwellings.Dwelling5Upgraded);
        public static readonly Creature Paladin = new("Paladin", 9, FactionKind.Knight, 6, new() { Atk = 11, Def = 12, MinDamage = 10, MaxDamage = 20, Health = 50, Speed = 5, Growth = 2, Gold = 600, Special = [CreatureAbility.DoubleAttack] }, dwelling: Dwellings.Dwelling6);
        public static readonly Creature Crusader = new("Crusader", 10, FactionKind.Knight, 6, new() { Atk = 11, Def = 12, MinDamage = 10, MaxDamage = 20, Health = 65, Speed = 6, Growth = 2, Gold = 1000, Special = [CreatureAbility.DoubleAttack] }, 9, dwelling: Dwellings.Dwelling6Upgraded);
        public static readonly Creature Goblin = new("Goblin", 11, FactionKind.Barbarian, 1, new() { Atk = 3, Def = 1, MinDamage = 1, MaxDamage = 2, Health = 3, Speed = 4, Growth = 10, Gold = 40 }, dwelling: Dwellings.Dwelling1);
        public static readonly Creature Orc = new("Orc", 12, FactionKind.Barbarian, 2, new() { Atk = 3, Def = 4, MinDamage = 2, MaxDamage = 3, Health = 10, Speed = 2, Growth = 8, Gold = 140, Shots = 8, Special = [CreatureAbility.Ranged] }, dwelling: Dwellings.Dwelling2);
        public static readonly Creature OrcChief = new("Orc Chief", 13, FactionKind.Barbarian, 2, new() { Atk = 3, Def = 4, MinDamage = 3, MaxDamage = 4, Health = 15, Speed = 3, Growth = 8, Gold = 175, Shots = 16, Special = [CreatureAbility.Ranged] }, 12, dwelling: Dwellings.Dwelling2Upgraded);
        public static readonly Creature Wolf = new("Wolf", 14, FactionKind.Barbarian, 3, new() { Atk = 6, Def = 2, MinDamage = 3, MaxDamage = 5, Health = 20, Speed = 6, Growth = 5, Gold = 200, Special = [CreatureAbility.DoubleAttack] }, dwelling: Dwellings.Dwelling3);
        public static readonly Creature Ogre = new("Ogre", 15, FactionKind.Barbarian, 4, new() { Atk = 9, Def = 5, MinDamage = 4, MaxDamage = 6, Health = 40, Speed = 2, Growth = 4, Gold = 300 }, dwelling: Dwellings.Dwelling4);
        public static readonly Creature OgreLord = new("Ogre Lord", 16, FactionKind.Barbarian, 4, new() { Atk = 9, Def = 5, MinDamage = 5, MaxDamage = 7, Health = 60, Speed = 4, Growth = 4, Gold = 500 }, 15, dwelling: Dwellings.Dwelling4Upgraded);
        public static readonly Creature Troll = new("Troll", 17, FactionKind.Barbarian, 5, new() { Atk = 10, Def = 5, MinDamage = 5, MaxDamage = 7, Health = 40, Speed = 4, Growth = 3, Gold = 600, Shots = 8, Special = [CreatureAbility.Ranged, CreatureAbility.Regeneration] }, dwelling: Dwellings.Dwelling5);
        public static readonly Creature WarTroll = new("War Troll", 18, FactionKind.Barbarian, 5, new() { Atk = 10, Def = 5, MinDamage = 7, MaxDamage = 9, Health = 40, Speed = 5, Growth = 3, Gold = 700, Shots = 16, Special = [CreatureAbility.Ranged, CreatureAbility.Regeneration] }, 17, dwelling: Dwellings.Dwelling5Upgraded);
        public static readonly Creature Cyclops = new("Cyclops", 19, FactionKind.Barbarian, 6, new() { Atk = 12, Def = 9, MinDamage = 12, MaxDamage = 24, Health = 80, Speed = 5, Growth = 2, Gold = 750, Crystal = 1, Special = [CreatureAbility.ParalyzingAttack] }, dwelling: Dwellings.Dwelling6);
        public static readonly Creature Sprite = new("Sprite", 20, FactionKind.Sorceress, 1, new() { Atk = 4, Def = 2, MinDamage = 1, MaxDamage = 2, Health = 2, Speed = 4, Growth = 8, Gold = 50, Special = [CreatureAbility.Flying] }, dwelling: Dwellings.Dwelling1);
        public static readonly Creature Dwarf = new("Dwarf", 21, FactionKind.Sorceress, 2, new() { Atk = 6, Def = 5, MinDamage = 2, MaxDamage = 4, Health = 20, Speed = 2, Growth = 6, Gold = 200, Special = [CreatureAbility.MagicResistance] }, dwelling: Dwellings.Dwelling2);
        public static readonly Creature BattleDwarf = new("Battle Dwarf", 22, FactionKind.Sorceress, 2, new() { Atk = 6, Def = 6, MinDamage = 2, MaxDamage = 4, Health = 20, Speed = 4, Growth = 6, Gold = 250, Special = [CreatureAbility.MagicResistance] }, 21, dwelling: Dwellings.Dwelling2Upgraded);
        public static readonly Creature Elf = new("Elf", 23, FactionKind.Sorceress, 3, new() { Atk = 4, Def = 3, MinDamage = 2, MaxDamage = 3, Health = 15, Speed = 4, Growth = 4, Gold = 250, Shots = 24, Special = [CreatureAbility.Ranged] }, dwelling: Dwellings.Dwelling3);
        public static readonly Creature GrandElf = new("Grand Elf", 24, FactionKind.Sorceress, 3, new() { Atk = 5, Def = 5, MinDamage = 2, MaxDamage = 3, Health = 15, Speed = 6, Growth = 4, Gold = 300, Shots = 24, Special = [CreatureAbility.Ranged, CreatureAbility.DoubleShot] }, 23, dwelling: Dwellings.Dwelling3Upgraded);
        public static readonly Creature Druid = new("Druid", 25, FactionKind.Sorceress, 4, new() { Atk = 7, Def = 5, MinDamage = 5, MaxDamage = 8, Health = 25, Speed = 5, Growth = 3, Gold = 350, Shots = 8, Special = [CreatureAbility.Ranged] }, dwelling: Dwellings.Dwelling4);
        public static readonly Creature GreaterDruid = new("Greater Druid", 26, FactionKind.Sorceress, 4, new() { Atk = 7, Def = 7, MinDamage = 5, MaxDamage = 8, Health = 25, Speed = 6, Growth = 3, Gold = 400, Shots = 16, Special = [CreatureAbility.Ranged] }, 25, dwelling: Dwellings.Dwelling4Upgraded);
        public static readonly Creature Unicorn = new("Unicorn", 27, FactionKind.Sorceress, 5, new() { Atk = 10, Def = 9, MinDamage = 7, MaxDamage = 14, Health = 40, Speed = 5, Growth = 2, Gold = 500, Special = [CreatureAbility.BlindingAttack] }, dwelling: Dwellings.Dwelling5);
        public static readonly Creature Phoenix = new("Phoenix", 28, FactionKind.Sorceress, 6, new() { Atk = 12, Def = 10, MinDamage = 20, MaxDamage = 40, Health = 100, Speed = 7, Growth = 1, Gold = 1500, Mercury = 1, Special = [CreatureAbility.Flying, CreatureAbility.ElementalSpellsImmunity] }, dwelling: Dwellings.Dwelling6);
        public static readonly Creature Centaur = new("Centaur", 29, FactionKind.Warlock, 1, new() { Atk = 3, Def = 1, MinDamage = 1, MaxDamage = 2, Health = 5, Speed = 4, Growth = 8, Gold = 60, Shots = 8, Special = [CreatureAbility.Ranged] }, dwelling: Dwellings.Dwelling1);
        public static readonly Creature Gargoyle = new("Gargoyle", 30, FactionKind.Warlock, 2, new() { Atk = 4, Def = 7, MinDamage = 2, MaxDamage = 3, Health = 15, Speed = 6, Growth = 6, Gold = 200, Special = [CreatureAbility.Flying] }, dwelling: Dwellings.Dwelling2);
        public static readonly Creature Griffin = new("Griffin", 31, FactionKind.Warlock, 3, new() { Atk = 6, Def = 6, MinDamage = 3, MaxDamage = 5, Health = 25, Speed = 4, Growth = 4, Gold = 300, Special = [CreatureAbility.Flying, CreatureAbility.UnlimitedRetaliations] }, dwelling: Dwellings.Dwelling3);
        public static readonly Creature Minotaur = new("Minotaur", 32, FactionKind.Warlock, 4, new() { Atk = 9, Def = 8, MinDamage = 5, MaxDamage = 10, Health = 35, Speed = 4, Growth = 3, Gold = 400 }, dwelling: Dwellings.Dwelling4);
        public static readonly Creature MinotaurKing = new("Minotaur King", 33, FactionKind.Warlock, 4, new() { Atk = 9, Def = 8, MinDamage = 5, MaxDamage = 10, Health = 45, Speed = 6, Growth = 3, Gold = 500 }, 32, dwelling: Dwellings.Dwelling4Upgraded);
        public static readonly Creature Hydra = new("Hydra", 34, FactionKind.Warlock, 5, new() { Atk = 8, Def = 9, MinDamage = 6, MaxDamage = 12, Health = 75, Speed = 2, Growth = 2, Gold = 800, Special = [CreatureAbility.NoEnemyRetaliation, CreatureAbility.AttacksAllAdjacentEnemies] }, dwelling: Dwellings.Dwelling5);
        public static readonly Creature GreenDragon = new("Green Dragon", 35, FactionKind.Warlock, 6, new() { Atk = 12, Def = 12, MinDamage = 25, MaxDamage = 50, Health = 200, Speed = 4, Growth = 1, Gold = 3000, Sulfur = 1, Special = [CreatureAbility.Flying, CreatureAbility.BreathAttack, CreatureAbility.Dragon] }, dwelling: Dwellings.Dwelling6);
        public static readonly Creature RedDragon = new("Red Dragon", 36, FactionKind.Warlock, 6, new() { Atk = 13, Def = 13, MinDamage = 25, MaxDamage = 50, Health = 250, Speed = 5, Growth = 1, Gold = 3500, Sulfur = 1, Special = [CreatureAbility.Flying, CreatureAbility.BreathAttack, CreatureAbility.Dragon] }, 35, dwelling: Dwellings.Dwelling6Upgraded);
        public static readonly Creature BlackDragon = new("Black Dragon", 37, FactionKind.Warlock, 6, new() { Atk = 14, Def = 14, MinDamage = 25, MaxDamage = 50, Health = 300, Speed = 6, Growth = 1, Gold = 4000, Sulfur = 2, Special = [CreatureAbility.Flying, CreatureAbility.BreathAttack, CreatureAbility.Dragon, CreatureAbility.MagicImmunity] }, 36, dwelling: Dwellings.Dwelling6UpgradedTwice);
        public static readonly Creature Halfling = new("Halfling", 38, FactionKind.Wizard, 1, new() { Atk = 2, Def = 1, MinDamage = 1, MaxDamage = 3, Health = 3, Speed = 3, Growth = 8, Gold = 50, Shots = 12, Special = [CreatureAbility.Ranged] }, dwelling: Dwellings.Dwelling1);
        public static readonly Creature Boar = new("Boar", 39, FactionKind.Wizard, 2, new() { Atk = 5, Def = 4, MinDamage = 2, MaxDamage = 3, Health = 15, Speed = 6, Growth = 6, Gold = 150 }, dwelling: Dwellings.Dwelling2);
        public static readonly Creature IronGolem = new("Iron Golem", 40, FactionKind.Wizard, 3, new() { Atk = 5, Def = 10, MinDamage = 4, MaxDamage = 5, Health = 30, Speed = 2, Growth = 4, Gold = 300, Special = [CreatureAbility.SpellDamageReduction] }, dwelling: Dwellings.Dwelling3);
        public static readonly Creature SteelGolem = new("Steel Golem", 41, FactionKind.Wizard, 3, new() { Atk = 7, Def = 10, MinDamage = 4, MaxDamage = 5, Health = 35, Speed = 3, Growth = 4, Gold = 350, Special = [CreatureAbility.SpellDamageReduction] }, 40, dwelling: Dwellings.Dwelling3Upgraded);
        public static readonly Creature Roc = new("Roc", 42, FactionKind.Wizard, 4, new() { Atk = 7, Def = 7, MinDamage = 4, MaxDamage = 8, Health = 40, Speed = 4, Growth = 3, Gold = 400, Special = [CreatureAbility.Flying] }, dwelling: Dwellings.Dwelling4);
        public static readonly Creature Mage = new("Mage", 43, FactionKind.Wizard, 5, new() { Atk = 11, Def = 7, MinDamage = 7, MaxDamage = 9, Health = 30, Speed = 5, Growth = 2, Gold = 600, Shots = 12, Special = [CreatureAbility.Ranged] }, dwelling: Dwellings.Dwelling5);
        public static readonly Creature Archmage = new("Archmage", 44, FactionKind.Wizard, 5, new() { Atk = 12, Def = 8, MinDamage = 7, MaxDamage = 9, Health = 35, Speed = 6, Growth = 2, Gold = 700, Shots = 24, Special = [CreatureAbility.Ranged] }, 43, dwelling: Dwellings.Dwelling5Upgraded);
        public static readonly Creature Giant = new("Giant", 45, FactionKind.Wizard, 6, new() { Atk = 13, Def = 10, MinDamage = 20, MaxDamage = 30, Health = 150, Speed = 4, Growth = 1, Gold = 2000, Gems = 1, Special = [CreatureAbility.MindSpellsImmunity] }, dwelling: Dwellings.Dwelling6);
        public static readonly Creature Titan = new("Titan", 46, FactionKind.Wizard, 6, new() { Atk = 15, Def = 15, MinDamage = 20, MaxDamage = 30, Health = 300, Speed = 6, Growth = 1, Gold = 5000, Gems = 2, Shots = 24, Special = [CreatureAbility.Ranged, CreatureAbility.MindSpellsImmunity] }, 45, dwelling: Dwellings.Dwelling6Upgraded);
        public static readonly Creature Skeleton = new("Skeleton", 47, FactionKind.Necromancer, 1, new() { Atk = 4, Def = 3, MinDamage = 2, MaxDamage = 3, Health = 4, Speed = 4, Growth = 8, Gold = 75, Special = [CreatureAbility.Undead] }, dwelling: Dwellings.Dwelling1);
        public static readonly Creature Zombie = new("Zombie", 48, FactionKind.Necromancer, 2, new() { Atk = 5, Def = 2, MinDamage = 2, MaxDamage = 3, Health = 15, Speed = 2, Growth = 6, Gold = 150, Special = [CreatureAbility.Undead] }, dwelling: Dwellings.Dwelling2);
        public static readonly Creature MutantZombie = new("Mutant Zombie", 49, FactionKind.Necromancer, 2, new() { Atk = 5, Def = 2, MinDamage = 2, MaxDamage = 3, Health = 20, Speed = 4, Growth = 6, Gold = 200, Special = [CreatureAbility.Undead] }, 48, dwelling: Dwellings.Dwelling2Upgraded);
        public static readonly Creature Mummy = new("Mummy", 50, FactionKind.Necromancer, 3, new() { Atk = 6, Def = 6, MinDamage = 3, MaxDamage = 4, Health = 25, Speed = 4, Growth = 4, Gold = 250, Special = [CreatureAbility.Undead, CreatureAbility.Curse] }, dwelling: Dwellings.Dwelling3);
        public static readonly Creature RoyalMummy = new("Royal Mummy", 51, FactionKind.Necromancer, 3, new() { Atk = 6, Def = 6, MinDamage = 3, MaxDamage = 4, Health = 30, Speed = 5, Growth = 4, Gold = 300, Special = [CreatureAbility.Undead, CreatureAbility.Curse] }, 50, dwelling: Dwellings.Dwelling3Upgraded);
        public static readonly Creature Vampire = new("Vampire", 52, FactionKind.Necromancer, 4, new() { Atk = 8, Def = 6, MinDamage = 5, MaxDamage = 7, Health = 30, Speed = 4, Growth = 3, Gold = 500, Special = [CreatureAbility.Flying, CreatureAbility.Undead] }, dwelling: Dwellings.Dwelling4);
        public static readonly Creature VampireLord = new("Vampire Lord", 53, FactionKind.Necromancer, 4, new() { Atk = 8, Def = 6, MinDamage = 5, MaxDamage = 7, Health = 40, Speed = 5, Growth = 3, Gold = 650, Special = [CreatureAbility.Flying, CreatureAbility.Undead, CreatureAbility.LifeDrain] }, 52, dwelling: Dwellings.Dwelling4Upgraded);
        public static readonly Creature Lich = new("Lich", 54, FactionKind.Necromancer, 5, new() { Atk = 7, Def = 12, MinDamage = 8, MaxDamage = 10, Health = 25, Speed = 5, Growth = 2, Gold = 750, Shots = 12, Special = [CreatureAbility.Ranged, CreatureAbility.Undead, CreatureAbility.DeathCloud] }, dwelling: Dwellings.Dwelling5);
        public static readonly Creature PowerLich = new("Power Lich", 55, FactionKind.Necromancer, 5, new() { Atk = 7, Def = 13, MinDamage = 8, MaxDamage = 10, Health = 35, Speed = 6, Growth = 2, Gold = 900, Shots = 24, Special = [CreatureAbility.Ranged, CreatureAbility.Undead, CreatureAbility.DeathCloud] }, 54, dwelling: Dwellings.Dwelling5Upgraded);
        public static readonly Creature BoneDragon = new("Bone Dragon", 56, FactionKind.Necromancer, 6, new() { Atk = 11, Def = 9, MinDamage = 25, MaxDamage = 45, Health = 150, Speed = 4, Growth = 1, Gold = 1500, Special = [CreatureAbility.Flying, CreatureAbility.Dragon, CreatureAbility.Undead, CreatureAbility.MoraleMinus1] }, dwelling: Dwellings.Dwelling6);
        public static readonly Creature Rogue = new("Rogue", 57, null, 1, new() { Atk = 6, Def = 1, MinDamage = 1, MaxDamage = 2, Health = 4, Speed = 5, Growth = 8, Gold = 50 });
        public static readonly Creature Nomad = new("Nomad", 58, null, 2, new() { Atk = 7, Def = 6, MinDamage = 2, MaxDamage = 5, Health = 20, Speed = 6, Growth = 4, Gold = 200 });
        public static readonly Creature Ghost = new("Ghost", 59, null, 3, new() { Atk = 8, Def = 7, MinDamage = 4, MaxDamage = 6, Health = 20, Speed = 5, Growth = 3, Gold = 1000, Special = [CreatureAbility.Flying, CreatureAbility.Undead] });
        public static readonly Creature Genie = new("Genie", 60, null, 6, new() { Atk = 10, Def = 9, MinDamage = 20, MaxDamage = 30, Health = 50, Speed = 6, Growth = 2, Gold = 650, Gems = 1, Special = [CreatureAbility.Flying, CreatureAbility.HalvesEnemyStack] });
        public static readonly Creature Medusa = new("Medusa", 61, null, 4, new() { Atk = 8, Def = 9, MinDamage = 6, MaxDamage = 10, Health = 35, Speed = 4, Growth = 5, Gold = 500, Special = [CreatureAbility.PetrifyingAttack] });
        public static readonly Creature EarthElemental = new("Earth Elemental", 62, null, 4, new() { Atk = 8, Def = 8, MinDamage = 4, MaxDamage = 5, Health = 50, Speed = 3, Growth = 4, Gold = 500, Special = [CreatureAbility.Elemental, CreatureAbility.MindSpellsImmunity, CreatureAbility.NeutralMorale, CreatureAbility.ImmuneToLightningAndMeteorSpells] });
        public static readonly Creature AirElemental = new("Air Elemental", 63, null, 4, new() { Atk = 7, Def = 7, MinDamage = 2, MaxDamage = 8, Health = 35, Speed = 6, Growth = 4, Gold = 500, Special = [CreatureAbility.Elemental, CreatureAbility.MindSpellsImmunity, CreatureAbility.NeutralMorale, CreatureAbility.ImmuneToMeteorShower, CreatureAbility.VulnerableToLightningSpells] });
        public static readonly Creature FireElemental = new("Fire Elemental", 64, null, 4, new() { Atk = 8, Def = 6, MinDamage = 4, MaxDamage = 6, Health = 40, Speed = 5, Growth = 4, Gold = 500, Special = [CreatureAbility.Elemental, CreatureAbility.MindSpellsImmunity, CreatureAbility.NeutralMorale, CreatureAbility.VulnerableToColdSpells, CreatureAbility.FireImmunity] });
        public static readonly Creature WaterElemental = new("Water Elemental", 65, null, 4, new() { Atk = 6, Def = 8, MinDamage = 3, MaxDamage = 7, Health = 45, Speed = 4, Growth = 4, Gold = 500, Special = [CreatureAbility.Elemental, CreatureAbility.MindSpellsImmunity, CreatureAbility.NeutralMorale, CreatureAbility.ImmuneToColdSpells] });

        private Creature(string name, byte code, Faction? faction, int tier, CreatureInfo info, byte? baseCreatureCode = null, Dwellings? dwelling = null)
            : base(name, code)
        {
            this.Faction = faction;
            this.Tier = tier;
            this.Info = info;
            this.BaseCreatureCode = baseCreatureCode;
            this.Dwelling = dwelling;
        }

        public static IReadOnlyList<Creature> All => GetAll<Creature>();

        /// <summary>
        /// Gets the town the creature is recruited in, or <c>null</c> for a neutral creature.
        /// </summary>
        public Faction? Faction { get; }

        /// <inheritdoc/>
        public int Tier { get; }

        /// <summary>
        /// Gets the stats of the creature: combat values, cost, weekly growth (<see cref="CreatureInfo.Growth"/>), shots and abilities.
        /// The game has no AI or fight values, so those are 0.
        /// </summary>
        public CreatureInfo Info { get; }

        /// <summary>
        /// Gets the code of the creature this one is the upgrade of, or <c>null</c> for a creature that is not an upgrade.
        /// </summary>
        public byte? BaseCreatureCode { get; }

        bool ICreature.IsUpgrade => this.BaseCreatureCode != null;

        IFaction ICreature.Faction => this.Faction ?? Entities.Faction.Neutral;

        /// <summary>
        /// Gets the dwelling flag of its castle that recruits the creature: the dwelling of a creature, the upgraded dwelling of an upgraded
        /// creature, or <c>null</c> for a neutral creature that no castle recruits.
        /// </summary>
        public Dwellings? Dwelling { get; }

        public static IReadOnlyList<Creature> ForFaction(Faction faction) => All.Where(c => c.Faction == faction).ToList();

        public static Creature? FromByte(byte code) => FromCode<Creature>(code);

        public static string NameOf(byte code) => FromByte(code)?.Name ?? $"Unknown creature {code}";
    }
}
