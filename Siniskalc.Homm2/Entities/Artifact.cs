namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// An artifact of Heroes of Might and Magic II. The code is the number the game keeps in a hero's artifact slot (0xFF is an empty
    /// slot). The Magic Book is an artifact too: a hero has a spell book while one of its slots holds it.
    /// Names and numbers come from the artifact tables of the open source engine fheroes2 (its numbers are one more than the game's)
    /// and are checked against the savegames for the Medal of Courage, the Ice Cloak, the Ammo Cart and the Magic Book.
    /// The Price of Loyalty adds the artifacts from the Spell Scroll on.
    /// </summary>
    public sealed class Artifact : Homm2Enumeration, IArtifact
    {
        public static readonly Artifact UltimateBookOfKnowledge = new("Ultimate Book of Knowledge", 0);
        public static readonly Artifact UltimateSwordOfDominion = new("Ultimate Sword of Dominion", 1);
        public static readonly Artifact UltimateCloakOfProtection = new("Ultimate Cloak of Protection", 2);
        public static readonly Artifact UltimateWandOfMagic = new("Ultimate Wand of Magic", 3);
        public static readonly Artifact UltimateShield = new("Ultimate Shield", 4);
        public static readonly Artifact UltimateStaff = new("Ultimate Staff", 5);
        public static readonly Artifact UltimateCrown = new("Ultimate Crown", 6);
        public static readonly Artifact GoldenGoose = new("Golden Goose", 7);
        public static readonly Artifact ArcaneNecklaceOfMagic = new("Arcane Necklace of Magic", 8);
        public static readonly Artifact CastersBraceletOfMagic = new("Caster's Bracelet of Magic", 9);
        public static readonly Artifact MagesRingOfPower = new("Mage's Ring of Power", 10);
        public static readonly Artifact WitchsBroachOfMagic = new("Witch's Broach of Magic", 11);
        public static readonly Artifact MedalOfValor = new("Medal of Valor", 12);
        public static readonly Artifact MedalOfCourage = new("Medal of Courage", 13);
        public static readonly Artifact MedalOfHonor = new("Medal of Honor", 14);
        public static readonly Artifact MedalOfDistinction = new("Medal of Distinction", 15);
        public static readonly Artifact FizbinOfMisfortune = new("Fizbin of Misfortune", 16);
        public static readonly Artifact ThunderMaceOfDominion = new("Thunder Mace of Dominion", 17);
        public static readonly Artifact ArmoredGauntletsOfProtection = new("Armored Gauntlets of Protection", 18);
        public static readonly Artifact DefenderHelmOfProtection = new("Defender Helm of Protection", 19);
        public static readonly Artifact GiantFlailOfDominion = new("Giant Flail of Dominion", 20);
        public static readonly Artifact BallistaOfQuickness = new("Ballista of Quickness", 21);
        public static readonly Artifact StealthShieldOfProtection = new("Stealth Shield of Protection", 22);
        public static readonly Artifact DragonSwordOfDominion = new("Dragon Sword of Dominion", 23);
        public static readonly Artifact PowerAxeOfDominion = new("Power Axe of Dominion", 24);
        public static readonly Artifact DivineBreastplateOfProtection = new("Divine Breastplate of Protection", 25);
        public static readonly Artifact MinorScrollOfKnowledge = new("Minor Scroll of Knowledge", 26);
        public static readonly Artifact MajorScrollOfKnowledge = new("Major Scroll of Knowledge", 27);
        public static readonly Artifact SuperiorScrollOfKnowledge = new("Superior Scroll of Knowledge", 28);
        public static readonly Artifact ForemostScrollOfKnowledge = new("Foremost Scroll of Knowledge", 29);
        public static readonly Artifact EndlessSackOfGold = new("Endless Sack of Gold", 30);
        public static readonly Artifact EndlessBagOfGold = new("Endless Bag of Gold", 31);
        public static readonly Artifact EndlessPurseOfGold = new("Endless Purse of Gold", 32);
        public static readonly Artifact NomadBootsOfMobility = new("Nomad Boots of Mobility", 33);
        public static readonly Artifact TravelersBootsOfMobility = new("Traveler's Boots of Mobility", 34);
        public static readonly Artifact LuckyRabbitsFoot = new("Lucky Rabbit's Foot", 35);
        public static readonly Artifact GoldenHorseshoe = new("Golden Horseshoe", 36);
        public static readonly Artifact GamblersLuckyCoin = new("Gambler's Lucky Coin", 37);
        public static readonly Artifact FourLeafClover = new("Four-Leaf Clover", 38);
        public static readonly Artifact TrueCompassOfMobility = new("True Compass of Mobility", 39);
        public static readonly Artifact SailorsAstrolabeOfMobility = new("Sailor's Astrolabe of Mobility", 40);
        public static readonly Artifact EvilEye = new("Evil Eye", 41);
        public static readonly Artifact EnchantedHourglass = new("Enchanted Hourglass", 42);
        public static readonly Artifact GoldWatch = new("Gold Watch", 43);
        public static readonly Artifact Skullcap = new("Skullcap", 44);
        public static readonly Artifact IceCloak = new("Ice Cloak", 45);
        public static readonly Artifact FireCloak = new("Fire Cloak", 46);
        public static readonly Artifact LightningHelm = new("Lightning Helm", 47);
        public static readonly Artifact EvercoldIcicle = new("Evercold Icicle", 48);
        public static readonly Artifact EverhotLavaRock = new("Everhot Lava Rock", 49);
        public static readonly Artifact LightningRod = new("Lightning Rod", 50);
        public static readonly Artifact SnakeRing = new("Snake-Ring", 51);
        public static readonly Artifact Ankh = new("Ankh", 52);
        public static readonly Artifact BookOfElements = new("Book of Elements", 53);
        public static readonly Artifact ElementalRing = new("Elemental Ring", 54);
        public static readonly Artifact HolyPendant = new("Holy Pendant", 55);
        public static readonly Artifact PendantOfFreeWill = new("Pendant of Free Will", 56);
        public static readonly Artifact PendantOfLife = new("Pendant of Life", 57);
        public static readonly Artifact SerenityPendant = new("Serenity Pendant", 58);
        public static readonly Artifact SeeingeyePendant = new("Seeing-eye Pendant", 59);
        public static readonly Artifact KineticPendant = new("Kinetic Pendant", 60);
        public static readonly Artifact PendantOfDeath = new("Pendant of Death", 61);
        public static readonly Artifact WandOfNegation = new("Wand of Negation", 62);
        public static readonly Artifact GoldenBow = new("Golden Bow", 63);
        public static readonly Artifact Telescope = new("Telescope", 64);
        public static readonly Artifact StatesmansQuill = new("Statesman's Quill", 65);
        public static readonly Artifact WizardsHat = new("Wizard's Hat", 66);
        public static readonly Artifact PowerRing = new("Power Ring", 67);
        public static readonly Artifact AmmoCart = new("Ammo Cart", 68);
        public static readonly Artifact TaxLien = new("Tax Lien", 69);
        public static readonly Artifact HideousMask = new("Hideous Mask", 70);
        public static readonly Artifact EndlessPouchOfSulfur = new("Endless Pouch of Sulfur", 71);
        public static readonly Artifact EndlessVialOfMercury = new("Endless Vial of Mercury", 72);
        public static readonly Artifact EndlessPouchOfGems = new("Endless Pouch of Gems", 73);
        public static readonly Artifact EndlessCordOfWood = new("Endless Cord of Wood", 74);
        public static readonly Artifact EndlessCartOfOre = new("Endless Cart of Ore", 75);
        public static readonly Artifact EndlessPouchOfCrystal = new("Endless Pouch of Crystal", 76);
        public static readonly Artifact SpikedHelm = new("Spiked Helm", 77);
        public static readonly Artifact SpikedShield = new("Spiked Shield", 78);
        public static readonly Artifact WhitePearl = new("White Pearl", 79);
        public static readonly Artifact BlackPearl = new("Black Pearl", 80);
        public static readonly Artifact MagicBook = new("Magic Book", 81);
        public static readonly Artifact SpellScroll = new("Spell Scroll", 86, GameVersion.PriceOfLoyalty);
        public static readonly Artifact ArmOfTheMartyr = new("Arm of the Martyr", 87, GameVersion.PriceOfLoyalty);
        public static readonly Artifact BreastplateOfAnduran = new("Breastplate of Anduran", 88, GameVersion.PriceOfLoyalty);
        public static readonly Artifact BroachOfShielding = new("Broach of Shielding", 89, GameVersion.PriceOfLoyalty);
        public static readonly Artifact BattleGarbOfAnduran = new("Battle Garb of Anduran", 90, GameVersion.PriceOfLoyalty);
        public static readonly Artifact CrystalBall = new("Crystal Ball", 91, GameVersion.PriceOfLoyalty);
        public static readonly Artifact HeartOfFire = new("Heart of Fire", 92, GameVersion.PriceOfLoyalty);
        public static readonly Artifact HeartOfIce = new("Heart of Ice", 93, GameVersion.PriceOfLoyalty);
        public static readonly Artifact HelmetOfAnduran = new("Helmet of Anduran", 94, GameVersion.PriceOfLoyalty);
        public static readonly Artifact HolyHammer = new("Holy Hammer", 95, GameVersion.PriceOfLoyalty);
        public static readonly Artifact LegendaryScepter = new("Legendary Scepter", 96, GameVersion.PriceOfLoyalty);
        public static readonly Artifact Masthead = new("Masthead", 97, GameVersion.PriceOfLoyalty);
        public static readonly Artifact SphereOfNegation = new("Sphere of Negation", 98, GameVersion.PriceOfLoyalty);
        public static readonly Artifact StaffOfWizardry = new("Staff of Wizardry", 99, GameVersion.PriceOfLoyalty);
        public static readonly Artifact SwordBreaker = new("Sword Breaker", 100, GameVersion.PriceOfLoyalty);
        public static readonly Artifact SwordOfAnduran = new("Sword of Anduran", 101, GameVersion.PriceOfLoyalty);
        public static readonly Artifact SpadeOfNecromancy = new("Spade of Necromancy", 102, GameVersion.PriceOfLoyalty);

        private Artifact(string name, byte code)
            : base(name, code)
        {
        }

        private Artifact(string name, byte code, GameVersion gameVersion)
            : base(name, code, gameVersion)
        {
        }

        public static IReadOnlyList<Artifact> All => GetAll<Artifact>();

        /// <inheritdoc/>
        public PrimarySkillBonus PrimarySkillBonus => ArtifactBonuses.Of(this);

        public static Artifact? FromByte(byte code) => FromCode<Artifact>(code);

        public static string NameOf(byte code) => FromByte(code)?.Name ?? $"Unknown artifact {code}";
    }
}
