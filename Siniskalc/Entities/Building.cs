namespace Siniskalc.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;

    /// <summary>
    /// A structure of a town. The <see cref="AbstractEnumeration.Code"/> is the position of the building's bit in the
    /// building masks of a town in the savefile. Buildings common to all factions (the halls, the fort, the Mage Guild, ...)
    /// have no <see cref="Faction"/>; the dwellings and the special structures belong to one faction, and share codes with
    /// the buildings of the other factions, so use <see cref="FromBit"/> rather than <see cref="AbstractEnumeration.FromCode{T}"/>.
    /// </summary>
    /// <remarks>
    /// The bit positions and the prerequisites come from the building tables of VCMI. Bits 0-16, 30-36 and the upgraded
    /// dwellings (37-43) are confirmed by savefiles of this editor, and so are the special structures (17-26) of the eight factions of
    /// Shadow of Death and Armageddon's Blade, but for the Rampart, whose shipyard-less castle was only checked as far as it was built. The buildings of
    /// Cove, Factory and Bulwark have their bits too (see the tests of those castles).
    /// The upgraded horde buildings (bits 19 and 25) cannot be built: the game sets them by itself once a horde building and the upgrade
    /// of its dwelling both exist. Likewise the bits 27, 28 and 29 come with the town, city hall and capitol and are not modelled.
    /// The upgraded horde buildings of Cove, Factory and Bulwark were found the same way and are named after their horde building.
    /// Necropolis castles have bit 23 from the start as well, and the game sets bit 23 with the fort in Castle, Fortress and Conflux castles (and 22 in Conflux, 27 in Stronghold); none of these are modelled.
    /// Rampart castles also have bit 23 from the start, with only a tavern, fort and two dwellings built, and the game does not allow building it; it is not modelled.
    /// Static fields must be declared after the buildings they refer to.
    /// </remarks>
    public sealed class Building : Homm3Enumeration
    {
        /// <summary>
        /// The number of levels of creatures in a town, and so of dwellings.
        /// </summary>
        public const int DwellingLevelCount = 7;

        /// <summary>
        /// The number of levels of the Mage Guild.
        /// </summary>
        public const int MageGuildLevelCount = 5;

        public static readonly Building MagesGuild1 = new("Mage Guild, level 1", BuildingType.MageGuild1, 0);
        public static readonly Building MagesGuild2 = new("Mage Guild, level 2", BuildingType.MageGuild2, 1, upgrades: MagesGuild1);
        public static readonly Building MagesGuild3 = new("Mage Guild, level 3", BuildingType.MageGuild3, 2, upgrades: MagesGuild2);
        public static readonly Building MagesGuild4 = new("Mage Guild, level 4", BuildingType.MageGuild4, 3, except: [Faction.Stronghold, Faction.Fortress], upgrades: MagesGuild3);
        public static readonly Building MagesGuild5 = new("Mage Guild, level 5", BuildingType.MageGuild5, 4, except: [Faction.Castle, Faction.Cove, Faction.Bulwark, Faction.Stronghold, Faction.Fortress], upgrades: MagesGuild4);
        public static readonly Building Tavern = new("Tavern", BuildingType.Tavern, 5);
        public static readonly Building Shipyard = new("Shipyard", BuildingType.Shipyard, 6, except: [Faction.Rampart, Faction.Tower, Faction.Inferno, Faction.Dungeon, Faction.Stronghold, Faction.Factory]);
        public static readonly Building Fort = new("Fort", BuildingType.Fort, 7);
        public static readonly Building Citadel = new("Citadel", BuildingType.Citadel, 8, upgrades: Fort);
        public static readonly Building Castle = new("Castle", BuildingType.Castle, 9, upgrades: Citadel);
        public static readonly Building VillageHall = new("Village Hall", BuildingType.VillageHall, 10);
        public static readonly Building TownHall = new("Town Hall", BuildingType.TownHall, 11, upgrades: VillageHall, requires: [Tavern]);
        public static readonly Building Marketplace = new("Marketplace", BuildingType.Marketplace, 14);
        public static readonly Building Blacksmith = new("Blacksmith", BuildingType.Blacksmith, 16);
        public static readonly Building CityHall = new("City Hall", BuildingType.CityHall, 12, upgrades: TownHall, requires: [MagesGuild1, Marketplace, Blacksmith]);
        public static readonly Building Capitol = new("Capitol", BuildingType.Capitol, 13, upgrades: CityHall, requires: [Castle]);
        public static readonly Building ResourceSilo = new("Resource Silo", BuildingType.ResourceSilo, 15, requires: [Marketplace]);

        // Castle
        public static readonly Building CastleLighthouse = new("Lighthouse", BuildingType.Special1, 17, Faction.Castle, requires: [Shipyard]);
        public static readonly Building CastleGuardhouse = new("Guardhouse", BuildingType.Dwelling1, 30, Faction.Castle, requires: [Fort]);
        public static readonly Building CastleBarracks = new("Barracks", BuildingType.Dwelling4, 33, Faction.Castle, requires: [Blacksmith, CastleGuardhouse]);
        public static readonly Building CastleGriffinTower = new("Griffin Tower", BuildingType.Dwelling3, 32, Faction.Castle, requires: [CastleBarracks]);
        public static readonly Building CastleGriffinBastion = new("Griffin Bastion", BuildingType.Horde1, 18, Faction.Castle, upgrades: CastleGriffinTower);
        public static readonly Building CastleUpgGriffinTower = new("Upg. Griffin Tower", BuildingType.UpgradedDwelling3, 39, Faction.Castle, upgrades: CastleGriffinTower);
        public static readonly Building CastleUpgGriffinBastion = new("Upg. Griffin Bastion", BuildingType.UpgradedHorde1, 19, Faction.Castle, upgrades: CastleUpgGriffinTower, requires: [CastleGriffinBastion]);
        public static readonly Building CastleStables = new("Stables", BuildingType.Special2, 21, Faction.Castle, requires: [CastleBarracks]);
        public static readonly Building CastleBrotherhoodOfTheSword = new("Brotherhood of the Sword", BuildingType.Special3, 22, Faction.Castle, upgrades: Tavern);
        public static readonly Building CastleColossus = new("Colossus", BuildingType.Grail, 26, Faction.Castle);
        public static readonly Building CastleArchersTower = new("Archers' Tower", BuildingType.Dwelling2, 31, Faction.Castle, requires: [CastleGuardhouse]);
        public static readonly Building CastleMonastery = new("Monastery", BuildingType.Dwelling5, 34, Faction.Castle, requires: [MagesGuild1, CastleBarracks]);
        public static readonly Building CastleTrainingGrounds = new("Training Grounds", BuildingType.Dwelling6, 35, Faction.Castle, requires: [CastleStables]);
        public static readonly Building CastlePortalOfGlory = new("Portal of Glory", BuildingType.Dwelling7, 36, Faction.Castle, requires: [CastleMonastery]);
        public static readonly Building CastleUpgGuardhouse = new("Upg. Guardhouse", BuildingType.UpgradedDwelling1, 37, Faction.Castle, upgrades: CastleGuardhouse);
        public static readonly Building CastleUpgArchersTower = new("Upg. Archers' Tower", BuildingType.UpgradedDwelling2, 38, Faction.Castle, upgrades: CastleArchersTower);
        public static readonly Building CastleUpgBarracks = new("Upg. Barracks", BuildingType.UpgradedDwelling4, 40, Faction.Castle, upgrades: CastleBarracks);
        public static readonly Building CastleUpgMonastery = new("Upg. Monastery", BuildingType.UpgradedDwelling5, 41, Faction.Castle, upgrades: CastleMonastery);
        public static readonly Building CastleUpgTrainingGrounds = new("Upg. Training Grounds", BuildingType.UpgradedDwelling6, 42, Faction.Castle, upgrades: CastleTrainingGrounds);
        public static readonly Building CastleUpgPortalOfGlory = new("Upg. Portal of Glory", BuildingType.UpgradedDwelling7, 43, Faction.Castle, upgrades: CastlePortalOfGlory);

        // Rampart
        public static readonly Building RampartMysticPond = new("Mystic Pond", BuildingType.Special1, 17, Faction.Rampart);
        public static readonly Building RampartCentaurStables = new("Centaur Stables", BuildingType.Dwelling1, 30, Faction.Rampart, requires: [Fort]);
        public static readonly Building RampartDwarfCottage = new("Dwarf Cottage", BuildingType.Dwelling2, 31, Faction.Rampart, requires: [RampartCentaurStables]);
        public static readonly Building RampartMinersGuild = new("Miners' Guild", BuildingType.Horde1, 18, Faction.Rampart, upgrades: RampartDwarfCottage);
        public static readonly Building RampartUpgDwarfCottage = new("Upg. Dwarf Cottage", BuildingType.UpgradedDwelling2, 38, Faction.Rampart, upgrades: RampartDwarfCottage);
        public static readonly Building RampartUpgMinersGuild = new("Upg. Miners' Guild", BuildingType.UpgradedHorde1, 19, Faction.Rampart, upgrades: RampartUpgDwarfCottage, requires: [RampartMinersGuild]);
        public static readonly Building RampartFountainOfFortune = new("Fountain of Fortune", BuildingType.Special2, 21, Faction.Rampart, upgrades: RampartMysticPond);
        public static readonly Building RampartTreasury = new("Treasury", BuildingType.Special3, 22, Faction.Rampart, requires: [RampartMinersGuild]);
        public static readonly Building RampartHomestead = new("Homestead", BuildingType.Dwelling3, 32, Faction.Rampart, requires: [RampartCentaurStables]);
        public static readonly Building RampartDendroidArches = new("Dendroid Arches", BuildingType.Dwelling5, 34, Faction.Rampart, requires: [RampartHomestead]);
        public static readonly Building RampartDendroidSaplings = new("Dendroid Saplings", BuildingType.Horde2, 24, Faction.Rampart, upgrades: RampartDendroidArches);
        public static readonly Building RampartUpgDendroidArches = new("Upg. Dendroid Arches", BuildingType.UpgradedDwelling5, 41, Faction.Rampart, upgrades: RampartDendroidArches);
        public static readonly Building RampartUpgDendroidSaplings = new("Upg. Dendroid Saplings", BuildingType.UpgradedHorde2, 25, Faction.Rampart, upgrades: RampartUpgDendroidArches, requires: [RampartDendroidSaplings]);
        public static readonly Building RampartSpiritGuardian = new("Spirit Guardian", BuildingType.Grail, 26, Faction.Rampart);
        public static readonly Building RampartEnchantedSpring = new("Enchanted Spring", BuildingType.Dwelling4, 33, Faction.Rampart, requires: [RampartHomestead]);
        public static readonly Building RampartUnicornGlade = new("Unicorn Glade", BuildingType.Dwelling6, 35, Faction.Rampart, requires: [RampartEnchantedSpring, RampartDendroidArches]);
        public static readonly Building RampartDragonCliffs = new("Dragon Cliffs", BuildingType.Dwelling7, 36, Faction.Rampart, requires: [RampartUnicornGlade, MagesGuild2]);
        public static readonly Building RampartUpgCentaurStables = new("Upg. Centaur Stables", BuildingType.UpgradedDwelling1, 37, Faction.Rampart, upgrades: RampartCentaurStables);
        public static readonly Building RampartUpgHomestead = new("Upg. Homestead", BuildingType.UpgradedDwelling3, 39, Faction.Rampart, upgrades: RampartHomestead);
        public static readonly Building RampartUpgEnchantedSpring = new("Upg. Enchanted Spring", BuildingType.UpgradedDwelling4, 40, Faction.Rampart, upgrades: RampartEnchantedSpring);
        public static readonly Building RampartUpgUnicornGlade = new("Upg. Unicorn Glade", BuildingType.UpgradedDwelling6, 42, Faction.Rampart, upgrades: RampartUnicornGlade);
        public static readonly Building RampartUpgDragonCliffs = new("Upg. Dragon Cliffs", BuildingType.UpgradedDwelling7, 43, Faction.Rampart, upgrades: RampartDragonCliffs, requires: [MagesGuild3]);

        // Tower
        public static readonly Building TowerArtifactMerchants = new("Artifact Merchants", BuildingType.Special1, 17, Faction.Tower, requires: [Marketplace]);
        public static readonly Building TowerWorkshop = new("Workshop", BuildingType.Dwelling1, 30, Faction.Tower, requires: [Fort]);
        public static readonly Building TowerParapet = new("Parapet", BuildingType.Dwelling2, 31, Faction.Tower, requires: [TowerWorkshop]);
        public static readonly Building TowerSculptorsWings = new("Sculptor's Wings", BuildingType.Horde1, 18, Faction.Tower, upgrades: TowerParapet);
        public static readonly Building TowerUpgParapet = new("Upg. Parapet", BuildingType.UpgradedDwelling2, 38, Faction.Tower, upgrades: TowerParapet);
        public static readonly Building TowerUpgSculptorsWings = new("Upg. Sculptor's Wings", BuildingType.UpgradedHorde1, 19, Faction.Tower, upgrades: TowerUpgParapet, requires: [TowerSculptorsWings]);
        public static readonly Building TowerLookoutTower = new("Lookout Tower", BuildingType.Special2, 21, Faction.Tower, requires: [Fort]);
        public static readonly Building TowerLibrary = new("Library", BuildingType.Special3, 22, Faction.Tower, requires: [MagesGuild1]);
        public static readonly Building TowerWallOfKnowledge = new("Wall of Knowledge", BuildingType.Special4, 23, Faction.Tower, requires: [MagesGuild1]);
        public static readonly Building TowerSkyship = new("Skyship", BuildingType.Grail, 26, Faction.Tower);
        public static readonly Building TowerGolemFactory = new("Golem Factory", BuildingType.Dwelling3, 32, Faction.Tower, requires: [TowerWorkshop]);
        public static readonly Building TowerMageTower = new("Mage Tower", BuildingType.Dwelling4, 33, Faction.Tower, requires: [MagesGuild1, TowerParapet, TowerGolemFactory]);
        public static readonly Building TowerAltarOfWishes = new("Altar of Wishes", BuildingType.Dwelling5, 34, Faction.Tower, requires: [TowerMageTower]);
        public static readonly Building TowerGoldenPavilion = new("Golden Pavilion", BuildingType.Dwelling6, 35, Faction.Tower, requires: [TowerMageTower]);
        public static readonly Building TowerCloudTemple = new("Cloud Temple", BuildingType.Dwelling7, 36, Faction.Tower, requires: [TowerAltarOfWishes, TowerGoldenPavilion]);
        public static readonly Building TowerUpgWorkshop = new("Upg. Workshop", BuildingType.UpgradedDwelling1, 37, Faction.Tower, upgrades: TowerWorkshop);
        public static readonly Building TowerUpgGolemFactory = new("Upg. Golem Factory", BuildingType.UpgradedDwelling3, 39, Faction.Tower, upgrades: TowerGolemFactory);
        public static readonly Building TowerUpgMageTower = new("Upg. Mage Tower", BuildingType.UpgradedDwelling4, 40, Faction.Tower, upgrades: TowerMageTower, requires: [TowerLibrary]);
        public static readonly Building TowerUpgAltarOfWishes = new("Upg. Altar of Wishes", BuildingType.UpgradedDwelling5, 41, Faction.Tower, upgrades: TowerAltarOfWishes);
        public static readonly Building TowerUpgGoldenPavilion = new("Upg. Golden Pavilion", BuildingType.UpgradedDwelling6, 42, Faction.Tower, upgrades: TowerGoldenPavilion);
        public static readonly Building TowerUpgCloudTemple = new("Upg. Cloud Temple", BuildingType.UpgradedDwelling7, 43, Faction.Tower, upgrades: TowerCloudTemple);

        // Inferno
        public static readonly Building InfernoImpCrucible = new("Imp Crucible", BuildingType.Dwelling1, 30, Faction.Inferno, requires: [Fort]);
        public static readonly Building InfernoBirthingPool = new("Birthing Pool", BuildingType.Horde1, 18, Faction.Inferno, upgrades: InfernoImpCrucible);
        public static readonly Building InfernoUpgImpCrucible = new("Upg. Imp Crucible", BuildingType.UpgradedDwelling1, 37, Faction.Inferno, upgrades: InfernoImpCrucible);
        public static readonly Building InfernoUpgBirthingPool = new("Upg. Birthing Pool", BuildingType.UpgradedHorde1, 19, Faction.Inferno, upgrades: InfernoUpgImpCrucible, requires: [InfernoBirthingPool]);
        public static readonly Building InfernoBrimstoneStormclouds = new("Brimstone Stormclouds", BuildingType.Special2, 21, Faction.Inferno, requires: [Fort]);
        public static readonly Building InfernoCastleGate = new("Castle Gate", BuildingType.Special3, 22, Faction.Inferno, requires: [Citadel]);
        public static readonly Building InfernoOrderOfFire = new("Order of Fire", BuildingType.Special4, 23, Faction.Inferno, requires: [MagesGuild1]);
        public static readonly Building InfernoKennels = new("Kennels", BuildingType.Dwelling3, 32, Faction.Inferno, requires: [InfernoImpCrucible]);
        public static readonly Building InfernoCages = new("Cages", BuildingType.Horde2, 24, Faction.Inferno, upgrades: InfernoKennels);
        public static readonly Building InfernoUpgKennels = new("Upg. Kennels", BuildingType.UpgradedDwelling3, 39, Faction.Inferno, upgrades: InfernoKennels);
        public static readonly Building InfernoUpgCages = new("Upg. Cages", BuildingType.UpgradedHorde2, 25, Faction.Inferno, upgrades: InfernoUpgKennels, requires: [InfernoCages]);
        public static readonly Building InfernoDeityOfFire = new("Deity of Fire", BuildingType.Grail, 26, Faction.Inferno);
        public static readonly Building InfernoHallOfSins = new("Hall of Sins", BuildingType.Dwelling2, 31, Faction.Inferno, requires: [InfernoImpCrucible]);
        public static readonly Building InfernoDemonGate = new("Demon Gate", BuildingType.Dwelling4, 33, Faction.Inferno, requires: [InfernoHallOfSins]);
        public static readonly Building InfernoHellHole = new("Hell Hole", BuildingType.Dwelling5, 34, Faction.Inferno, requires: [InfernoDemonGate]);
        public static readonly Building InfernoFireLake = new("Fire Lake", BuildingType.Dwelling6, 35, Faction.Inferno, requires: [MagesGuild1, InfernoDemonGate]);
        public static readonly Building InfernoForsakenPalace = new("Forsaken Palace", BuildingType.Dwelling7, 36, Faction.Inferno, requires: [InfernoHellHole, InfernoFireLake]);
        public static readonly Building InfernoUpgHallOfSins = new("Upg. Hall of Sins", BuildingType.UpgradedDwelling2, 38, Faction.Inferno, upgrades: InfernoHallOfSins);
        public static readonly Building InfernoUpgDemonGate = new("Upg. Demon Gate", BuildingType.UpgradedDwelling4, 40, Faction.Inferno, upgrades: InfernoDemonGate);
        public static readonly Building InfernoUpgHellHole = new("Upg. Hell Hole", BuildingType.UpgradedDwelling5, 41, Faction.Inferno, upgrades: InfernoHellHole, requires: [MagesGuild2]);
        public static readonly Building InfernoUpgFireLake = new("Upg. Fire Lake", BuildingType.UpgradedDwelling6, 42, Faction.Inferno, upgrades: InfernoFireLake);
        public static readonly Building InfernoUpgForsakenPalace = new("Upg. Forsaken Palace", BuildingType.UpgradedDwelling7, 43, Faction.Inferno, upgrades: InfernoForsakenPalace);

        // Necropolis
        public static readonly Building NecropolisCoverOfDarkness = new("Cover of Darkness", BuildingType.Special1, 17, Faction.Necropolis, requires: [Fort]);
        public static readonly Building NecropolisCursedTemple = new("Cursed Temple", BuildingType.Dwelling1, 30, Faction.Necropolis, requires: [Fort]);
        public static readonly Building NecropolisSkeletonTransformer = new("Skeleton Transformer", BuildingType.Special3, 22, Faction.Necropolis, requires: [NecropolisCursedTemple]);
        public static readonly Building NecropolisUnearthedGraves = new("Unearthed Graves", BuildingType.Horde1, 18, Faction.Necropolis, upgrades: NecropolisCursedTemple, requires: [NecropolisSkeletonTransformer]);
        public static readonly Building NecropolisUpgCursedTemple = new("Upg. Cursed Temple", BuildingType.UpgradedDwelling1, 37, Faction.Necropolis, upgrades: NecropolisCursedTemple);
        public static readonly Building NecropolisUpgUnearthedGraves = new("Upg. Unearthed Graves", BuildingType.UpgradedHorde1, 19, Faction.Necropolis, upgrades: NecropolisUpgCursedTemple, requires: [NecropolisUnearthedGraves]);
        public static readonly Building NecropolisNecromancyAmplifier = new("Necromancy Amplifier", BuildingType.Special2, 21, Faction.Necropolis, requires: [MagesGuild1]);
        public static readonly Building NecropolisSoulPrison = new("Soul Prison", BuildingType.Grail, 26, Faction.Necropolis);
        public static readonly Building NecropolisGraveyard = new("Graveyard", BuildingType.Dwelling2, 31, Faction.Necropolis, requires: [NecropolisCursedTemple]);
        public static readonly Building NecropolisTombOfSouls = new("Tomb of Souls", BuildingType.Dwelling3, 32, Faction.Necropolis, requires: [NecropolisCursedTemple]);
        public static readonly Building NecropolisEstate = new("Estate", BuildingType.Dwelling4, 33, Faction.Necropolis, requires: [NecropolisGraveyard]);
        public static readonly Building NecropolisMausoleum = new("Mausoleum", BuildingType.Dwelling5, 34, Faction.Necropolis, requires: [MagesGuild1, NecropolisGraveyard]);
        public static readonly Building NecropolisHallOfDarkness = new("Hall of Darkness", BuildingType.Dwelling6, 35, Faction.Necropolis, requires: [NecropolisEstate, NecropolisMausoleum]);
        public static readonly Building NecropolisDragonVault = new("Dragon Vault", BuildingType.Dwelling7, 36, Faction.Necropolis, requires: [NecropolisHallOfDarkness]);
        public static readonly Building NecropolisUpgGraveyard = new("Upg. Graveyard", BuildingType.UpgradedDwelling2, 38, Faction.Necropolis, upgrades: NecropolisGraveyard);
        public static readonly Building NecropolisUpgTombOfSouls = new("Upg. Tomb of Souls", BuildingType.UpgradedDwelling3, 39, Faction.Necropolis, upgrades: NecropolisTombOfSouls);
        public static readonly Building NecropolisUpgEstate = new("Upg. Estate", BuildingType.UpgradedDwelling4, 40, Faction.Necropolis, upgrades: NecropolisEstate, requires: [NecropolisNecromancyAmplifier]);
        public static readonly Building NecropolisUpgMausoleum = new("Upg. Mausoleum", BuildingType.UpgradedDwelling5, 41, Faction.Necropolis, upgrades: NecropolisMausoleum);
        public static readonly Building NecropolisUpgHallOfDarkness = new("Upg. Hall of Darkness", BuildingType.UpgradedDwelling6, 42, Faction.Necropolis, upgrades: NecropolisHallOfDarkness);
        public static readonly Building NecropolisUpgDragonVault = new("Upg. Dragon Vault", BuildingType.UpgradedDwelling7, 43, Faction.Necropolis, upgrades: NecropolisDragonVault);

        // Dungeon
        public static readonly Building DungeonArtifactMerchants = new("Artifact Merchants", BuildingType.Special1, 17, Faction.Dungeon, requires: [Marketplace]);
        public static readonly Building DungeonWarren = new("Warren", BuildingType.Dwelling1, 30, Faction.Dungeon, requires: [Fort]);
        public static readonly Building DungeonMushroomRings = new("Mushroom Rings", BuildingType.Horde1, 18, Faction.Dungeon, upgrades: DungeonWarren);
        public static readonly Building DungeonUpgWarren = new("Upg. Warren", BuildingType.UpgradedDwelling1, 37, Faction.Dungeon, upgrades: DungeonWarren);
        public static readonly Building DungeonUpgMushroomRings = new("Upg. Mushroom Rings", BuildingType.UpgradedHorde1, 19, Faction.Dungeon, upgrades: DungeonUpgWarren, requires: [DungeonMushroomRings]);
        public static readonly Building DungeonManaVortex = new("Mana Vortex", BuildingType.Special2, 21, Faction.Dungeon, requires: [MagesGuild1]);
        public static readonly Building DungeonPortalOfSummoning = new("Portal of Summoning", BuildingType.Special3, 22, Faction.Dungeon);
        public static readonly Building DungeonBattleScholarAcademy = new("Battle Scholar Academy", BuildingType.Special4, 23, Faction.Dungeon);
        public static readonly Building DungeonGuardianOfEarth = new("Guardian of Earth", BuildingType.Grail, 26, Faction.Dungeon);
        public static readonly Building DungeonHarpyLoft = new("Harpy Loft", BuildingType.Dwelling2, 31, Faction.Dungeon, requires: [DungeonWarren]);
        public static readonly Building DungeonPillarOfEyes = new("Pillar of Eyes", BuildingType.Dwelling3, 32, Faction.Dungeon, requires: [DungeonWarren]);
        public static readonly Building DungeonChapelOfStilledVoices = new("Chapel of Stilled Voices", BuildingType.Dwelling4, 33, Faction.Dungeon, requires: [DungeonHarpyLoft, DungeonPillarOfEyes]);
        public static readonly Building DungeonLabyrinth = new("Labyrinth", BuildingType.Dwelling5, 34, Faction.Dungeon, requires: [DungeonChapelOfStilledVoices]);
        public static readonly Building DungeonManticoreLair = new("Manticore Lair", BuildingType.Dwelling6, 35, Faction.Dungeon, requires: [DungeonChapelOfStilledVoices]);
        public static readonly Building DungeonDragonCave = new("Dragon Cave", BuildingType.Dwelling7, 36, Faction.Dungeon, requires: [MagesGuild2, DungeonLabyrinth, DungeonManticoreLair]);
        public static readonly Building DungeonUpgHarpyLoft = new("Upg. Harpy Loft", BuildingType.UpgradedDwelling2, 38, Faction.Dungeon, upgrades: DungeonHarpyLoft);
        public static readonly Building DungeonUpgPillarOfEyes = new("Upg. Pillar of Eyes", BuildingType.UpgradedDwelling3, 39, Faction.Dungeon, upgrades: DungeonPillarOfEyes);
        public static readonly Building DungeonUpgChapelOfStilledVoices = new("Upg. Chapel of Stilled Voices", BuildingType.UpgradedDwelling4, 40, Faction.Dungeon, upgrades: DungeonChapelOfStilledVoices);
        public static readonly Building DungeonUpgLabyrinth = new("Upg. Labyrinth", BuildingType.UpgradedDwelling5, 41, Faction.Dungeon, upgrades: DungeonLabyrinth);
        public static readonly Building DungeonUpgManticoreLair = new("Upg. Manticore Lair", BuildingType.UpgradedDwelling6, 42, Faction.Dungeon, upgrades: DungeonManticoreLair);
        public static readonly Building DungeonUpgDragonCave = new("Upg. Dragon Cave", BuildingType.UpgradedDwelling7, 43, Faction.Dungeon, upgrades: DungeonDragonCave, requires: [MagesGuild3]);

        // Stronghold
        public static readonly Building StrongholdEscapeTunnel = new("Escape Tunnel", BuildingType.Special1, 17, Faction.Stronghold, requires: [Fort]);
        public static readonly Building StrongholdGoblinBarracks = new("Goblin Barracks", BuildingType.Dwelling1, 30, Faction.Stronghold, requires: [Fort]);
        public static readonly Building StrongholdMessHall = new("Mess Hall", BuildingType.Horde1, 18, Faction.Stronghold, upgrades: StrongholdGoblinBarracks);
        public static readonly Building StrongholdUpgGoblinBarracks = new("Upg. Goblin Barracks", BuildingType.UpgradedDwelling1, 37, Faction.Stronghold, upgrades: StrongholdGoblinBarracks);
        public static readonly Building StrongholdUpgMessHall = new("Upg. Mess Hall", BuildingType.UpgradedHorde1, 19, Faction.Stronghold, upgrades: StrongholdUpgGoblinBarracks, requires: [StrongholdMessHall]);
        public static readonly Building StrongholdFreelancersGuild = new("Freelancer's Guild", BuildingType.Special2, 21, Faction.Stronghold, requires: [Marketplace]);
        public static readonly Building StrongholdBallistaYard = new("Ballista Yard", BuildingType.Special3, 22, Faction.Stronghold, requires: [Blacksmith]);
        public static readonly Building StrongholdHallOfValhalla = new("Hall of Valhalla", BuildingType.Special4, 23, Faction.Stronghold, requires: [Fort]);
        public static readonly Building StrongholdWarlordsMonument = new("Warlords' Monument", BuildingType.Grail, 26, Faction.Stronghold);
        public static readonly Building StrongholdWolfPen = new("Wolf Pen", BuildingType.Dwelling2, 31, Faction.Stronghold, requires: [StrongholdGoblinBarracks]);
        public static readonly Building StrongholdOrcTower = new("Orc Tower", BuildingType.Dwelling3, 32, Faction.Stronghold, requires: [StrongholdGoblinBarracks]);
        public static readonly Building StrongholdOgreFort = new("Ogre Fort", BuildingType.Dwelling4, 33, Faction.Stronghold, requires: [StrongholdOrcTower]);
        public static readonly Building StrongholdCliffNest = new("Cliff Nest", BuildingType.Dwelling5, 34, Faction.Stronghold, requires: [StrongholdWolfPen]);
        public static readonly Building StrongholdCyclopsCave = new("Cyclops Cave", BuildingType.Dwelling6, 35, Faction.Stronghold, requires: [StrongholdOgreFort]);
        public static readonly Building StrongholdBehemothLair = new("Behemoth Lair", BuildingType.Dwelling7, 36, Faction.Stronghold, requires: [StrongholdCliffNest]);
        public static readonly Building StrongholdUpgWolfPen = new("Upg. Wolf Pen", BuildingType.UpgradedDwelling2, 38, Faction.Stronghold, upgrades: StrongholdWolfPen, requires: [StrongholdUpgGoblinBarracks]);
        public static readonly Building StrongholdUpgOrcTower = new("Upg. Orc Tower", BuildingType.UpgradedDwelling3, 39, Faction.Stronghold, upgrades: StrongholdOrcTower, requires: [Blacksmith]);
        public static readonly Building StrongholdUpgOgreFort = new("Upg. Ogre Fort", BuildingType.UpgradedDwelling4, 40, Faction.Stronghold, upgrades: StrongholdOgreFort, requires: [MagesGuild1]);
        public static readonly Building StrongholdUpgCliffNest = new("Upg. Cliff Nest", BuildingType.UpgradedDwelling5, 41, Faction.Stronghold, upgrades: StrongholdCliffNest);
        public static readonly Building StrongholdUpgCyclopsCave = new("Upg. Cyclops Cave", BuildingType.UpgradedDwelling6, 42, Faction.Stronghold, upgrades: StrongholdCyclopsCave);
        public static readonly Building StrongholdUpgBehemothLair = new("Upg. Behemoth Lair", BuildingType.UpgradedDwelling7, 43, Faction.Stronghold, upgrades: StrongholdBehemothLair);

        // Fortress
        public static readonly Building FortressGlyphsOfFear = new("Glyphs of Fear", BuildingType.Special2, 21, Faction.Fortress, requires: [Fort]);
        public static readonly Building FortressCaptainsQuarters = new("Captain's Quarters", BuildingType.Special1, 17, Faction.Fortress, requires: [TownHall, FortressGlyphsOfFear]);
        public static readonly Building FortressGnollHut = new("Gnoll Hut", BuildingType.Dwelling1, 30, Faction.Fortress, requires: [Fort]);
        public static readonly Building FortressCageOfWarlords = new("Cage of Warlords", BuildingType.Horde1, 18, Faction.Fortress, upgrades: FortressGnollHut);
        public static readonly Building FortressUpgGnollHut = new("Upg. Gnoll Hut", BuildingType.UpgradedDwelling1, 37, Faction.Fortress, upgrades: FortressGnollHut, requires: [Tavern]);
        public static readonly Building FortressUpgCageOfWarlords = new("Upg. Cage of Warlords", BuildingType.UpgradedHorde1, 19, Faction.Fortress, upgrades: FortressUpgGnollHut, requires: [FortressCageOfWarlords]);
        public static readonly Building FortressBloodObelisk = new("Blood Obelisk", BuildingType.Special3, 22, Faction.Fortress, requires: [FortressGlyphsOfFear]);
        public static readonly Building FortressCarnivorousPlant = new("Carnivorous Plant", BuildingType.Grail, 26, Faction.Fortress);
        public static readonly Building FortressLizardDen = new("Lizard Den", BuildingType.Dwelling2, 31, Faction.Fortress, requires: [FortressGnollHut]);
        public static readonly Building FortressSerpentFlyHive = new("Serpent Fly Hive", BuildingType.Dwelling3, 32, Faction.Fortress, requires: [FortressGnollHut]);
        public static readonly Building FortressBasiliskPit = new("Basilisk Pit", BuildingType.Dwelling4, 33, Faction.Fortress, requires: [FortressSerpentFlyHive]);
        public static readonly Building FortressGorgonLair = new("Gorgon Lair", BuildingType.Dwelling5, 34, Faction.Fortress, requires: [FortressLizardDen, FortressSerpentFlyHive]);
        public static readonly Building FortressWyvernNest = new("Wyvern Nest", BuildingType.Dwelling6, 35, Faction.Fortress, requires: [FortressLizardDen]);
        public static readonly Building FortressHydraPond = new("Hydra Pond", BuildingType.Dwelling7, 36, Faction.Fortress, requires: [FortressBasiliskPit, FortressWyvernNest]);
        public static readonly Building FortressUpgLizardDen = new("Upg. Lizard Den", BuildingType.UpgradedDwelling2, 38, Faction.Fortress, upgrades: FortressLizardDen);
        public static readonly Building FortressUpgSerpentFlyHive = new("Upg. Serpent Fly Hive", BuildingType.UpgradedDwelling3, 39, Faction.Fortress, upgrades: FortressSerpentFlyHive);
        public static readonly Building FortressUpgBasiliskPit = new("Upg. Basilisk Pit", BuildingType.UpgradedDwelling4, 40, Faction.Fortress, upgrades: FortressBasiliskPit);
        public static readonly Building FortressUpgGorgonLair = new("Upg. Gorgon Lair", BuildingType.UpgradedDwelling5, 41, Faction.Fortress, upgrades: FortressGorgonLair, requires: [ResourceSilo]);
        public static readonly Building FortressUpgWyvernNest = new("Upg. Wyvern Nest", BuildingType.UpgradedDwelling6, 42, Faction.Fortress, upgrades: FortressWyvernNest);
        public static readonly Building FortressUpgHydraPond = new("Upg. Hydra Pond", BuildingType.UpgradedDwelling7, 43, Faction.Fortress, upgrades: FortressHydraPond);

        // Conflux
        public static readonly Building ConfluxArtifactMerchants = new("Artifact Merchants", BuildingType.Special1, 17, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [Marketplace]);
        public static readonly Building ConfluxMagicLantern = new("Magic Lantern", BuildingType.Dwelling1, 30, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [Fort]);
        public static readonly Building ConfluxGardenOfLife = new("Garden of Life", BuildingType.Horde1, 18, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxMagicLantern);
        public static readonly Building ConfluxUpgMagicLantern = new("Upg. Magic Lantern", BuildingType.UpgradedDwelling1, 37, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxMagicLantern);
        public static readonly Building ConfluxUpgGardenOfLife = new("Upg. Garden of Life", BuildingType.UpgradedHorde1, 19, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxUpgMagicLantern, requires: [ConfluxGardenOfLife]);
        public static readonly Building ConfluxMagicUniversity = new("Magic University", BuildingType.Special2, 21, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [MagesGuild1]);
        public static readonly Building ConfluxAuroraBorealis = new("Aurora Borealis", BuildingType.Grail, 26, Faction.Conflux, GameVersion.ArmageddonsBlade);
        public static readonly Building ConfluxAltarOfAir = new("Altar of Air", BuildingType.Dwelling2, 31, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [ConfluxMagicLantern, MagesGuild1]);
        public static readonly Building ConfluxAltarOfWater = new("Altar of Water", BuildingType.Dwelling3, 32, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [ConfluxMagicLantern, MagesGuild1]);
        public static readonly Building ConfluxAltarOfFire = new("Altar of Fire", BuildingType.Dwelling4, 33, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [ConfluxAltarOfAir]);
        public static readonly Building ConfluxAltarOfEarth = new("Altar of Earth", BuildingType.Dwelling5, 34, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [ConfluxAltarOfWater]);
        public static readonly Building ConfluxAltarOfThought = new("Altar of Thought", BuildingType.Dwelling6, 35, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [ConfluxAltarOfFire, ConfluxAltarOfEarth]);
        public static readonly Building ConfluxPyre = new("Pyre", BuildingType.Dwelling7, 36, Faction.Conflux, GameVersion.ArmageddonsBlade, requires: [ConfluxAltarOfThought]);
        public static readonly Building ConfluxUpgAltarOfAir = new("Upg. Altar of Air", BuildingType.UpgradedDwelling2, 38, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxAltarOfAir);
        public static readonly Building ConfluxUpgAltarOfWater = new("Upg. Altar of Water", BuildingType.UpgradedDwelling3, 39, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxAltarOfWater);
        public static readonly Building ConfluxUpgAltarOfFire = new("Upg. Altar of Fire", BuildingType.UpgradedDwelling4, 40, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxAltarOfFire, requires: [ConfluxAltarOfAir]);
        public static readonly Building ConfluxUpgAltarOfEarth = new("Upg. Altar of Earth", BuildingType.UpgradedDwelling5, 41, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxAltarOfEarth);
        public static readonly Building ConfluxUpgAltarOfThought = new("Upg. Altar of Thought", BuildingType.UpgradedDwelling6, 42, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxAltarOfThought, requires: [MagesGuild2]);
        public static readonly Building ConfluxUpgPyre = new("Upg. Pyre", BuildingType.UpgradedDwelling7, 43, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxPyre);
        public static readonly Building ConfluxVaultOfAshes = new("Vault of Ashes", BuildingType.Horde2, 24, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxPyre);
        public static readonly Building ConfluxUpgVaultOfAshes = new("Upg. Vault of Ashes", BuildingType.UpgradedHorde2, 25, Faction.Conflux, GameVersion.ArmageddonsBlade, upgrades: ConfluxUpgPyre, requires: [ConfluxVaultOfAshes]);

        // Horn of the Abyss factions. Their bits are known; the names come from the building trees of https://heroes.thelazy.net and some
        // prerequisites may still be missing.
        // Cove
        public static readonly Building CoveThievesGuild = new("Thieves' Guild", BuildingType.Special3, 22, Faction.Cove, GameVersion.HornOfTheAbyss);
        public static readonly Building CoveGrotto = new("Grotto", BuildingType.Special4, 23, Faction.Cove, GameVersion.HornOfTheAbyss, requires: [Marketplace]);
        public static readonly Building CoveLodestar = new("Lodestar", BuildingType.Grail, 26, Faction.Cove, GameVersion.HornOfTheAbyss);
        public static readonly Building CoveNymphWaterfall = new("Nymph Waterfall", BuildingType.Dwelling1, 30, Faction.Cove, GameVersion.HornOfTheAbyss);
        public static readonly Building CoveUpgNymphWaterfall = new("Upg. Nymph Waterfall", BuildingType.UpgradedDwelling1, 37, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveNymphWaterfall);
        public static readonly Building CoveShack = new("Shack", BuildingType.Dwelling2, 31, Faction.Cove, GameVersion.HornOfTheAbyss);
        public static readonly Building CoveUpgShack = new("Upg. Shack", BuildingType.UpgradedDwelling2, 38, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveShack);
        public static readonly Building CovePub = new("Pub", BuildingType.Horde1, 18, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveShack);
        public static readonly Building CoveFrigate = new("Frigate", BuildingType.Dwelling3, 32, Faction.Cove, GameVersion.HornOfTheAbyss);
        public static readonly Building CoveUpgFrigate = new("Upg. Frigate", BuildingType.UpgradedDwelling3, 39, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveFrigate);
        public static readonly Building CoveGunpowderWarehouse = new("Gunpowder Warehouse", BuildingType.Special2, 21, Faction.Cove, GameVersion.HornOfTheAbyss);
        public static readonly Building CoveNest = new("Nest", BuildingType.Dwelling4, 33, Faction.Cove, GameVersion.HornOfTheAbyss);
        public static readonly Building CoveRoost = new("Roost", BuildingType.Horde2, 24, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveNest);
        public static readonly Building CoveUpgNest = new("Upg. Nest", BuildingType.UpgradedDwelling4, 40, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveNest);
        public static readonly Building CoveUpgPub = new("Upg. Pub", BuildingType.UpgradedHorde1, 19, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveUpgShack, requires: [CovePub]);
        public static readonly Building CoveUpgRoost = new("Upg. Roost", BuildingType.UpgradedHorde2, 25, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveUpgNest, requires: [CoveRoost]);
        public static readonly Building CoveToweroftheSeas = new("Tower of the Seas", BuildingType.Dwelling5, 34, Faction.Cove, GameVersion.HornOfTheAbyss, requires: [MagesGuild1]);
        public static readonly Building CoveUpgToweroftheSeas = new("Upg. Tower of the Seas", BuildingType.UpgradedDwelling5, 41, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveToweroftheSeas);
        public static readonly Building CoveNixFort = new("Nix Fort", BuildingType.Dwelling6, 35, Faction.Cove, GameVersion.HornOfTheAbyss);
        public static readonly Building CoveUpgNixFort = new("Upg. Nix Fort", BuildingType.UpgradedDwelling6, 42, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveNixFort);
        public static readonly Building CoveMaelstrom = new("Maelstrom", BuildingType.Dwelling7, 36, Faction.Cove, GameVersion.HornOfTheAbyss, requires: [CoveToweroftheSeas]);
        public static readonly Building CoveUpgMaelstrom = new("Upg. Maelstrom", BuildingType.UpgradedDwelling7, 43, Faction.Cove, GameVersion.HornOfTheAbyss, upgrades: CoveMaelstrom);

        // Factory
        public static readonly Building FactoryBank = new("Bank", BuildingType.Special2, 21, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryManaGenerator = new("Mana Generator", BuildingType.ManaGenerator, 6, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryArtifactMerchants = new("Artifact Merchants", BuildingType.Special1, 17, Faction.Factory, GameVersion.HornOfTheAbyss, requires: [Marketplace]);
        public static readonly Building FactoryLightningRod = new("Lightning Rod", BuildingType.Grail, 26, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryHalflingAdobe = new("Halfling Adobe", BuildingType.Dwelling1, 30, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryUpgHalflingAdobe = new("Upg. Halfling Adobe", BuildingType.UpgradedDwelling1, 37, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryHalflingAdobe);
        public static readonly Building FactoryFoundry = new("Foundry", BuildingType.Dwelling2, 31, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryUpgFoundry = new("Upg. Foundry", BuildingType.UpgradedDwelling2, 38, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryFoundry);
        public static readonly Building FactoryRanch = new("Ranch", BuildingType.Dwelling3, 32, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryUpgRanch = new("Upg. Ranch", BuildingType.UpgradedDwelling3, 39, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryRanch);
        public static readonly Building FactoryPen = new("Pen", BuildingType.Horde1, 18, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryRanch);
        public static readonly Building FactoryUpgPen = new("Upg. Pen", BuildingType.UpgradedHorde1, 19, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryUpgRanch, requires: [FactoryPen]);
        public static readonly Building FactoryManufactory = new("Manufactory", BuildingType.Dwelling4, 33, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryUpgManufactory = new("Upg. Manufactory", BuildingType.UpgradedDwelling4, 40, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryManufactory);
        public static readonly Building FactoryCatacombs = new("Catacombs", BuildingType.Dwelling5, 34, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryUpgCatacombs = new("Upg. Catacombs", BuildingType.UpgradedDwelling5, 41, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryCatacombs);
        public static readonly Building FactoryWatchtower = new("Watchtower", BuildingType.Dwelling6, 35, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryUpgWatchtower = new("Upg. Watchtower", BuildingType.UpgradedDwelling6, 42, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryWatchtower);
        public static readonly Building FactorySerpentarium = new("Serpentarium", BuildingType.Dwelling7, 36, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryUpgSerpentarium = new("Upg. Serpentarium", BuildingType.UpgradedDwelling7, 43, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactorySerpentarium);
        public static readonly Building FactoryGantry = new("Gantry", BuildingType.Special3, 22, Faction.Factory, GameVersion.HornOfTheAbyss);
        public static readonly Building FactoryUpgGantry = new("Upg. Gantry", BuildingType.Special4, 23, Faction.Factory, GameVersion.HornOfTheAbyss, upgrades: FactoryGantry);

        // Bulwark
        public static readonly Building BulwarkFair = new("Fair", BuildingType.Special1, 17, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkSieidiOfTheRunes = new("Sieidi of the Runes", BuildingType.Special2, 21, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkAltarOfTheRunes = new("Altar of the Runes", BuildingType.Special3, 22, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkSieidiOfTheRunes);
        public static readonly Building BulwarkSpiritsOfTheForebears = new("Spirits of the Forebears", BuildingType.Grail, 26, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkColliery = new("Colliery", BuildingType.Dwelling1, 30, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkUpgColliery = new("Upg. Colliery", BuildingType.UpgradedDwelling1, 37, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkColliery);
        public static readonly Building BulwarkFrigidSpur = new("Frigid Spur", BuildingType.Dwelling2, 31, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkUpgFrigidSpur = new("Upg. Frigid Spur", BuildingType.UpgradedDwelling2, 38, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkFrigidSpur);
        public static readonly Building BulwarkHalloftheBold = new("Hall of the Bold", BuildingType.Dwelling3, 32, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkUpgHalloftheBold = new("Upg. Hall of the Bold", BuildingType.UpgradedDwelling3, 39, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkHalloftheBold);
        public static readonly Building BulwarkMountainEmbassy = new("Mountain Embassy", BuildingType.Dwelling4, 33, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkUpgMountainEmbassy = new("Upg. Mountain Embassy", BuildingType.UpgradedDwelling4, 40, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkMountainEmbassy);
        public static readonly Building BulwarkShamanFane = new("Shaman Fane", BuildingType.Dwelling5, 34, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkUpgShamanFane = new("Upg. Shaman Fane", BuildingType.UpgradedDwelling5, 41, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkShamanFane);
        public static readonly Building BulwarkMammothStalls = new("Mammoth Stalls", BuildingType.Dwelling6, 35, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkUpgMammothStalls = new("Upg. Mammoth Stalls", BuildingType.UpgradedDwelling6, 42, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkMammothStalls);
        public static readonly Building BulwarkHayBins = new("Hay Bins", BuildingType.Horde1, 18, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkMammothStalls);
        public static readonly Building BulwarkUpgHayBins = new("Upg. Hay Bins", BuildingType.UpgradedHorde1, 19, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkUpgMammothStalls, requires: [BulwarkHayBins]);
        public static readonly Building BulwarkFrosthome = new("Frosthome", BuildingType.Dwelling7, 36, Faction.Bulwark, GameVersion.HornOfTheAbyss);
        public static readonly Building BulwarkUpgFrosthome = new("Upg. Frosthome", BuildingType.UpgradedDwelling7, 43, Faction.Bulwark, GameVersion.HornOfTheAbyss, upgrades: BulwarkFrosthome);

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Faction, IReadOnlyList<Building>> ByFaction = new();

        private readonly IReadOnlyList<Faction> unavailableFor;

        private Building(string name, BuildingType type, byte code, GameVersion gameVersion, Faction? faction, Building? upgrades, Building[]? requires, Faction[]? except)
            : base(name, code, gameVersion)
        {
            this.Type = type;
            this.Faction = faction;
            this.Upgrades = upgrades;
            this.Requires = requires ?? [];
            this.unavailableFor = except ?? [];
        }

        private Building(string name, BuildingType type, byte code, Building? upgrades = null, Building[]? requires = null, Faction[]? except = null)
            : this(name, type, code, GameVersion.RestorationOfErathia, null, upgrades, requires, except)
        {
        }

        private Building(string name, BuildingType type, byte code, Faction faction, Building? upgrades = null, Building[]? requires = null)
            : this(name, type, code, GameVersion.RestorationOfErathia, faction, upgrades, requires, null)
        {
        }

        private Building(string name, BuildingType type, byte code, Faction faction, GameVersion gameVersion, Building? upgrades = null, Building[]? requires = null)
            : this(name, type, code, gameVersion, faction, upgrades, requires, null)
        {
        }

        public static IReadOnlyList<Building> All => GetAll<Building>();

        /// <summary>
        /// Gets what the building is for, whatever its faction calls it.
        /// </summary>
        public BuildingType Type { get; }

        /// <summary>
        /// Gets the faction the building belongs to, or null for a building that every faction can have.
        /// </summary>
        public Faction? Faction { get; }

        /// <summary>
        /// Gets whether this is the Grail structure of its faction (Colossus, Skyship, ...).
        /// </summary>
        public bool IsGrail => this.Type == BuildingType.Grail;

        /// <summary>
        /// Gets whether this is one of the seven creature dwellings of its faction, not counting upgrades.
        /// </summary>
        public bool IsDwelling => this.Type is >= BuildingType.Dwelling1 and <= BuildingType.Dwelling7;

        /// <summary>
        /// Gets whether this is the upgraded form of one of the seven creature dwellings of its faction.
        /// </summary>
        public bool IsUpgradedDwelling => this.Type is >= BuildingType.UpgradedDwelling1 and <= BuildingType.UpgradedDwelling7;

        /// <summary>
        /// Gets whether this is one of the levels of the Mage Guild, which are common to all factions.
        /// </summary>
        public bool IsMageGuild => this.Type is >= BuildingType.MageGuild1 and <= BuildingType.MageGuild5;

        /// <summary>
        /// Gets the level (from 1) of the Mage Guild, or null for any other building.
        /// </summary>
        public int? MageGuildLevel => this.IsMageGuild ? this.Type - BuildingType.MageGuild1 + 1 : null;

        /// <summary>
        /// Gets the level (from 0) of the creatures a dwelling or its upgrade holds, or null for any other building.
        /// </summary>
        public int? DwellingLevel => this.IsDwelling
            ? this.Type - BuildingType.Dwelling1
            : this.IsUpgradedDwelling ? this.Type - BuildingType.UpgradedDwelling1 : null;

        /// <summary>
        /// Gets the building this one is built on top of: the previous hall or Mage Guild level, the fort of a citadel,
        /// the dwelling of its upgrade or of the horde building that boosts it. Null if it is not built on another building.
        /// </summary>
        public Building? Upgrades { get; }

        /// <summary>
        /// Gets the buildings that have to be built first, besides <see cref="Upgrades"/>.
        /// </summary>
        public IReadOnlyList<Building> Requires { get; }

        /// <summary>
        /// Gets every building that has to be built first: <see cref="Upgrades"/> and <see cref="Requires"/>.
        /// </summary>
        public IReadOnlyList<Building> Prerequisites => this.Upgrades == null ? this.Requires : [this.Upgrades, .. this.Requires];

        /// <summary>
        /// Returns the buildings a town of <paramref name="faction"/> can have, ordered by code.
        /// </summary>
        public static IReadOnlyList<Building> ForFaction(Faction faction) =>
            ByFaction.GetOrAdd(faction, f => All.Where(b => b.IsAvailableFor(f)).OrderBy(b => b.Code).ToList());

        /// <summary>
        /// Returns the building of <paramref name="faction"/> whose bit in the building masks is <paramref name="bit"/>, or null.
        /// </summary>
        public static Building? FromBit(Faction faction, byte bit) => ForFaction(faction).FirstOrDefault(b => b.Code == bit);

        /// <summary>
        /// Returns the building of <paramref name="faction"/> that is the given type, or null if it has none.
        /// </summary>
        public static Building? FromType(Faction faction, BuildingType type) => ForFaction(faction).FirstOrDefault(b => b.Type == type);

        /// <summary>
        /// Returns the dwelling of <paramref name="faction"/> for the creatures of the zero-based <paramref name="level"/>, or its upgraded form when <paramref name="upgraded"/> is set.
        /// </summary>
        public static Building? FromDwellingLevel(Faction faction, int level, bool upgraded) =>
            level is < 0 or >= DwellingLevelCount ? null : FromType(faction, (upgraded ? BuildingType.UpgradedDwelling1 : BuildingType.Dwelling1) + level);

        /// <summary>
        /// Gets whether a town of <paramref name="faction"/> can have the building.
        /// Only the nine factions of the original games are known; any other faction is assumed to allow the common buildings.
        /// </summary>
        public bool IsAvailableFor(Faction faction)
        {
            return this.Faction == null ? !this.unavailableFor.Contains(faction) : this.Faction == faction;
        }

        public override bool Equals(AbstractEnumeration? other)
            => other is Building building && this.Code == building.Code && this.Faction == building.Faction && this.Name == building.Name;

        public override int GetHashCode() => System.HashCode.Combine(typeof(Building), this.Code, this.Faction, this.Name);
    }
}
