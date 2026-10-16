namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Something a creature of Heroes of Might and Magic II can do or suffer beyond the plain attack and defense of its stats.
    /// The set is the flags the open source engine fheroes2 keeps for the creatures, not the one of Heroes 3. <see cref="Ranged"/> does not
    /// say how many shots the creature has; that is <see cref="CreatureInfo.Shots"/>. Which creature has which ability was matched to the
    /// fheroes2 flags and the manual, and has not been checked in the game.
    /// </summary>
    public sealed class CreatureAbility : Homm2Enumeration, ICreatureAbility
    {
        public static readonly CreatureAbility Ranged = new("Ranged", 0);
        public static readonly CreatureAbility Flying = new("Flying", 1);
        public static readonly CreatureAbility DoubleShot = new("Double shot", 2);
        public static readonly CreatureAbility DoubleAttack = new("Double attack", 3);
        public static readonly CreatureAbility NoEnemyRetaliation = new("No enemy retaliation", 4);
        public static readonly CreatureAbility AttacksAllAdjacentEnemies = new("Attacks all adjacent enemies", 5);
        public static readonly CreatureAbility UnlimitedRetaliations = new("Unlimited retaliations", 6);
        public static readonly CreatureAbility BreathAttack = new("Breath attack", 7);
        public static readonly CreatureAbility Dragon = new("Dragon", 8);
        public static readonly CreatureAbility Undead = new("Undead", 9);
        public static readonly CreatureAbility Regeneration = new("Regeneration", 10);
        public static readonly CreatureAbility LifeDrain = new("Life drain", 11);
        public static readonly CreatureAbility DeathCloud = new("Death cloud", 12);
        public static readonly CreatureAbility MindSpellsImmunity = new("Mind spells immunity", 13);
        public static readonly CreatureAbility MagicImmunity = new("Magic immunity", 14);
        public static readonly CreatureAbility MoraleMinus1 = new("Morale -1", 15);
        public static readonly CreatureAbility Curse = new("Curse", 16);
        public static readonly CreatureAbility BlindingAttack = new("Blinding attack (20% chance)", 17);
        public static readonly CreatureAbility ParalyzingAttack = new("Paralyzing attack", 18);
        public static readonly CreatureAbility PetrifyingAttack = new("Petrifying attack", 19);
        public static readonly CreatureAbility Elemental = new("Elemental", 20);
        public static readonly CreatureAbility FireImmunity = new("Immune to Fireball and Fireblast", 21);
        public static readonly CreatureAbility HalvesEnemyStack = new("Halves an enemy stack", 22);
        public static readonly CreatureAbility MagicResistance = new("Magic resistance (25% chance)", 23);
        public static readonly CreatureAbility SpellDamageReduction = new("Spell damage reduction (50%)", 24);
        public static readonly CreatureAbility ElementalSpellsImmunity = new("Elemental spells immunity", 25);
        public static readonly CreatureAbility NeutralMorale = new("Always neutral morale", 26);
        public static readonly CreatureAbility VulnerableToColdSpells = new("Vulnerable to Cold Ray and Cold Ring (+100% damage)", 27);
        public static readonly CreatureAbility ImmuneToLightningAndMeteorSpells = new("Immune to Lightning Bolt, Chain Lightning, Elemental Storm and Meteor Shower", 28);
        public static readonly CreatureAbility ImmuneToMeteorShower = new("Immune to Meteor Shower", 29);
        public static readonly CreatureAbility VulnerableToLightningSpells = new("Vulnerable to Lightning Bolt, Chain Lightning and Elemental Storm (+100% damage)", 30);
        public static readonly CreatureAbility ImmuneToColdSpells = new("Immune to Cold Ray and Cold Ring", 31);

        private CreatureAbility(string name, byte code)
            : base(name, code)
        {
        }

        public bool IsRanged => this == Ranged;

        public static IReadOnlyList<CreatureAbility> All => GetAll<CreatureAbility>();
    }
}
