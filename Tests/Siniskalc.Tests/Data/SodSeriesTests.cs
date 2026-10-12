using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The Shadow of Death series in TestData/Sod (saves 0 to 34): a game of the same map as the Restoration of Erathia series with seven colours
    /// in play (all but Teal), in which Red builds its Conflux castle one building per save, from the fort to the Grail structure. Tazar moves in save 1, all the heroes
    /// of Red are put to sleep in save 8, and the date goes from month 1 week 1 day 1 to month 2 week 1 day 6.
    /// </summary>
    [TestFixture]
    internal sealed class SodSeriesTests
    {
        private const ulong IgnoredBits = 1UL << 23;

        private static string Path(string save) => System.IO.Path.Combine("TestData", "Sod", save + ".GM1");

        private static Game Load(string save) => TestServices.GameService.LoadGameFromFile(Path(save));

        private static List<string> Saves() => SeriesSaves.Names("Sod", "conflux");

        private static CastleModel Castle(Game game, string name)
        {
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);
        }

        private static HeroModel Hero(Game game, string name) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name)).Current;

        private static IEnumerable<TestCaseData> Steps()
        {
            var saves = Saves();
            for (var i = 2; i < saves.Count; i++)
            {
                yield return new TestCaseData(saves[i - 1], saves[i]).SetName($"{saves[i]}_after_{saves[i - 1]}");
            }
        }

        [Test]
        public void TheGameHasSevenColoursAllButTealAndRedOwnsAConfluxCastleWithItsVillageHallOnly()
        {
            var game = Load("conflux_00_baseline");

            Assert.That(game.GameVersion, Is.EqualTo(GameVersion.ShadowOfDeath));
            Assert.That(game.History.Current.Players().GetColors(), Is.EqualTo(Enum.GetValues<PlayerColor>().Where(c => c != PlayerColor.Teal)));
            Assert.That(game.History.Current.Castles(), Has.Count.EqualTo(10));

            var conflux = Castle(game, "Conflux");
            Assert.That(conflux.Faction, Is.EqualTo(Faction.Conflux));
            Assert.That(conflux.Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(conflux.Position, Is.EqualTo(new Position(50, 4, 0)));
            Assert.That(conflux.Buildings, Is.EquivalentTo(new[] { Building.VillageHall }));
            Assert.That(Castle(game, "8_Fort").Owner, Is.EqualTo(PlayerColor.Pink));
            Assert.That(Castle(game, "Marishen").Owner, Is.Null);
        }

        [TestCase("conflux_02_fort", "Fort")]
        [TestCase("conflux_03_citadel", "Citadel")]
        [TestCase("conflux_04_castle", "Castle")]
        [TestCase("conflux_05_tavern", "Tavern")]
        [TestCase("conflux_06_blacksmith", "Blacksmith")]
        [TestCase("conflux_07_marketplace", "Marketplace")]
        [TestCase("conflux_08_resource_silo", "Resource Silo")]
        [TestCase("conflux_09_mage_guild_1", "Mage Guild, level 1")]
        [TestCase("conflux_10_mage_guild_2", "Mage Guild, level 2")]
        [TestCase("conflux_11_mage_guild_3", "Mage Guild, level 3")]
        [TestCase("conflux_12_mage_guild_4", "Mage Guild, level 4")]
        [TestCase("conflux_13_mage_guild_5", "Mage Guild, level 5")]
        [TestCase("conflux_14_town_hall", "Town Hall")]
        [TestCase("conflux_15_city_hall", "City Hall")]
        [TestCase("conflux_16_capitol", "Capitol")]
        [TestCase("conflux_17_magic_university", "Magic University")]
        [TestCase("conflux_18_artifact_merchants", "Artifact Merchants")]
        [TestCase("conflux_19_dwelling_1", "Magic Lantern")]
        [TestCase("conflux_20_dwelling_2", "Altar of Air")]
        [TestCase("conflux_21_dwelling_3", "Altar of Water")]
        [TestCase("conflux_22_dwelling_4", "Altar of Fire")]
        [TestCase("conflux_23_dwelling_5", "Altar of Earth")]
        [TestCase("conflux_24_dwelling_6", "Altar of Thought")]
        [TestCase("conflux_25_dwelling_7", "Pyre")]
        [TestCase("conflux_26_garden_of_life", "Garden of Life")]
        [TestCase("conflux_27_upg_dwelling_1", "Upg. Magic Lantern", "Upg. Garden of Life")]
        [TestCase("conflux_28_upg_dwelling_2", "Upg. Altar of Air")]
        [TestCase("conflux_29_upg_dwelling_3", "Upg. Altar of Water")]
        [TestCase("conflux_30_upg_dwelling_4", "Upg. Altar of Fire")]
        [TestCase("conflux_31_upg_dwelling_5", "Upg. Altar of Earth")]
        [TestCase("conflux_32_upg_dwelling_6", "Upg. Altar of Thought")]
        [TestCase("conflux_33_upg_dwelling_7", "Upg. Pyre")]
        [TestCase("conflux_34_grail", "Aurora Borealis")]
        public void EachSaveAddsOneBuildingToTheConfluxCastle(string save, params string[] added)
        {
            var saves = Saves();
            var before = Castle(Load(saves[saves.IndexOf(save) - 1]), "Conflux");
            var after = Castle(Load(save), "Conflux");

            // The game adds the upgraded horde building by itself (bits 19 and 25) when the horde building and the upgraded dwelling both stand.
            Assert.That(after.Buildings.Except(before.Buildings).Select(b => b.Name), Is.EquivalentTo(added));
        }

        [TestCase("conflux_08_resource_silo", 0, "Resource Silo")]
        [TestCase("conflux_09_mage_guild_1", 1, null)]
        [TestCase("conflux_13_mage_guild_5", 5, null)]
        public void TheMageGuildLevelOfTheConfluxCastleFollowsTheBuilding(string save, int level, string? other)
        {
            var castle = Castle(Load(save), "Conflux");

            Assert.That(castle.MageGuildLevel, Is.EqualTo(level));
            Assert.That(other == null || castle.Buildings.Select(b => b.Name).Contains(other), Is.True);
        }

        [TestCase("conflux_19_dwelling_1", "20,0,0,0,0,0,0", "0,0,0,0,0,0,0")]
        [TestCase("conflux_21_dwelling_3", "20,6,6,0,0,0,0", "0,0,0,0,0,0,0")]
        [TestCase("conflux_22_dwelling_4", "60,18,18,5,0,0,0", "0,0,0,0,0,0,0")]
        [TestCase("conflux_25_dwelling_7", "60,18,18,5,4,2,2", "0,0,0,0,0,0,0")]
        [TestCase("conflux_27_upg_dwelling_1", "0,18,18,5,4,2,2", "60,0,0,0,0,0,0")]
        [TestCase("conflux_29_upg_dwelling_3", "0,0,0,15,12,6,6", "110,30,30,0,0,0,0")]
        [TestCase("conflux_33_upg_dwelling_7", "0,0,0,0,0,0,0", "110,30,30,15,12,6,6")]
        public void TheCreaturesWaitingInTheConfluxDwellingsFollowTheWeeklyGrowth(string save, string waiting, string upgraded)
        {
            var castle = Castle(Load(save), "Conflux");

            Assert.That(string.Join(",", castle.CreaturesAvailable), Is.EqualTo(waiting));
            Assert.That(string.Join(",", castle.UpgradedCreaturesAvailable), Is.EqualTo(upgraded));
        }

        [Test]
        public void TheConfluxGrailBuildingIsAuroraBorealisAndTazarVisitsTheCastleThen()
        {
            var game = Load("conflux_34_grail");
            var conflux = Castle(game, "Conflux");

            Assert.That(conflux.Buildings, Does.Contain(Building.ConfluxAuroraBorealis));
            Assert.That(conflux.VisitingHeroId, Is.EqualTo(Hero(game, "Tazar").HeroId));
            Assert.That(Hero(game, "Tazar").Position, Is.EqualTo(new HeroPosition(50, 4, 0, FacingDirection.NorthEast)));
        }

        [TestCaseSource(nameof(Steps))]
        public void BuildingWhatTheGameBuiltInTheConfluxCastleGivesTheMasksTheGameWrote(string before, string after)
        {
            var from = Load(before).History.Current.Castles();
            var to = Load(after).History.Current.Castles();
            var chunk = from[from.FindByName("Conflux")];
            var actual = to[to.FindByName("Conflux")];
            var model = TestServices.CastleDeserializer.Deserialize(chunk);
            var added = TestServices.CastleDeserializer.Deserialize(actual).Buildings.Except(model.Buildings).Where(b => !CastleBuilder.IsAutomatic(b)).ToList();

            Assert.That(added, Has.Count.EqualTo(1), after);
            var edited = TestServices.CastleSerializer.Serialize(chunk, CastleBuilder.Build(model, added[0]));

            Assert.That(Masks(edited), Is.EqualTo(Masks(actual)), added[0].Name);
        }

        private static ulong[] Masks(CastleChunk chunk) =>
        [
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.CurrentBuildingsFromNameEnd) & ~IgnoredBits,
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.BuiltBuildingsFromNameEnd) & ~IgnoredBits,
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.EnabledBuildingsFromNameEnd) & ~IgnoredBits,
        ];

        [Test]
        public void TheTazarOfTheFirstSaveHasMovedAndTheGameSetsNoFlagForIt()
        {
            var before = Load("conflux_00_baseline");
            var after = Load("conflux_01_tazar_wins_boots_tomes");
            var tazar = Hero(before, "Tazar");
            var moved = Hero(after, "Tazar");

            Assert.That(tazar.Position, Is.EqualTo(new HeroPosition(44, 7, 0, FacingDirection.East)));
            Assert.That(moved.Position, Is.EqualTo(new HeroPosition(22, 10, 0, FacingDirection.SouthWest)));
            Assert.That((tazar.MovementPoints.CurrentMovementPoints, tazar.MovementPoints.MaxMovementPoints), Is.EqualTo((2900, 2900)));
            Assert.That((moved.MovementPoints.CurrentMovementPoints, moved.MovementPoints.MaxMovementPoints), Is.EqualTo((2318, 2900)));

            // The flag of "moved today" the editor reads (a Horn of the Abyss byte) is beyond the hero here and says nothing.
            Assert.That(tazar.MovedToday, Is.False);
            Assert.That(moved.MovedToday, Is.False);
        }

        [Test]
        public void NoByteOfAHeroRecordSeparatesTheHeroesThatMovedFromTheOnesThatDidNotExceptTheirMovementPoints()
        {
            // Every hero of a player in every Shadow of Death savefile of the repository: a hero has moved when it has fewer movement points than its maximum.
            var seen = new Dictionary<int, (HashSet<byte> Moved, HashSet<byte> Unmoved)>();
            var moved = 0;
            foreach (var path in Directory.GetFiles("TestData", "*.GM1", SearchOption.AllDirectories).Where(p => p.Contains("Sod") || p.Contains("Roe")))
            {
                var game = TestServices.GameService.LoadGameFromFile(path);
                var heroes = game.History.Current.Heroes();
                for (var i = 0; i < heroes.Count; i++)
                {
                    var chunk = heroes[i];
                    var maximum = chunk.GetUInt32At(57);
                    if (chunk.GetByteAt(26) == 0xFF || maximum == 0 || maximum > 100_000)
                    {
                        continue;
                    }

                    var hasMoved = chunk.GetUInt32At(61) < maximum;
                    moved += hasMoved ? 1 : 0;
                    for (var offset = 0; offset < chunk.Length; offset++)
                    {
                        if (!seen.TryGetValue(offset, out var sets))
                        {
                            seen[offset] = sets = ([], []);
                        }

                        (hasMoved ? sets.Moved : sets.Unmoved).Add(chunk.GetByteAt(offset));
                    }
                }
            }

            var separating = seen.Where(p => p.Value.Moved.Count > 0 && p.Value.Unmoved.Count > 0 && !p.Value.Moved.Overlaps(p.Value.Unmoved)).Select(p => p.Key).ToList();

            Assert.That(moved, Is.GreaterThan(30));
            Assert.That(separating, Is.All.InRange(61, 64), "only the current movement points tell");
        }

        [Test]
        public void AllTheHeroesOfRedAreAsleepInSaveEightAndNoneBeforeIt()
        {
            string[] red = ["Ufretin", "Torosar ", "Nymus", "Damacon", "Crag Hack", "Tazar", "Sir Mullich"];

            Assert.That(red.Where(n => Hero(Load("conflux_07_marketplace"), n).IsAsleep), Is.Empty);
            Assert.That(red.Where(n => !Hero(Load("conflux_08_resource_silo"), n).IsAsleep), Is.Empty);
        }

        [Test]
        public void TheHeroesThatTheComputerPlayersKeepInTheirCastlesAreAsleep()
        {
            var game = Load("conflux_01_tazar_wins_boots_tomes");
            var fort = Castle(game, "8_Fort");

            Assert.That(game.History.Current.Heroes().FindById(fort.GarrisonedHeroId!.Value)!.Name, Is.EqualTo("Styg"));
            Assert.That(Hero(game, "Styg").Owner, Is.EqualTo(PlayerColor.Pink));
            Assert.That(Hero(game, "Styg").IsAsleep, Is.True);
        }

        [Test]
        public void TazarCarriesEightGrailsInTheFirstSaveAndWinsArtifactsInTheSecond()
        {
            var before = Hero(Load("conflux_00_baseline"), "Tazar");
            var after = Hero(Load("conflux_01_tazar_wins_boots_tomes"), "Tazar");

            Assert.That(before.Backpack.All.OfType<BackpackSlot>().Select(s => s.Art), Is.EqualTo(Enumerable.Repeat(Artifact.TheGrail, 8)));
            Assert.That(before.EquippedArtifacts.GetEquippedArtifacts().Select(t => t.Item2), Is.EqualTo(new[] { Artifact.EquestriansGloves }));
            Assert.That(after.EquippedArtifacts.GetArtifact(ArtifactSlots.Helmet), Is.EqualTo(Artifact.SpellbindersHat));
            Assert.That(after.EquippedArtifacts.GetArtifact(ArtifactSlots.Cape), Is.EqualTo(Artifact.AngelWings));
            Assert.That(after.EquippedArtifacts.GetArtifact(ArtifactSlots.Feet), Is.EqualTo(Artifact.BootsOfLevitation));
            Assert.That(after.EquippedArtifacts.GetEquippedArtifacts().Select(t => t.Item2), Does.Contain(Artifact.TomeOfFireMagic).And.Contain(Artifact.TomeOfWaterMagic));
        }

        [TestCase(PlayerColor.Red, "conflux_00_baseline", 77, 123)]
        [TestCase(PlayerColor.Red, "conflux_07_marketplace", 77, 123)]
        [TestCase(PlayerColor.Red, "conflux_08_resource_silo", 64, 12)]
        [TestCase(PlayerColor.Red, "conflux_14_town_hall", 64, 12)]
        [TestCase(PlayerColor.Red, "conflux_15_city_hall", 64, 2)]
        [TestCase(PlayerColor.Red, "conflux_22_dwelling_4", 75, 111)]
        [TestCase(PlayerColor.Red, "conflux_29_upg_dwelling_3", 76, 119)]
        [TestCase(PlayerColor.Blue, "conflux_00_baseline", 31, 127)]
        [TestCase(PlayerColor.Blue, "conflux_08_resource_silo", 16, 75)]
        [TestCase(PlayerColor.Pink, "conflux_29_upg_dwelling_3", 118, 142)]
        public void TheTavernsOfAllColoursOfTheGameChangeAtTheStartOfAWeek(PlayerColor color, string save, int first, int second)
        {
            var game = Load(save);
            var tavern = game.History.Current.Players().GetModel(game.History.Current.Players().FindByColor(color)).Tavern;

            Assert.That(tavern, Is.EqualTo(new TavernHeroes(first, second)));
        }

        [Test]
        public void AHeroThatWasHiredIsReplacedInItsPlaceOfTheTavern()
        {
            // Pink hired Styg (id 125) in save 1 and Sandro (74) in save 3; Tan hired Cyra (46) in save 34. The other place is the same before and after.
            TavernHeroes Tavern(string save, PlayerColor color)
            {
                var game = Load(save);
                return game.History.Current.Players().GetModel(game.History.Current.Players().FindByColor(color)).Tavern;
            }

            Assert.That(Tavern("conflux_00_baseline", PlayerColor.Pink), Is.EqualTo(new TavernHeroes(125, 104)));
            Assert.That(Tavern("conflux_01_tazar_wins_boots_tomes", PlayerColor.Pink), Is.EqualTo(new TavernHeroes(74, 104)));
            Assert.That(Tavern("conflux_03_citadel", PlayerColor.Pink), Is.EqualTo(new TavernHeroes(30, 104)));
            Assert.That(Tavern("conflux_33_upg_dwelling_7", PlayerColor.Tan), Is.EqualTo(new TavernHeroes(46, 101)));
            Assert.That(Tavern("conflux_34_grail", PlayerColor.Tan), Is.EqualTo(new TavernHeroes(117, 101)));
            Assert.That(Hero(Load("conflux_01_tazar_wins_boots_tomes"), "Styg").Owner, Is.EqualTo(PlayerColor.Pink));
            Assert.That(Hero(Load("conflux_03_citadel"), "Sandro").Owner, Is.EqualTo(PlayerColor.Pink));
            Assert.That(Hero(Load("conflux_34_grail"), "Cyra").Owner, Is.EqualTo(PlayerColor.Tan));
        }

        [Test]
        public void TheObelisksOfTheMapAreFiveAndNoneIsVisitedInTheWholeSeries()
        {
            foreach (var save in Saves())
            {
                Assert.That(Load(save).History.Current.Obelisks().GetModel(), Is.EqualTo(ObeliskVisits.NoneVisited(5)), save);
            }
        }
    }
}
