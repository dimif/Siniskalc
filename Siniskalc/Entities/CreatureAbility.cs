namespace Siniskalc.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// Something a creature can do or suffer beyond the plain attack and defense of its stats, as listed on the Heroes 3 wiki.
    /// <see cref="Ranged"/> does not say how many shots the creature has; that is <see cref="CreatureInfo.Shots"/>.
    /// </summary>
    public sealed class CreatureAbility : Homm3Enumeration
    {
        public static readonly CreatureAbility Spying = new("Spying", 0);
        public static readonly CreatureAbility DoublesFriendlyUnitsLuckChance = new("Doubles friendly units Luck chance", 1);
        public static readonly CreatureAbility SpellcasterFortune = new("Spellcaster (Fortune)", 2);
        public static readonly CreatureAbility Undead = new("Undead", 3);
        public static readonly CreatureAbility Curse = new("Curse", 4);
        public static readonly CreatureAbility Sandwalker = new("Sandwalker", 5);
        public static readonly CreatureAbility Ranged = new("Ranged", 6);
        public static readonly CreatureAbility NoRangeAndObstaclePenalty = new("No range and obstacle penalty", 7);
        public static readonly CreatureAbility SpellcasterMirth = new("Spellcaster (Mirth)", 8);
        public static readonly CreatureAbility Unliving = new("Unliving", 9);
        public static readonly CreatureAbility SpellDamageResistancePlus80Percent = new("Spell damage resistance (+80%)", 10);
        public static readonly CreatureAbility Regeneration = new("Regeneration", 11);
        public static readonly CreatureAbility SpellDamageResistancePlus85Percent = new("Spell damage resistance (+85%)", 12);
        public static readonly CreatureAbility Flying = new("Flying", 13);
        public static readonly CreatureAbility MindSpellsImmunity = new("Mind spells immunity", 14);
        public static readonly CreatureAbility UnlimitedRetaliations = new("Unlimited retaliations", 15);
        public static readonly CreatureAbility HypnotizingAttack = new("Hypnotizing attack", 16);
        public static readonly CreatureAbility SpellDamageResistancePlus95Percent = new("Spell Damage Resistance (+95%)", 17);
        public static readonly CreatureAbility NoMeleeAndObstaclePenalty = new("No melee and obstacle penalty", 18);
        public static readonly CreatureAbility Spellcaster = new("Spellcaster", 19);
        public static readonly CreatureAbility Dragon = new("Dragon", 20);
        public static readonly CreatureAbility NaturalMagicMirror = new("Natural Magic Mirror", 21);
        public static readonly CreatureAbility BreathAttack = new("Breath attack", 22);
        public static readonly CreatureAbility AcidBreath = new("Acid breath", 23);
        public static readonly CreatureAbility CrystalGeneration = new("Crystal generation", 24);
        public static readonly CreatureAbility MagicResistancePlus20Percent = new("Magic resistance (+20%)", 25);
        public static readonly CreatureAbility Fear = new("Fear", 26);
        public static readonly CreatureAbility Fearless = new("Fearless", 27);
        public static readonly CreatureAbility SpellsImmunityLevels1To3 = new("1-3 lvl spells immunity", 28);
        public static readonly CreatureAbility ImmuneToJousting = new("Immune to Jousting", 29);
        public static readonly CreatureAbility DoubleAttack = new("Double attack", 30);
        public static readonly CreatureAbility TwoRetaliations = new("Two retaliations", 31);
        public static readonly CreatureAbility NoMeleePenalty = new("No melee penalty", 32);
        public static readonly CreatureAbility Jousting = new("Jousting", 33);
        public static readonly CreatureAbility HatesDevils = new("Hates Devils", 34);
        public static readonly CreatureAbility MoralePlus1 = new("Morale +1", 35);
        public static readonly CreatureAbility Resurrection = new("Resurrection", 36);
        public static readonly CreatureAbility ResistancePlus20Percent = new("Resistance (+20%)", 37);
        public static readonly CreatureAbility ResistancePlus40Percent = new("Resistance (+40%)", 38);
        public static readonly CreatureAbility MagicDamper = new("Magic damper", 39);
        public static readonly CreatureAbility Binding = new("Binding", 40);
        public static readonly CreatureAbility BlindingAttack = new("Blinding attack", 41);
        public static readonly CreatureAbility AuraOfResistancePlus20Percent = new("Aura of Resistance (+20%)", 42);
        public static readonly CreatureAbility SpellsImmunityLevels1To4 = new("1-4 lvl spells immunity", 43);
        public static readonly CreatureAbility Bloodless = new("Bloodless", 44);
        public static readonly CreatureAbility SpellDamageResistancePlus50Percent = new("Spell damage resistance +50%", 45);
        public static readonly CreatureAbility SpellDamageResistancePlus75Percent = new("Spell damage resistance +75%", 46);
        public static readonly CreatureAbility NoObstaclePenalty = new("No obstacle penalty", 47);
        public static readonly CreatureAbility MagicChanneler = new("Magic channeler", 48);
        public static readonly CreatureAbility HatesEfreetAndEfreetSultans = new("Hates Efreet and Efreet Sultans", 49);
        public static readonly CreatureAbility NoEnemyRetaliation = new("No enemy retaliation", 50);
        public static readonly CreatureAbility HatesBlackDragons = new("Hates Black Dragons", 51);
        public static readonly CreatureAbility MagicChannel = new("Magic channel", 52);
        public static readonly CreatureAbility FireballAttack = new("Fireball attack", 53);
        public static readonly CreatureAbility ThreeHeadedAttack = new("3-headed attack", 54);
        public static readonly CreatureAbility SummonDemons = new("Summon demons", 55);
        public static readonly CreatureAbility FireImmunity = new("Fire immunity", 56);
        public static readonly CreatureAbility HatesGenies = new("Hates Genies", 57);
        public static readonly CreatureAbility NaturalFireShield = new("Natural Fire Shield", 58);
        public static readonly CreatureAbility Teleporting = new("Teleporting", 59);
        public static readonly CreatureAbility LuckMinus1 = new("Luck -1", 60);
        public static readonly CreatureAbility HatesAngels = new("Hates Angels", 61);
        public static readonly CreatureAbility LuckMinus2 = new("Luck -2", 62);
        public static readonly CreatureAbility Disease = new("Disease", 63);
        public static readonly CreatureAbility ManaDrain = new("Mana drain", 64);
        public static readonly CreatureAbility LifeDrain = new("Life drain", 65);
        public static readonly CreatureAbility DeathCloud = new("Death cloud", 66);
        public static readonly CreatureAbility DeathBlow = new("Death blow", 67);
        public static readonly CreatureAbility MoraleMinus1 = new("Morale -1", 68);
        public static readonly CreatureAbility Aging = new("Aging", 69);
        public static readonly CreatureAbility ImmuneToBlindingAndStoneGaze = new("Immune to Blinding and Stone gaze", 70);
        public static readonly CreatureAbility StrikeAndReturn = new("Strike and return", 71);
        public static readonly CreatureAbility PetrifyingAttack = new("Petrifying attack", 72);
        public static readonly CreatureAbility PositiveMorale = new("Positive Morale", 73);
        public static readonly CreatureAbility ParalyzingVenom = new("Paralyzing venom", 74);
        public static readonly CreatureAbility MagicImmunity = new("Magic immunity", 75);
        public static readonly CreatureAbility HatesTitans = new("Hates Titans", 76);
        public static readonly CreatureAbility SpellcasterBloodlust = new("Spellcaster (Bloodlust)", 77);
        public static readonly CreatureAbility LightningStrike = new("Lightning strike", 78);
        public static readonly CreatureAbility CanAttackSiegeWalls = new("Can attack siege walls", 79);
        public static readonly CreatureAbility Ignores40PercentOfEnemyDefenseValue = new("Ignores 40% of enemy defense value", 80);
        public static readonly CreatureAbility Ignores80PercentOfEnemyDefenseValue = new("Ignores 80% of enemy defense value", 81);
        public static readonly CreatureAbility DispelsBeneficialSpells = new("Dispels beneficial spells", 82);
        public static readonly CreatureAbility Weakness = new("Weakness", 83);
        public static readonly CreatureAbility DeathStare10PercentChancePerUnit = new("Death stare (10% chance per unit)", 84);
        public static readonly CreatureAbility Poisonous = new("Poisonous", 85);
        public static readonly CreatureAbility AttacksAllAdjacentEnemies = new("Attacks all adjacent enemies", 86);
        public static readonly CreatureAbility Elemental = new("Elemental", 87);
        public static readonly CreatureAbility ImmuneToMeteorShower = new("Immune to Meteor Shower", 88);
        public static readonly CreatureAbility VulnerableToLightningBoltChainLightningTitansLightningBoltAndArmageddon = new("Vulnerable to Lightning Bolt, Chain Lightning, Titan's Lightning Bolt and Armageddon", 89);
        public static readonly CreatureAbility Plus100PercentToBasicDamageToEarthAndMagmaElementals = new("+100% to basic damage to Earth and Magma Elementals", 90);
        public static readonly CreatureAbility SpellcasterProtectionFromAir = new("Spellcaster (Protection from Air)", 91);
        public static readonly CreatureAbility ImmuneToIceBoltAndFrostRing = new("Immune to Ice Bolt and Frost Ring", 92);
        public static readonly CreatureAbility VulnerableToFireballInfernoAndArmageddon = new("Vulnerable to Fireball, Inferno and Armageddon", 93);
        public static readonly CreatureAbility Plus100PercentToBasicDamageToFireAndEnergyElementals = new("+100% to basic damage to Fire and Energy Elementals", 94);
        public static readonly CreatureAbility SpellcasterProtectionFromWater = new("Spellcaster (Protection from Water)", 95);
        public static readonly CreatureAbility VulnerableToIceBoltAndFrostRing = new("Vulnerable to Ice Bolt and Frost Ring", 96);
        public static readonly CreatureAbility Plus100PercentToBasicDamageToWaterAndIceElementals = new("+100% to basic damage to Water and Ice Elementals", 97);
        public static readonly CreatureAbility SpellcasterProtectionFromFire = new("Spellcaster (Protection from Fire)", 98);
        public static readonly CreatureAbility ImmuneToLightningBoltChainLightningTitansLightningBoltAndArmageddon = new("Immune to Lightning Bolt, Chain Lightning, Titan's Lightning Bolt and Armageddon", 99);
        public static readonly CreatureAbility VulnerableToMeteorShower = new("Vulnerable to Meteor Shower", 100);
        public static readonly CreatureAbility Plus100PercentToBasicDamageToAirAndStormElementals = new("+100% to basic damage to Air and Storm Elementals", 101);
        public static readonly CreatureAbility SpellcasterProtectionFromEarth = new("Spellcaster (Protection from Earth)", 102);
        public static readonly CreatureAbility Minus50PercentDamageToCreaturesWithMindSpellsImmunity = new("-50% damage to creatures with Mind spells immunity", 103);
        public static readonly CreatureAbility Minus50PercentDamageToCreaturesWithMagicImmunity = new("-50% damage to creatures with Magic immunity", 104);
        public static readonly CreatureAbility FireResistance50Percent = new("50% Fire resistance", 105);
        public static readonly CreatureAbility Rebirth = new("Rebirth", 106);
        public static readonly CreatureAbility AccurateShot = new("Accurate shot", 107);
        public static readonly CreatureAbility Ferocity = new("Ferocity", 108);
        public static readonly CreatureAbility SpellcasterAfterShotWeaknessDisruptingRay = new("Spellcaster (after shot, Weakness / Disrupting Ray)", 109);
        public static readonly CreatureAbility Ignores30PercentOfEnemyAttackValue = new("Ignores 30% of enemy attack value", 110);
        public static readonly CreatureAbility Ignores60PercentOfEnemyAttackValue = new("Ignores 60% of enemy attack value", 111);
        public static readonly CreatureAbility Revenge = new("Revenge", 112);
        public static readonly CreatureAbility PositiveLuck = new("Positive Luck", 113);
        public static readonly CreatureAbility RangedAttackIgnores20PercentOfEnemysDefenseSkill = new("Ranged attack ignores 20% of enemy's defense skill", 114);
        public static readonly CreatureAbility Repairing10HPPerUnit = new("Repairing (10 HP per unit)", 115);
        public static readonly CreatureAbility Repairing20HPPerUnit = new("Repairing (20 HP per unit)", 116);
        public static readonly CreatureAbility Mechanical = new("Mechanical", 117);
        public static readonly CreatureAbility Detonation = new("Detonation", 118);
        public static readonly CreatureAbility Burrowing = new("Burrowing", 119);
        public static readonly CreatureAbility DevoursCorpses = new("Devours corpses", 120);
        public static readonly CreatureAbility PreemptiveShotOncePerRound = new("Preemptive shot (once per round)", 121);
        public static readonly CreatureAbility PreemptiveShotUnlimited = new("Preemptive shot (unlimited)", 122);
        public static readonly CreatureAbility MeditationSkippingATurn = new("Meditation (skipping a turn)", 123);
        public static readonly CreatureAbility MeditationWithoutSkippingATurn = new("Meditation (without skipping a turn)", 124);
        public static readonly CreatureAbility HeatStroke = new("Heat stroke", 125);
        public static readonly CreatureAbility Generates1GoldPerUnitPerDay = new("Generates 1 Gold per unit per day", 126);
        public static readonly CreatureAbility AdjacentEnemiesReceivePlus20PercentSpellDamage = new("Adjacent enemies receive +20% Spell Damage", 127);
        public static readonly CreatureAbility MayShootWhileAdjacentToEnemyCreatures = new("May shoot while adjacent to enemy creatures", 128);
        public static readonly CreatureAbility RecoversFromNegativeEffectsInOneRoundOfCombat = new("Recovers from negative effects in one round of combat", 129);
        public static readonly CreatureAbility PossessesRunes = new("Possesses Runes", 130);
        public static readonly CreatureAbility SpellcasterAirShield = new("Spellcaster (Air Shield)", 131);
        public static readonly CreatureAbility FreezingShot = new("Freezing Shot", 132);
        public static readonly CreatureAbility ReceivesPlus100PercentDefenseWhileDefendingInCombat = new("Receives +100% Defense while defending in combat", 133);
        public static readonly CreatureAbility TeleportsAlliesOncePerCombat = new("Teleports allies (once per combat)", 134);
        public static readonly CreatureAbility Minus1EnemyFlyingUnitsSpeed = new("-1 enemy flying units' Speed", 135);
        public static readonly CreatureAbility TeleportsAlliesUnlimited = new("Teleports allies (unlimited)", 136);
        public static readonly CreatureAbility Minus2EnemyFlyingUnitsSpeed = new("-2 enemy flying units' Speed", 137);

        private CreatureAbility(string name, byte code)
            : base(name, code)
        {
        }

        public static IReadOnlyList<CreatureAbility> All => GetAll<CreatureAbility>();
    }
}
