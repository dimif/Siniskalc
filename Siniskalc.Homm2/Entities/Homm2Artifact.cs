namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// An artifact of Heroes of Might and Magic II. The code is the number the game keeps in a hero's artifact slot (0xFF is an empty
    /// slot). The Magic Book is an artifact too: a hero has a spell book while one of its slots holds it.
    /// Names and numbers come from the artifact tables of the open source engine fheroes2 (its numbers are one more than the game's)
    /// and are checked against the savegames for the Medal of Courage, the Ice Cloak, the Ammo Cart and the Magic Book.
    /// The Price of Loyalty adds the artifacts from the Spell Scroll on.
    /// </summary>
    public sealed class Homm2Artifact : Homm2Enumeration
    {
        public static readonly Homm2Artifact UltimateBookOfKnowledge = new("Ultimate Book of Knowledge", 0);
        public static readonly Homm2Artifact UltimateSwordOfDominion = new("Ultimate Sword of Dominion", 1);
        public static readonly Homm2Artifact UltimateCloakOfProtection = new("Ultimate Cloak of Protection", 2);
        public static readonly Homm2Artifact UltimateWandOfMagic = new("Ultimate Wand of Magic", 3);
        public static readonly Homm2Artifact UltimateShield = new("Ultimate Shield", 4);
        public static readonly Homm2Artifact UltimateStaff = new("Ultimate Staff", 5);
        public static readonly Homm2Artifact UltimateCrown = new("Ultimate Crown", 6);
        public static readonly Homm2Artifact GoldenGoose = new("Golden Goose", 7);
        public static readonly Homm2Artifact ArcaneNecklaceOfMagic = new("Arcane Necklace of Magic", 8);
        public static readonly Homm2Artifact CastersBraceletOfMagic = new("Caster's Bracelet of Magic", 9);
        public static readonly Homm2Artifact MagesRingOfPower = new("Mage's Ring of Power", 10);
        public static readonly Homm2Artifact WitchsBroachOfMagic = new("Witch's Broach of Magic", 11);
        public static readonly Homm2Artifact MedalOfValor = new("Medal of Valor", 12);
        public static readonly Homm2Artifact MedalOfCourage = new("Medal of Courage", 13);
        public static readonly Homm2Artifact MedalOfHonor = new("Medal of Honor", 14);
        public static readonly Homm2Artifact MedalOfDistinction = new("Medal of Distinction", 15);
        public static readonly Homm2Artifact FizbinOfMisfortune = new("Fizbin of Misfortune", 16);
        public static readonly Homm2Artifact ThunderMaceOfDominion = new("Thunder Mace of Dominion", 17);
        public static readonly Homm2Artifact ArmoredGauntletsOfProtection = new("Armored Gauntlets of Protection", 18);
        public static readonly Homm2Artifact DefenderHelmOfProtection = new("Defender Helm of Protection", 19);
        public static readonly Homm2Artifact GiantFlailOfDominion = new("Giant Flail of Dominion", 20);
        public static readonly Homm2Artifact BallistaOfQuickness = new("Ballista of Quickness", 21);
        public static readonly Homm2Artifact StealthShieldOfProtection = new("Stealth Shield of Protection", 22);
        public static readonly Homm2Artifact DragonSwordOfDominion = new("Dragon Sword of Dominion", 23);
        public static readonly Homm2Artifact PowerAxeOfDominion = new("Power Axe of Dominion", 24);
        public static readonly Homm2Artifact DivineBreastplateOfProtection = new("Divine Breastplate of Protection", 25);
        public static readonly Homm2Artifact MinorScrollOfKnowledge = new("Minor Scroll of Knowledge", 26);
        public static readonly Homm2Artifact MajorScrollOfKnowledge = new("Major Scroll of Knowledge", 27);
        public static readonly Homm2Artifact SuperiorScrollOfKnowledge = new("Superior Scroll of Knowledge", 28);
        public static readonly Homm2Artifact ForemostScrollOfKnowledge = new("Foremost Scroll of Knowledge", 29);
        public static readonly Homm2Artifact EndlessSackOfGold = new("Endless Sack of Gold", 30);
        public static readonly Homm2Artifact EndlessBagOfGold = new("Endless Bag of Gold", 31);
        public static readonly Homm2Artifact EndlessPurseOfGold = new("Endless Purse of Gold", 32);
        public static readonly Homm2Artifact NomadBootsOfMobility = new("Nomad Boots of Mobility", 33);
        public static readonly Homm2Artifact TravelersBootsOfMobility = new("Traveler's Boots of Mobility", 34);
        public static readonly Homm2Artifact LuckyRabbitsFoot = new("Lucky Rabbit's Foot", 35);
        public static readonly Homm2Artifact GoldenHorseshoe = new("Golden Horseshoe", 36);
        public static readonly Homm2Artifact GamblersLuckyCoin = new("Gambler's Lucky Coin", 37);
        public static readonly Homm2Artifact FourLeafClover = new("Four-Leaf Clover", 38);
        public static readonly Homm2Artifact TrueCompassOfMobility = new("True Compass of Mobility", 39);
        public static readonly Homm2Artifact SailorsAstrolabeOfMobility = new("Sailor's Astrolabe of Mobility", 40);
        public static readonly Homm2Artifact EvilEye = new("Evil Eye", 41);
        public static readonly Homm2Artifact EnchantedHourglass = new("Enchanted Hourglass", 42);
        public static readonly Homm2Artifact GoldWatch = new("Gold Watch", 43);
        public static readonly Homm2Artifact Skullcap = new("Skullcap", 44);
        public static readonly Homm2Artifact IceCloak = new("Ice Cloak", 45);
        public static readonly Homm2Artifact FireCloak = new("Fire Cloak", 46);
        public static readonly Homm2Artifact LightningHelm = new("Lightning Helm", 47);
        public static readonly Homm2Artifact EvercoldIcicle = new("Evercold Icicle", 48);
        public static readonly Homm2Artifact EverhotLavaRock = new("Everhot Lava Rock", 49);
        public static readonly Homm2Artifact LightningRod = new("Lightning Rod", 50);
        public static readonly Homm2Artifact SnakeRing = new("Snake-Ring", 51);
        public static readonly Homm2Artifact Ankh = new("Ankh", 52);
        public static readonly Homm2Artifact BookOfElements = new("Book of Elements", 53);
        public static readonly Homm2Artifact ElementalRing = new("Elemental Ring", 54);
        public static readonly Homm2Artifact HolyPendant = new("Holy Pendant", 55);
        public static readonly Homm2Artifact PendantOfFreeWill = new("Pendant of Free Will", 56);
        public static readonly Homm2Artifact PendantOfLife = new("Pendant of Life", 57);
        public static readonly Homm2Artifact SerenityPendant = new("Serenity Pendant", 58);
        public static readonly Homm2Artifact SeeingeyePendant = new("Seeing-eye Pendant", 59);
        public static readonly Homm2Artifact KineticPendant = new("Kinetic Pendant", 60);
        public static readonly Homm2Artifact PendantOfDeath = new("Pendant of Death", 61);
        public static readonly Homm2Artifact WandOfNegation = new("Wand of Negation", 62);
        public static readonly Homm2Artifact GoldenBow = new("Golden Bow", 63);
        public static readonly Homm2Artifact Telescope = new("Telescope", 64);
        public static readonly Homm2Artifact StatesmansQuill = new("Statesman's Quill", 65);
        public static readonly Homm2Artifact WizardsHat = new("Wizard's Hat", 66);
        public static readonly Homm2Artifact PowerRing = new("Power Ring", 67);
        public static readonly Homm2Artifact AmmoCart = new("Ammo Cart", 68);
        public static readonly Homm2Artifact TaxLien = new("Tax Lien", 69);
        public static readonly Homm2Artifact HideousMask = new("Hideous Mask", 70);
        public static readonly Homm2Artifact EndlessPouchOfSulfur = new("Endless Pouch of Sulfur", 71);
        public static readonly Homm2Artifact EndlessVialOfMercury = new("Endless Vial of Mercury", 72);
        public static readonly Homm2Artifact EndlessPouchOfGems = new("Endless Pouch of Gems", 73);
        public static readonly Homm2Artifact EndlessCordOfWood = new("Endless Cord of Wood", 74);
        public static readonly Homm2Artifact EndlessCartOfOre = new("Endless Cart of Ore", 75);
        public static readonly Homm2Artifact EndlessPouchOfCrystal = new("Endless Pouch of Crystal", 76);
        public static readonly Homm2Artifact SpikedHelm = new("Spiked Helm", 77);
        public static readonly Homm2Artifact SpikedShield = new("Spiked Shield", 78);
        public static readonly Homm2Artifact WhitePearl = new("White Pearl", 79);
        public static readonly Homm2Artifact BlackPearl = new("Black Pearl", 80);
        public static readonly Homm2Artifact MagicBook = new("Magic Book", 81);
        public static readonly Homm2Artifact SpellScroll = new("Spell Scroll", 86, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact ArmOfTheMartyr = new("Arm of the Martyr", 87, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact BreastplateOfAnduran = new("Breastplate of Anduran", 88, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact BroachOfShielding = new("Broach of Shielding", 89, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact BattleGarbOfAnduran = new("Battle Garb of Anduran", 90, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact CrystalBall = new("Crystal Ball", 91, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact HeartOfFire = new("Heart of Fire", 92, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact HeartOfIce = new("Heart of Ice", 93, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact HelmetOfAnduran = new("Helmet of Anduran", 94, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact HolyHammer = new("Holy Hammer", 95, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact LegendaryScepter = new("Legendary Scepter", 96, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact Masthead = new("Masthead", 97, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact SphereOfNegation = new("Sphere of Negation", 98, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact StaffOfWizardry = new("Staff of Wizardry", 99, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact SwordBreaker = new("Sword Breaker", 100, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact SwordOfAnduran = new("Sword of Anduran", 101, Homm2GameVersion.PriceOfLoyalty);
        public static readonly Homm2Artifact SpadeOfNecromancy = new("Spade of Necromancy", 102, Homm2GameVersion.PriceOfLoyalty);

        private Homm2Artifact(string name, byte code)
            : base(name, code)
        {
        }

        private Homm2Artifact(string name, byte code, Homm2GameVersion gameVersion)
            : base(name, code, gameVersion)
        {
        }

        public static IReadOnlyList<Homm2Artifact> All => GetAll<Homm2Artifact>();

        public static Homm2Artifact? FromByte(byte code) => FromCode<Homm2Artifact>(code);

        public static string NameOf(byte code) => FromByte(code)?.Name ?? $"Unknown artifact {code}";
    }
}
