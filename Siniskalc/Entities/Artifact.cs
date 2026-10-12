namespace Siniskalc.Entities
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Static collection of all artifacts and their properties up to hota v1.8.1.
    /// </summary>
    public sealed class Artifact : Homm3Enumeration
    {
        public static readonly Artifact HelmOfTheAlabasterUnicorn = new("Helm of the Alabaster Unicorn", 0x13, ArtifactClass.Treasure, 1000, ArtifactSlots.Helmet, new() { Kno = 1 });
        public static readonly Artifact SkullHelmet = new("Skull Helmet", 0x14, ArtifactClass.Treasure, 3000, ArtifactSlots.Helmet, new() { Kno = 2 }, 0x84);
        public static readonly Artifact HelmOfChaos = new("Helm of Chaos", 0x15, ArtifactClass.Minor, 4000, ArtifactSlots.Helmet, new() { Kno = 3 });
        public static readonly Artifact CrownOfTheSupremeMagi = new("Crown of the Supreme Magi", 0x16, ArtifactClass.Minor, 5000, ArtifactSlots.Helmet, new() { Kno = 4 }, 0x8F);
        public static readonly Artifact HellstormHelmet = new("Hellstorm Helmet", 0x17, ArtifactClass.Major, 6000, ArtifactSlots.Helmet, new() { Kno = 5 });
        public static readonly Artifact ThunderHelmet = new("Thunder Helmet", 0x18, ArtifactClass.Relic, 10000, ArtifactSlots.Helmet, new() { Pow = -2, Kno = 10 }, 0x87);
        public static readonly Artifact HelmOfHeavenlyEnlightenment = new("Helm of Heavenly Enlightenment", 0x24, ArtifactClass.Relic, 24000, ArtifactSlots.Helmet, new() { Atk = 6, Def = 6, Pow = 6, Kno = 6 }, 0x81);
        public static readonly Artifact CrownOfDragontooth = new("Crown of Dragontooth", 0x2C, ArtifactClass.Relic, 8000, ArtifactSlots.Helmet, new() { Pow = 4, Kno = 4 }, combinesInto: 0x86);
        public static readonly Artifact SeaCaptainsHat = new("Sea Captain's Hat", 0x7B, ArtifactClass.Relic, 15000, ArtifactSlots.Helmet, new() { SpellGrants = [SpellGrantProvider.Of(Spell.SummonBoat, Spell.ScuttleBoat)], WaterMovement = 500, Other = "Can Cast Summon Boat, Scuttle Boat, Protection from Whirlpools" }, 0x88);
        public static readonly Artifact SpellbindersHat = new("Spellbinder's Hat", 0x7C, ArtifactClass.Relic, 30000, ArtifactSlots.Helmet, new() { SpellGrants = [SpellGrantProvider.OfLevel(5)], Other = "When equipped, the hat enables the hero to cast all 5th level spells." });
        public static readonly Artifact AdmiralsHat = new("Admiral's Hat", 0x88, ArtifactClass.Relic, 25000, ArtifactSlots.Helmet, ArtifactSlots.Helmet | ArtifactSlots.Necklace, new() { SpellGrants = [SpellGrantProvider.Of(Spell.SummonBoat, Spell.ScuttleBoat)], WaterMovement = 1500, Other = "No Penalty to Board/Leave Boat, Can Cast Summon Boat and Scuttle Boat, Protection from Whirlpools." }, GameVersion.ShadowOfDeath);
        public static readonly Artifact CrownOfTheFiveSeas = new("Crown of the Five Seas", 150, ArtifactClass.Major, 7000, ArtifactSlots.Helmet, new() { Kno = 6 }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact AmuletOfTheUndertaker = new("Amulet of the Undertaker", 0x36, ArtifactClass.Treasure, 2000, ArtifactSlots.Necklace, new() { Other = "Necromancy +5%" }, 0x82);
        public static readonly Artifact CelestialNecklaceOfBliss = new("Celestial Necklace of Bliss", 0x21, ArtifactClass.Relic, 12000, ArtifactSlots.Necklace, new() { Atk = 3, Def = 3, Pow = 3, Kno = 3 }, 0x81);
        public static readonly Artifact CollarOfConjuring = new("Collar of Conjuring", 0x4C, ArtifactClass.Treasure, 500, ArtifactSlots.Necklace, new() { Other = "Spell Duration +1" }, 0x8B);
        public static readonly Artifact GarnitureOfInterference = new("Garniture of Interference", 0x39, ArtifactClass.Major, 2000, ArtifactSlots.Necklace, new() { Other = "Magic Resistance 5%" }, 0x8E);
        public static readonly Artifact NecklaceOfDragonteeth = new("Necklace of Dragonteeth", 0x2B, ArtifactClass.Major, 6000, ArtifactSlots.Necklace, new() { Pow = 3, Kno = 3 }, combinesInto: 0x86);
        public static readonly Artifact NecklaceOfOceanGuidance = new("Necklace of Ocean Guidance", 0x47, ArtifactClass.Major, 10000, ArtifactSlots.Necklace, new() { WaterMovement = 1000 }, 0x88);
        public static readonly Artifact NecklaceOfSwiftness = new("Necklace of Swiftness", 0x61, ArtifactClass.Treasure, 5000, ArtifactSlots.Necklace, new() { Other = "+1 Unit Speed" });
        public static readonly Artifact PendantOfCourage = new("Pendant of Courage", 0x6C, ArtifactClass.Major, 7000, ArtifactSlots.Necklace, new() { Morale = 3, Luck = 3 });
        public static readonly Artifact PendantOfDeath = new("Pendant of Death", 0x68, ArtifactClass.Treasure, 2500, ArtifactSlots.Necklace, new() { Other = "Immunity to Destroy Undead" });
        public static readonly Artifact PendantOfDispassion = new("Pendant of Dispassion", 0x64, ArtifactClass.Treasure, 1000, ArtifactSlots.Necklace, new() { Other = "Immunity to Berserk" });
        public static readonly Artifact PendantOfFreeWill = new("Pendant of Free Will", 0x69, ArtifactClass.Treasure, 1000, ArtifactSlots.Necklace, new() { Other = "Immunity to Hypnotize" });
        public static readonly Artifact PendantOfHoliness = new("Pendant of Holiness", 0x66, ArtifactClass.Treasure, 1000, ArtifactSlots.Necklace, new() { Other = "Immunity to Curse" });
        public static readonly Artifact PendantOfLife = new("Pendant of Life", 0x67, ArtifactClass.Treasure, 2500, ArtifactSlots.Necklace, new() { Other = "Immunity to Death Ripple" });
        public static readonly Artifact PendantOfNegativity = new("Pendant of Negativity", 0x6A, ArtifactClass.Major, 5000, ArtifactSlots.Necklace, new() { Other = "Immunity to Lightning Bolt and Chain-Lightning" });
        public static readonly Artifact PendantOfSecondSight = new("Pendant of Second Sight", 0x65, ArtifactClass.Major, 5000, ArtifactSlots.Necklace, new() { Other = "Immunity to Blind" });
        public static readonly Artifact PendantOfTotalRecall = new("Pendant of Total Recall", 0x6B, ArtifactClass.Treasure, 3000, ArtifactSlots.Necklace, new() { Other = "Immunity to Forgetfulness" });
        public static readonly Artifact StatesmensMedal = new("Statesmen's Medal", 0x42, ArtifactClass.Major, 5000, ArtifactSlots.Necklace, new() { Other = "Surrendering Cost -10%" }, 0x8D);
        public static readonly Artifact PendantOfDownfall = new("Pendant of Downfall", 157, ArtifactClass.Major, 7000, ArtifactSlots.Necklace, new() { Other = "Decreases enemy's Morale by 2" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact PendantOfReflection = new("Pendant of Reflection", 0x8E, ArtifactClass.Relic, 12000, ArtifactSlots.Necklace, ArtifactSlots.Necklace | ArtifactSlots.Cape | ArtifactSlots.Feet, new() { Other = "Magic resistance +20%. Components effect: Magic resistance +30%" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact ArmorOfTheDamned = new("Armor of the Damned", 0x84, ArtifactClass.Relic, 12000, ArtifactSlots.Torso, ArtifactSlots.Torso | ArtifactSlots.Helmet | ArtifactSlots.Weapon | ArtifactSlots.Shield, new() { Atk = 3, Def = 3, Pow = 2, Kno = 2, Other = "Cast Slow,Curse,Weakness and Misfortune for 50 rounds in combat." }, GameVersion.ShadowOfDeath);
        public static readonly Artifact ArmorOfWonder = new("Armor of Wonder", 0x1F, ArtifactClass.Minor, 4000, ArtifactSlots.Torso, new() { Atk = 1, Def = 1, Pow = 1, Kno = 1 }, 0x81);
        public static readonly Artifact BreastplateOfBrimstone = new("Breastplate of Brimstone", 0x1D, ArtifactClass.Major, 6000, ArtifactSlots.Torso, new() { Pow = 5 }, 0x80);
        public static readonly Artifact BreastplateOfPetrifiedWood = new("Breastplate of Petrified Wood", 0x19, ArtifactClass.Treasure, 1000, ArtifactSlots.Torso, new() { Pow = 1 });
        public static readonly Artifact DragonScaleArmor = new("Dragon Scale Armor", 0x28, ArtifactClass.Relic, 8000, ArtifactSlots.Torso, new() { Atk = 4, Def = 4 }, combinesInto: 0x86);
        public static readonly Artifact PowerOfTheDragonFather = new("Power of the Dragon Father", 0x86, ArtifactClass.Relic, 42000, ArtifactSlots.Torso, ArtifactSlots.Helmet | ArtifactSlots.Necklace | ArtifactSlots.Torso | ArtifactSlots.Cape | ArtifactSlots.Feet | ArtifactSlots.Weapon | ArtifactSlots.Shield | ArtifactSlots.Ring1 | ArtifactSlots.Ring2, new() { Atk = 16, Def = 16, Pow = 16, Kno = 16, Morale = 1, Luck = 1, Other = "Immune to Lvl 1-4 Spells" }, GameVersion.ShadowOfDeath);
        public static readonly Artifact RibCage = new("Rib Cage", 0x1A, ArtifactClass.Minor, 3000, ArtifactSlots.Torso, new() { Pow = 2 }, 0x84);
        public static readonly Artifact ScalesOfTheGreaterBasilisk = new("Scales of the Greater Basilisk", 0x1B, ArtifactClass.Minor, 4000, ArtifactSlots.Torso, new() { Pow = 3 });
        public static readonly Artifact TitansCuirass = new("Titan's Cuirass", 0x1E, ArtifactClass.Relic, 10000, ArtifactSlots.Torso, new() { Pow = 10, Kno = -2 }, 0x87);
        public static readonly Artifact TunicOfTheCyclopsKing = new("Tunic of the Cyclops King", 0x1C, ArtifactClass.Major, 5000, ArtifactSlots.Torso, new() { Pow = 4 }, 0x8F);
        public static readonly Artifact PlateOfDyingLight = new("Plate of Dying Light", 164, ArtifactClass.Relic, 10000, ArtifactSlots.Torso, new() { Other = "Reduces the Power skill of enemy hero by 25% during combat" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact RoyalArmorOfNix = new("Royal Armor of Nix", 149, ArtifactClass.Major, 7000, ArtifactSlots.Torso, new() { Other = "Increases Spell Power skill by 6" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact AmbassadorsSash = new("Ambassador's Sash", 0x44, ArtifactClass.Major, 5000, ArtifactSlots.Cape, new() { Other = "Surrendering Cost -10%" }, 0x8D);
        public static readonly Artifact AngelWings = new("Angel Wings", 0x48, ArtifactClass.Relic, 20000, ArtifactSlots.Cape, new() { Other = "Permanent Expert Fly when equipped." });
        public static readonly Artifact CapeOfConjuring = new("Cape of Conjuring", 0x4E, ArtifactClass.Treasure, 1500, ArtifactSlots.Cape, new() { Other = "Spell Duration +3" }, 0x8B);
        public static readonly Artifact CapeOfVelocity = new("Cape of Velocity", 0x63, ArtifactClass.Major, 10000, ArtifactSlots.Cape, new() { Other = "+2 Unit Speed" });
        public static readonly Artifact CloakOfUndeadKing = new("Cloak of Undead King", 0x82, ArtifactClass.Relic, 12000, ArtifactSlots.Cape, ArtifactSlots.Cape | ArtifactSlots.Necklace | ArtifactSlots.Feet, new() { Other = "Necromancy +60%, Raise more Creature Types" }, GameVersion.ShadowOfDeath);
        public static readonly Artifact DragonWingTabard = new("Dragon Wing Tabard", 0x2A, ArtifactClass.Minor, 4000, ArtifactSlots.Cape, new() { Pow = 2, Kno = 2 }, combinesInto: 0x86);
        public static readonly Artifact EverflowingCrystalCloak = new("Everflowing Crystal Cloak", 0x6D, ArtifactClass.Major, 5000, ArtifactSlots.Cape, new() { Crystal = 1 }, combinesInto: 0x8C);
        public static readonly Artifact RecantersCloak = new("Recanter's Cloak", 0x53, ArtifactClass.Major, 8000, ArtifactSlots.Cape, new() { Other = "Prevents Casting lvl 3+ Spells" });
        public static readonly Artifact SurcoatOfCounterpoise = new("Surcoat of Counterpoise", 0x3A, ArtifactClass.Major, 4000, ArtifactSlots.Cape, new() { Other = "Magic Resistance 10%" }, 0x8E);
        public static readonly Artifact VampiresCowl = new("Vampire's Cowl", 0x37, ArtifactClass.Minor, 4000, ArtifactSlots.Cape, new() { Other = "Necromancy 10%" }, 0x82);
        public static readonly Artifact CapeOfSilence = new("Cape of Silence", 159, ArtifactClass.Major, 7500, ArtifactSlots.Cape, new() { Other = "Bans all level 1 and 2 spells in battle" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact DiplomatsCloak = new("Diplomat's Cloak", 0x8D, ArtifactClass.Relic, 15000, ArtifactSlots.Cape, ArtifactSlots.Cape | ArtifactSlots.Necklace | ArtifactSlots.Ring1, new() { Other = "Allows your hero to retreat or surrender when battling neutral monsters or defending a town. Multiplies your hero army strength by 3" });
        public static readonly Artifact BootsOfLevitation = new("Boots of Levitation", 0x5A, ArtifactClass.Relic, 10000, ArtifactSlots.Feet, new() { Other = "Permanent Expert Water Walk when equipped." });
        public static readonly Artifact BootsOfPolarity = new("Boots of Polarity", 0x3B, ArtifactClass.Relic, 6000, ArtifactSlots.Feet, new() { Other = "Magic Resistance 15%" }, 0x8E);
        public static readonly Artifact BootsOfSpeed = new("Boots of Speed", 0x62, ArtifactClass.Minor, 6000, ArtifactSlots.Feet, new() { LandMovement = 600 });
        public static readonly Artifact DeadMensBoots = new("Dead Men's Boots", 0x38, ArtifactClass.Major, 6000, ArtifactSlots.Feet, new() { Other = "Necromancy 15%" }, 0x82);
        public static readonly Artifact DragonboneGreaves = new("Dragonbone Greaves", 0x29, ArtifactClass.Treasure, 2000, ArtifactSlots.Feet, new() { Pow = 1, Kno = 1 }, combinesInto: 0x86);
        public static readonly Artifact SandalsOfTheSaint = new("Sandals of the Saint", 0x20, ArtifactClass.Relic, 8000, ArtifactSlots.Feet, new() { Atk = 2, Def = 2, Pow = 2, Kno = 2 }, 0x81);
        public static readonly Artifact WayfarersBoots = new("Wayfarer's Boots", 151, ArtifactClass.Major, 5000, ArtifactSlots.Feet, new() { Other = "Reduces the cost of moving one tile to 100 on any terrain." }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact AngelicAlliance = new("Angelic Alliance", 0x81, ArtifactClass.Relic, 84000, ArtifactSlots.Weapon, ArtifactSlots.Weapon | ArtifactSlots.Helmet | ArtifactSlots.Necklace | ArtifactSlots.Torso | ArtifactSlots.Feet | ArtifactSlots.Shield, new() { Atk = 21, Def = 21, Pow = 21, Kno = 21, Other = "Allows Castle, Rampart, Tower, Fortress, Stronghold, Conflux, Cove, Factory, and Bulwark creatures to be mixed without a morale penalty. Casts Expert Prayer for 10 rounds at the start of combat." }, GameVersion.ShadowOfDeath);
        public static readonly Artifact BlackshardOfTheDeadKnight = new("Blackshard of the Dead Knight", 0x08, ArtifactClass.Minor, 3000, ArtifactSlots.Weapon, new() { Atk = 3 }, 0x84);
        public static readonly Artifact CentaursAxe = new("Centaur's Axe", 0x07, ArtifactClass.Treasure, 2000, ArtifactSlots.Weapon, new() { Atk = 2 });
        public static readonly Artifact GreaterKnollsFlail = new("Greater Knoll's Flail", 0x09, ArtifactClass.Minor, 4000, ArtifactSlots.Weapon, new() { Atk = 4 });
        public static readonly Artifact OgresClubOfHavoc = new("Ogre's Club of Havoc", 0x0A, ArtifactClass.Major, 5000, ArtifactSlots.Weapon, new() { Atk = 5 }, 0x8F);
        public static readonly Artifact RedDragonFlameTongue = new("Red Dragon Flame Tongue", 0x26, ArtifactClass.Minor, 4000, ArtifactSlots.Weapon, new() { Atk = 2, Def = 2 }, combinesInto: 0x86);
        public static readonly Artifact SwordOfHellfire = new("Sword of Hellfire", 0x0B, ArtifactClass.Major, 6000, ArtifactSlots.Weapon, new() { Atk = 6 }, 0x80);
        public static readonly Artifact SwordOfJudgement = new("Sword of Judgement", 0x23, ArtifactClass.Relic, 20000, ArtifactSlots.Weapon, new() { Atk = 5, Def = 5, Pow = 5, Kno = 5 }, 0x81);
        public static readonly Artifact TitansGladius = new("Titan's Gladius", 0x0C, ArtifactClass.Relic, 10000, ArtifactSlots.Weapon, new() { Atk = 12, Def = -3 }, 0x87);
        public static readonly Artifact TitansThunder = new("Titan's Thunder", 0x87, ArtifactClass.Relic, 40000, ArtifactSlots.Weapon, ArtifactSlots.Weapon | ArtifactSlots.Helmet | ArtifactSlots.Torso | ArtifactSlots.Shield, new() { GrantsSpellBook = true, SpellGrants = [SpellGrantProvider.Of(Spell.TitansLightningBolt)], Atk = 9, Def = 9, Pow = 8, Kno = 8, Other = "Gain Titan's Lightning Bolt while equipped. Gain a Spell Book." }, GameVersion.ShadowOfDeath);
        public static readonly Artifact ArmageddonsBlade = new("Armageddon's Blade", 0x80, ArtifactClass.Relic, 50000, ArtifactSlots.Weapon, new() { SpellGrants = [SpellGrantProvider.Of(Spell.Armageddon)], Atk = 3, Def = 3, Pow = 3, Kno = 6, Other = "Can Cast Armageddon, Immune to Armageddon" }, GameVersion.ArmageddonsBlade);
        public static readonly Artifact IronfistOfTheOgre = new("Ironfist of the Ogre", 0x8F, ArtifactClass.Relic, 20000, ArtifactSlots.Weapon, ArtifactSlots.Weapon | ArtifactSlots.Helmet | ArtifactSlots.Torso | ArtifactSlots.Shield, new() { Atk = 5, Def = 4, Pow = 4, Kno = 4, Other = "Casts Expert Haste, Bloodlust, Fire Shield, and Counterstrike for 50 rounds at the start of combat." }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact TridentOfDomination = new("Trident of Domination", 147, ArtifactClass.Major, 7000, ArtifactSlots.Weapon, new() { Atk = 7 });
        public static readonly Artifact BucklerOfTheGnollKing = new("Buckler of the Gnoll King", 0x0F, ArtifactClass.Minor, 4000, ArtifactSlots.Shield, new() { Def = 4 });
        public static readonly Artifact DragonScaleShield = new("Dragon Scale Shield", 0x27, ArtifactClass.Major, 6000, ArtifactSlots.Shield, new() { Atk = 3, Def = 3 }, combinesInto: 0x86);
        public static readonly Artifact LionsShieldOfCourage = new("Lion's Shield of Courage", 0x22, ArtifactClass.Relic, 16000, ArtifactSlots.Shield, new() { Atk = 4, Def = 4, Pow = 4, Kno = 4 }, 0x81);
        public static readonly Artifact SentinelsShield = new("Sentinel's Shield", 0x12, ArtifactClass.Relic, 10000, ArtifactSlots.Shield, new() { Atk = -3, Def = 12 }, 0x87);
        public static readonly Artifact ShieldOfTheDamned = new("Shield of the Damned", 0x11, ArtifactClass.Major, 6000, ArtifactSlots.Shield, new() { Def = 6 }, 0x80);
        public static readonly Artifact ShieldOfTheDwarvenLords = new("Shield of the Dwarven Lords", 0x0D, ArtifactClass.Treasure, 2000, ArtifactSlots.Shield, new() { Def = 2 });
        public static readonly Artifact ShieldOfTheYawningDead = new("Shield of the Yawning Dead", 0x0E, ArtifactClass.Minor, 3000, ArtifactSlots.Shield, new() { Def = 3 });
        public static readonly Artifact TargOfTheRampagingOgre = new("Targ of the Rampaging Ogre", 0x10, ArtifactClass.Major, 5000, ArtifactSlots.Shield, new() { Def = 5 }, 0x8F);
        public static readonly Artifact ShieldOfNavalGlory = new("Shield of Naval Glory", 148, ArtifactClass.Major, 7000, ArtifactSlots.Shield, new() { Def = 7 }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact DiplomatRing = new("Diplomat Ring", 0x43, ArtifactClass.Major, 5000, ArtifactSlots.Rings, new() { Other = "Surrendering Cost -10%" });
        public static readonly Artifact EquestriansGloves = new("Equestrian's Gloves", 0x46, ArtifactClass.Minor, 3000, ArtifactSlots.Rings, new() { LandMovement = 300 });
        public static readonly Artifact EversmokingRingOfSulfur = new("Eversmoking Ring of Sulfur", 0x71, ArtifactClass.Major, 5000, ArtifactSlots.Rings, new() { Sulfur = 1 }, combinesInto: 0x8C);
        public static readonly Artifact QuietEyeOfTheDragon = new("Quiet Eye of the Dragon", 0x25, ArtifactClass.Treasure, 2000, ArtifactSlots.Rings, new() { Atk = 1, Def = 1 }, combinesInto: 0x86);
        public static readonly Artifact RingOfConjuring = new("Ring of Conjuring", 0x4D, ArtifactClass.Treasure, 1000, ArtifactSlots.Rings, new() { Other = "Spell Duration +2" }, 0x8B);
        public static readonly Artifact RingOfInfiniteGems = new("Ring of Infinite Gems", 0x6E, ArtifactClass.Major, 5000, ArtifactSlots.Rings, new() { Gems = 1 }, combinesInto: 0x8C);
        public static readonly Artifact RingOfLife = new("Ring of Life", 0x5F, ArtifactClass.Minor, 5000, ArtifactSlots.Rings, new() { Other = "+1 Unit Health" }, 0x83);
        public static readonly Artifact RingOfTheMagi = new("Ring of the Magi", 0x8B, ArtifactClass.Relic, 3000, ArtifactSlots.Ring1, ArtifactSlots.Ring1 | ArtifactSlots.Necklace | ArtifactSlots.Cape, new() { Other = "Add 50+6 rounds to spell duration" }, GameVersion.ShadowOfDeath);
        public static readonly Artifact RingOfTheWayfarer = new("Ring of the Wayfarer", 0x45, ArtifactClass.Major, 5000, ArtifactSlots.Rings, new() { Other = "Unit Speed +1" });
        public static readonly Artifact RingOfVitality = new("Ring of Vitality", 0x5E, ArtifactClass.Treasure, 5000, ArtifactSlots.Rings, new() { Other = "+1 Unit Health" }, 0x83);
        public static readonly Artifact StillEyeOfTheDragon = new("Still Eye of the Dragon", 0x2D, ArtifactClass.Treasure, 2000, ArtifactSlots.Rings, new() { Morale = 1, Luck = 1 }, combinesInto: 0x86);
        public static readonly Artifact RingOfOblivion = new("Ring of Oblivion", 158, ArtifactClass.Major, 7500, ArtifactSlots.Rings, new() { Other = "Makes all losses in the battle irrevocable" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact RingOfSuppression = new("Ring of Suppression", 156, ArtifactClass.Treasure, 4000, ArtifactSlots.Rings, new() { Other = "Decreases enemy's Morale by 1" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact SealOfSunset = new("Seal of Sunset", 163, ArtifactClass.Minor, 5000, ArtifactSlots.Rings, new() { Other = "Reduces the Power skill of enemy hero by 10% during combat" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact SpellScroll = new("Spell Scroll", 0x01, ArtifactClass.Special, 0, ArtifactSlots.Misc, default);
        public static readonly Artifact AngelFeatherArrows = new("Angel Feather Arrows", 0x3E, ArtifactClass.Major, 6000, ArtifactSlots.Misc, new() { Other = "Archery Skill 15%" }, 0x89);
        public static readonly Artifact ArmsOfLegion = new("Arms of Legion", 0x79, ArtifactClass.Major, 5000, ArtifactSlots.Misc, new() { Other = "Lvl 5 Creature Growth +2" }, 0x85);
        public static readonly Artifact BadgeOfCourage = new("Badge of Courage", 0x31, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Morale = 1 });
        public static readonly Artifact BirdOfPerception = new("Bird of Perception", 0x3F, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Other = "Eagle Eye Skill 5%" });
        public static readonly Artifact BowOfElvenCherrywood = new("Bow of Elven Cherrywood", 0x3C, ArtifactClass.Treasure, 2000, ArtifactSlots.Misc, new() { Other = "Archery Skill 5%" }, 0x89);
        public static readonly Artifact BowOfTheSharpshooter = new("Bow of the Sharpshooter", 0x89, ArtifactClass.Relic, 12000, ArtifactSlots.Misc1, ArtifactSlots.Misc1 | ArtifactSlots.Misc2 | ArtifactSlots.Misc3, new() { Other = "Shooters may shoot while adjacent to enemy creatures, no range and obstacle Penalty, no melee Penalty, Archery Skill +30%" }, GameVersion.ShadowOfDeath);
        public static readonly Artifact BowstringOfTheUnicornsMane = new("Bowstring of the Unicorn's Mane", 0x3D, ArtifactClass.Minor, 4000, ArtifactSlots.Misc, new() { Other = "Archery Skill 10%" }, 0x89);
        public static readonly Artifact CardsOfProphecy = new("Cards of Prophecy", 0x2F, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Luck = 1 });
        public static readonly Artifact CharmOfSpellPoints = new("Charm of Spell Points", 0x49, ArtifactClass.Treasure, 500, ArtifactSlots.Misc, new() { Other = "Regenerates +1 Spell Points per day" }, 0x8A);
        public static readonly Artifact CloverOfFortune = new("Clover of Fortune", 0x2E, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Luck = 1 });
        public static readonly Artifact Cornucopia = new("Cornucopia", 0x8C, ArtifactClass.Relic, 20000, ArtifactSlots.Misc1, ArtifactSlots.Misc1 | ArtifactSlots.Cape | ArtifactSlots.Ring1 | ArtifactSlots.Ring2, new() { Mercury = 5, Sulfur = 5, Crystal = 5, Gems = 5, Other = "Generates 5 of each precious resource, each day." }, GameVersion.ShadowOfDeath);
        public static readonly Artifact CrestOfValor = new("Crest of Valor", 0x32, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Morale = 1 });
        public static readonly Artifact ElixirOfLife = new("Elixir of Life", 0x83, ArtifactClass.Relic, 20000, ArtifactSlots.Misc1, ArtifactSlots.Misc1 | ArtifactSlots.Ring1 | ArtifactSlots.Ring2, new() { Other = "+25% Unit Health, +4 Health Point Regeneration" }, GameVersion.ShadowOfDeath);
        public static readonly Artifact EmblemOfCognizance = new("Emblem of Cognizance", 0x41, ArtifactClass.Minor, 3000, ArtifactSlots.Misc, new() { Other = "Eagle Eye Skill 15%" });
        public static readonly Artifact EndlessBagOfGold = new("Endless Bag of Gold", 0x74, ArtifactClass.Major, 7500, ArtifactSlots.Misc, new() { Gold = 750 }, combinesInto: 160);
        public static readonly Artifact EndlessPurseOfGold = new("Endless Purse of Gold", 0x75, ArtifactClass.Major, 5000, ArtifactSlots.Misc, new() { Gold = 500 }, combinesInto: 160);
        public static readonly Artifact EndlessSackOfGold = new("Endless Sack of Gold", 0x73, ArtifactClass.Relic, 10000, ArtifactSlots.Misc, new() { Gold = 1000 }, combinesInto: 160);
        public static readonly Artifact EverpouringVialOfMercury = new("Everpouring Vial of Mercury", 0x6F, ArtifactClass.Major, 5000, ArtifactSlots.Misc, new() { Mercury = 1 }, combinesInto: 0x8C);
        public static readonly Artifact GlyphOfGallantry = new("Glyph of Gallantry", 0x33, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Morale = 1 });
        public static readonly Artifact GoldenBow = new("Golden Bow", 0x5B, ArtifactClass.Major, 8000, ArtifactSlots.Misc, new() { Other = "No Range and Obstacle Penalty" });
        public static readonly Artifact HeadOfLegion = new("Head of Legion", 0x7A, ArtifactClass.Major, 5000, ArtifactSlots.Misc, new() { Other = "Lvl 6 Creature Growth +1" }, 0x85);
        public static readonly Artifact HourglassOfTheEvilHour = new("Hourglass of the Evil Hour", 0x55, ArtifactClass.Treasure, 2000, ArtifactSlots.Misc, new() { Other = "Negates ally and enemy Luck bonuses." });
        public static readonly Artifact InexhaustibleCartOfOre = new("Inexhaustible Cart of Ore", 0x70, ArtifactClass.Minor, 5000, ArtifactSlots.Misc, new() { Ore = 1 });
        public static readonly Artifact InexhaustibleCartOfLumber = new("Inexhaustible Cart of Lumber", 0x72, ArtifactClass.Minor, 5000, ArtifactSlots.Misc, new() { Wood = 1 });
        public static readonly Artifact LadybirdOfLuck = new("Ladybird of Luck", 0x30, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Luck = 1 });
        public static readonly Artifact LegsOfLegion = new("Legs of Legion", 0x76, ArtifactClass.Treasure, 5000, ArtifactSlots.Misc, new() { Other = "Lvl 2 Creature Growth +5" }, 0x85);
        public static readonly Artifact LoinsOfLegion = new("Loins of Legion", 0x77, ArtifactClass.Minor, 5000, ArtifactSlots.Misc, new() { Other = "Lvl 3 Creature Growth +4" }, 0x85);
        public static readonly Artifact MysticOrbOfSpellPoints = new("Mystic Orb of Spell Points", 0x4B, ArtifactClass.Treasure, 1500, ArtifactSlots.Misc, new() { Other = "Regenerates +3 Spell Points per day" }, 0x8A);
        public static readonly Artifact OrbOfDrivingRain = new("Orb of Driving Rain", 0x52, ArtifactClass.Major, 6000, ArtifactSlots.Misc, new() { Other = "All Water Spell Damage +50%" });
        public static readonly Artifact OrbOfInhibition = new("Orb of Inhibition", 0x7E, ArtifactClass.Relic, 20000, ArtifactSlots.Misc, new() { Other = "Prevents Casting All Spells" });
        public static readonly Artifact OrbOfSilt = new("Orb of Silt", 0x50, ArtifactClass.Major, 6000, ArtifactSlots.Misc, new() { Other = "All Earth Spell Damage +50%" });
        public static readonly Artifact OrbOfTempestousFire = new("Orb of Tempestous Fire", 0x51, ArtifactClass.Major, 6000, ArtifactSlots.Misc, new() { Other = "All Fire Spell Damage +50%" });
        public static readonly Artifact OrbOfFirmament = new("Orb of Firmament", 0x4F, ArtifactClass.Special, 0, ArtifactSlots.Misc, new() { Other = "All Air Spell Damage +50%" });
        public static readonly Artifact OrbOfVulnerability = new("Orb of Vulnerability", 0x5D, ArtifactClass.Relic, 25000, ArtifactSlots.Misc, new() { Other = "Negate natural magic resistance, magic immunity and chances to resist spells of all creatures on the battlefield. Does not affect the limitations on applicability stated in spell descriptions as well as the immunities given by artifacts." });
        public static readonly Artifact ShacklesOfWar = new("Shackles of War", 0x7D, ArtifactClass.Major, 5000, ArtifactSlots.Misc, new() { Other = "Neither you nor your opponent may flee or surrender" });
        public static readonly Artifact Speculum = new("Speculum", 0x34, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Other = "Scouting Radius +1" });
        public static readonly Artifact SphereOfPermanence = new("Sphere of Permanence", 0x5C, ArtifactClass.Major, 7500, ArtifactSlots.Misc, new() { Other = "Immune to Dispel" });
        public static readonly Artifact SpiritOfOppression = new("Spirit of Oppression", 0x54, ArtifactClass.Treasure, 2000, ArtifactSlots.Misc, new() { Other = "Positive Morale Disabled" });
        public static readonly Artifact Spyglass = new("Spyglass", 0x35, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Other = "Scouting Radius +1" });
        public static readonly Artifact StatueOfLegion = new("Statue of Legion", 0x85, ArtifactClass.Relic, 25000, ArtifactSlots.Misc, ArtifactSlots.Misc1 | ArtifactSlots.Misc2 | ArtifactSlots.Misc3 | ArtifactSlots.Misc4 | ArtifactSlots.Misc5, new() { Other = "Creature Growth +50% base in all towns + Artifact Effects" }, GameVersion.ShadowOfDeath);
        public static readonly Artifact StoicWatchman = new("Stoic Watchman", 0x40, ArtifactClass.Treasure, 2000, ArtifactSlots.Misc, new() { Other = "Eagle Eye Skill 10%" });
        public static readonly Artifact TalismanOfSpellPoints = new("Talisman of Spell Points", 0x4A, ArtifactClass.Treasure, 1000, ArtifactSlots.Misc, new() { Other = "Regenerates +2 Spell Points per day" }, 0x8A);
        public static readonly Artifact TomeOfAirMagic = new("Tome of Wind Magic", 0x57, ArtifactClass.Relic, 20000, ArtifactSlots.Misc, new() { SpellGrants = [SpellGrantProvider.OfSchool(SpellSchool.Wind)], Other = "When equipped, all air spells are available" });
        public static readonly Artifact TomeOfEarthMagic = new("Tome of Earth Magic", 0x59, ArtifactClass.Relic, 20000, ArtifactSlots.Misc, new() { SpellGrants = [SpellGrantProvider.OfSchool(SpellSchool.Earth)], Other = "When equipped, all earth spells are available" });
        public static readonly Artifact TomeOfFireMagic = new("Tome of Fire Magic", 0x56, ArtifactClass.Relic, 20000, ArtifactSlots.Misc, new() { SpellGrants = [SpellGrantProvider.OfSchool(SpellSchool.Fire)], Other = "When equipped, all fire spells are available" });
        public static readonly Artifact TomeOfWaterMagic = new("Tome of Water Magic", 0x58, ArtifactClass.Relic, 20000, ArtifactSlots.Misc, new() { SpellGrants = [SpellGrantProvider.OfSchool(SpellSchool.Water)], Other = "When equipped, all water spells are available" });
        public static readonly Artifact TorsoOfLegion = new("Torso of Legion", 0x78, ArtifactClass.Minor, 5000, ArtifactSlots.Misc, new() { Other = "Lvl 4 Creature Growth +3" }, 0x85);
        public static readonly Artifact VialOfDragonblood = new("Vial of Dragonblood", 0x7F, ArtifactClass.Relic, 20000, ArtifactSlots.Misc, new() { Other = "Dragons receive +5 Attack and Defense" }, GameVersion.ArmageddonsBlade);
        public static readonly Artifact VialOfLifeblood = new("Vial of Lifeblood", 0x60, ArtifactClass.Major, 10000, ArtifactSlots.Misc, new() { Other = "+2 Unit Health" }, 0x83);
        public static readonly Artifact WizardsWell = new("Wizard's Well", 0x8A, ArtifactClass.Relic, 3000, ArtifactSlots.Misc1, ArtifactSlots.Misc1 | ArtifactSlots.Misc2 | ArtifactSlots.Misc3, new() { Other = "Regenerates all spell points each day" }, GameVersion.ShadowOfDeath);
        public static readonly Artifact CharmOfEclipse = new("Charm of Eclipse", 162, ArtifactClass.Minor, 5000, ArtifactSlots.Misc, new() { Other = "Reduces the Power skill of enemy hero by 10% during combat" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact DemonsHorseshoe = new("Demon's Horseshoe", 153, ArtifactClass.Treasure, 2000, ArtifactSlots.Misc, new() { Other = "Decreases enemy's Luck by 1" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact GoldenGoose = new("Golden Goose", 160, ArtifactClass.Relic, 22500, ArtifactSlots.Misc, ArtifactSlots.Misc1 | ArtifactSlots.Misc2 | ArtifactSlots.Misc3, new() { Other = "brings 7000 gold per day" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact HideousMask = new("Hideous Mask", 155, ArtifactClass.Minor, 4000, ArtifactSlots.Misc, new() { Other = "Decreases enemy's Morale by 1" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact HornOfTheAbyss = new("Horn of the Abyss", 161, ArtifactClass.Relic, 50000, ArtifactSlots.Misc, new() { Other = "After a stack of living creatures is slain, a stack of Fangarms will rise in their stead and will stay loyal to the hero after the battle concludes" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact RunesOfImminency = new("Runes of Imminency", 152, ArtifactClass.Treasure, 2000, ArtifactSlots.Misc, new() { Other = "Decreases enemy's Luck by 1" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact ShamansPuppet = new("Shaman's Puppet", 154, ArtifactClass.Minor, 4000, ArtifactSlots.Misc, new() { Other = "Decreases enemy's Luck by 2" }, GameVersion.HornOfTheAbyss);
        public static readonly Artifact SpellBook = new("Spell Book", 0x00, ArtifactClass.Special, 0, ArtifactSlots.None, default);
        public static readonly Artifact Catapult = new("Catapult", 0x03, ArtifactClass.Special, 0, ArtifactSlots.None, default);
        public static readonly Artifact TheGrail = new("The Grail", 0x02, ArtifactClass.Special, 0, ArtifactSlots.None, default);

        public static readonly IReadOnlyList<Artifact> Helmets =
        [
            HelmOfTheAlabasterUnicorn,
            SkullHelmet,
            HelmOfChaos,
            CrownOfTheSupremeMagi,
            HellstormHelmet,
            ThunderHelmet,
            HelmOfHeavenlyEnlightenment,
            CrownOfDragontooth,
            SeaCaptainsHat,
            SpellbindersHat,
            AdmiralsHat,
            CrownOfTheFiveSeas,
        ];

        public static readonly IReadOnlyList<Artifact> Necklaces =
        [
            AmuletOfTheUndertaker,
            CelestialNecklaceOfBliss,
            CollarOfConjuring,
            GarnitureOfInterference,
            NecklaceOfDragonteeth,
            NecklaceOfOceanGuidance,
            NecklaceOfSwiftness,
            PendantOfCourage,
            PendantOfDeath,
            PendantOfDispassion,
            PendantOfFreeWill,
            PendantOfHoliness,
            PendantOfLife,
            PendantOfNegativity,
            PendantOfSecondSight,
            PendantOfTotalRecall,
            StatesmensMedal,
            PendantOfDownfall,
            PendantOfReflection,
        ];

        public static readonly IReadOnlyList<Artifact> Torsos =
        [
            ArmorOfTheDamned,
            ArmorOfWonder,
            BreastplateOfBrimstone,
            BreastplateOfPetrifiedWood,
            DragonScaleArmor,
            PowerOfTheDragonFather,
            RibCage,
            ScalesOfTheGreaterBasilisk,
            TitansCuirass,
            TunicOfTheCyclopsKing,
            PlateOfDyingLight,
            RoyalArmorOfNix,
        ];

        public static readonly IReadOnlyList<Artifact> Capes =
        [
            AmbassadorsSash,
            AngelWings,
            CapeOfConjuring,
            CapeOfVelocity,
            CloakOfUndeadKing,
            DragonWingTabard,
            EverflowingCrystalCloak,
            RecantersCloak,
            SurcoatOfCounterpoise,
            VampiresCowl,
            CapeOfSilence,
            DiplomatsCloak,
        ];

        public static readonly IReadOnlyList<Artifact> Feet =
        [
            BootsOfLevitation,
            BootsOfPolarity,
            BootsOfSpeed,
            DeadMensBoots,
            DragonboneGreaves,
            SandalsOfTheSaint,
            WayfarersBoots,
        ];

        public static readonly IReadOnlyList<Artifact> Weapons =
        [
            AngelicAlliance,
            BlackshardOfTheDeadKnight,
            CentaursAxe,
            GreaterKnollsFlail,
            OgresClubOfHavoc,
            RedDragonFlameTongue,
            SwordOfHellfire,
            SwordOfJudgement,
            TitansGladius,
            TitansThunder,
            ArmageddonsBlade,
            IronfistOfTheOgre,
            TridentOfDomination,
        ];

        public static readonly IReadOnlyList<Artifact> Shields =
        [
            BucklerOfTheGnollKing,
            DragonScaleShield,
            LionsShieldOfCourage,
            SentinelsShield,
            ShieldOfTheDamned,
            ShieldOfTheDwarvenLords,
            ShieldOfTheYawningDead,
            TargOfTheRampagingOgre,
            ShieldOfNavalGlory,
        ];

        public static readonly IReadOnlyList<Artifact> Rings =
        [
            DiplomatRing,
            EquestriansGloves,
            EversmokingRingOfSulfur,
            QuietEyeOfTheDragon,
            RingOfConjuring,
            RingOfInfiniteGems,
            RingOfLife,
            RingOfTheMagi,
            RingOfTheWayfarer,
            RingOfVitality,
            StillEyeOfTheDragon,
            RingOfOblivion,
            RingOfSuppression,
            SealOfSunset,
        ];

        public static readonly IReadOnlyList<Artifact> Misc =
        [
            SpellScroll,
            AngelFeatherArrows,
            ArmsOfLegion,
            BadgeOfCourage,
            BirdOfPerception,
            BowOfElvenCherrywood,
            BowOfTheSharpshooter,
            BowstringOfTheUnicornsMane,
            CardsOfProphecy,
            CharmOfSpellPoints,
            CloverOfFortune,
            Cornucopia,
            CrestOfValor,
            ElixirOfLife,
            EmblemOfCognizance,
            EndlessBagOfGold,
            EndlessPurseOfGold,
            EndlessSackOfGold,
            EverpouringVialOfMercury,
            GlyphOfGallantry,
            GoldenBow,
            HeadOfLegion,
            HourglassOfTheEvilHour,
            InexhaustibleCartOfOre,
            InexhaustibleCartOfLumber,
            LadybirdOfLuck,
            LegsOfLegion,
            LoinsOfLegion,
            MysticOrbOfSpellPoints,
            OrbOfDrivingRain,
            OrbOfInhibition,
            OrbOfSilt,
            OrbOfTempestousFire,
            OrbOfFirmament,
            OrbOfVulnerability,
            ShacklesOfWar,
            Speculum,
            SphereOfPermanence,
            SpiritOfOppression,
            Spyglass,
            StatueOfLegion,
            StoicWatchman,
            TalismanOfSpellPoints,
            TomeOfAirMagic,
            TomeOfEarthMagic,
            TomeOfFireMagic,
            TomeOfWaterMagic,
            TorsoOfLegion,
            VialOfDragonblood,
            VialOfLifeblood,
            WizardsWell,
            CharmOfEclipse,
            DemonsHorseshoe,
            GoldenGoose,
            HideousMask,
            HornOfTheAbyss,
            RunesOfImminency,
            ShamansPuppet,
        ];

        public static readonly IReadOnlyList<Artifact> NonEquippable =
        [
            SpellBook,
            Catapult,
            TheGrail,
        ];

        public static readonly IReadOnlyList<Artifact> Combination = [.. Helmets.Concat(Necklaces)
            .Concat(Torsos)
            .Concat(Capes)
            .Concat(Feet)
            .Concat(Weapons)
            .Concat(Shields)
            .Concat(Rings)
            .Concat(Misc)
            .Where(a => a.IsCombination)];

        public static readonly IReadOnlyDictionary<byte, Artifact> All = Helmets.Concat(Necklaces)
            .Concat(Torsos)
            .Concat(Capes)
            .Concat(Feet)
            .Concat(Weapons)
            .Concat(Shields)
            .Concat(Rings)
            .Concat(Misc)
            .GroupBy(a => a.Code)
            .ToDictionary(g => g.Key, g => g.First());

        /// <summary>
        /// Gets what a backpack can hold, by code: every artifact that can be worn, and the Grail, which a hero carries after digging
        /// it up but cannot wear. It is not offered by the finders; it is only read and kept.
        /// </summary>
        public static readonly IReadOnlyDictionary<byte, Artifact> CarriedInBackpack = new Dictionary<byte, Artifact>(All)
        {
            [TheGrail.Code] = TheGrail,
        };

        private static readonly IReadOnlyDictionary<string, Artifact> ByName = All.Values
            .GroupBy(a => a.Name)
            .ToDictionary(g => g.Key, g => g.First());

        private readonly byte? combinesIntoCode;

        private Artifact(string name, byte code, ArtifactClass artifactClass, int cost, ArtifactSlots slot, ArtifactEffectInfo effectInfo, ArtifactSlots? blockedSlots, bool isCombination, byte? combinesInto, string description, GameVersion gameVersion)
            : base(name, code, gameVersion)
        {
            this.Slot = slot;
            this.EffectInfo = effectInfo;
            this.BlockedSlots = blockedSlots != null && blockedSlots.Value != ArtifactSlots.None ? blockedSlots.Value : slot;
            this.IsCombination = isCombination;
            this.combinesIntoCode = combinesInto;
            this.Description = description;
            this.Class = artifactClass;
            this.Cost = cost;
        }

        private Artifact(string name, byte code, ArtifactClass artifactClass, int cost, ArtifactSlots slot, ArtifactEffectInfo effectInfo)
            : this(name, code, artifactClass, cost, slot, effectInfo, null, false, null, string.Empty, GameVersion.RestorationOfErathia)
        {
        }

        private Artifact(string name, byte code, ArtifactClass artifactClass, int cost, ArtifactSlots slot, ArtifactEffectInfo effectInfo, GameVersion gameVersion)
            : this(name, code, artifactClass, cost, slot, effectInfo, null, false, null, string.Empty, gameVersion)
        {
        }

        private Artifact(string name, byte code, ArtifactClass artifactClass, int cost, ArtifactSlots slot, ArtifactEffectInfo effectInfo, byte combinesInto)
            : this(name, code, artifactClass, cost, slot, effectInfo, null, false, combinesInto, string.Empty, GameVersion.RestorationOfErathia)
        {
        }

        private Artifact(string name, byte code, ArtifactClass artifactClass, int cost, ArtifactSlots slot, ArtifactSlots blockedSlot, ArtifactEffectInfo effectInfo)
            : this(name, code, artifactClass, cost, slot, effectInfo, blockedSlot, true, null, string.Empty, GameVersion.RestorationOfErathia)
        {
        }

        private Artifact(string name, byte code, ArtifactClass artifactClass, int cost, ArtifactSlots slot, ArtifactSlots blockedSlot, ArtifactEffectInfo effectInfo, GameVersion gameVersion)
            : this(name, code, artifactClass, cost, slot, effectInfo, blockedSlot, true, null, string.Empty, gameVersion)
        {
        }

        public ArtifactSlots Slot { get; }

        public ArtifactSlots BlockedSlots { get; }

        public ArtifactEffectInfo EffectInfo { get; }

        public string Description { get; }

        public bool IsCombination { get; }

        public ArtifactClass Class { get; }

        public int Cost { get; }

        public Artifact? CombinesInto => this.combinesIntoCode is null ? null : FromCode<Artifact>(this.combinesIntoCode.Value);

        /// <summary>
        /// Returns the artifact with the given name, or null if there is none. A scroll name such as "Spell Scroll: Town Portal"
        /// gives the Spell Scroll artifact; see <see cref="Entities.SpellScroll"/> for the spell.
        /// </summary>
        public static Artifact? FindByName(string name) =>
            ByName.GetValueOrDefault(name) ?? (Entities.SpellScroll.TryParse(name, out _) ? SpellScroll : null);

        /// <summary>
        /// Gets the spells the artifact lets its wearer cast, according to the specified game <paramref name="version"/>.
        /// </summary>
        /// <returns>
        /// A collection of spells.
        /// </returns>
        public IReadOnlyCollection<Spell> GrantedSpells(GameVersion version) => this.EffectInfo.GrantedSpells(version);

        public override string ToString()
        {
            var combinesInto = this.CombinesInto is null ? "None" : this.CombinesInto.Name;

            return $"[{this.Name}, {nameof(this.Slot)}:{this.Slot}, {nameof(this.BlockedSlots)}:{this.BlockedSlots}, {nameof(this.CombinesInto)}:{combinesInto}, {nameof(this.IsCombination)}:{this.IsCombination}]";
        }
    }
}
