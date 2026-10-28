using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// castles_00_baseline, castles_01_blacksmith_garrison_tumlac_to_blue and castles_02_cloud_temple are HoTA savefiles of one map, with two castles of tan (Haret and Tumlac).
    /// In castles_01_blacksmith_garrison_tumlac_to_blue Haret got a blacksmith and a garrison and Tumlac changed hands to blue. In castles_02_cloud_temple Tumlac built its cloud temple.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleDeserializerTests
    {
        private static CastleModel Castle(string save, string name)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", save + ".GM1"));
            var castles = game.History.Current.Castles();
            var index = castles.FindByName(name);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), $"{name} in {save}");
            return TestServices.CastleDeserializer.Deserialize(castles[index]);
        }

        [TestCase("castles_00_baseline")]
        [TestCase("castles_01_blacksmith_garrison_tumlac_to_blue")]
        [TestCase("castles_02_cloud_temple")]
        public void BothCastlesAreFound(string save)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", save + ".GM1"));

            Assert.That(game.History.Current.Castles().Select(c => c.Name), Does.Contain("Castle Haret").And.Contain("Castle Tumlac"));
        }

        // The list of castles of a player (CastleLinks, which the player page and the overview show) leaves out a castle the map does not
        // use, as HoMM2's does; the loader of HoMM3 finds only the castles the map has, so every one has a name.
        [Test]
        public void EveryCastleOfEverySavefileHasANameSoTheMapUsesIt()
        {
            foreach (var path in Directory.GetFiles("TestData", "*.GM?", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
            {
                var game = TestServices.GameService.LoadGameFromFile(path);

                Assert.That(game.History.Current.Castles().GetModels().Where(c => !c.IsUsed), Is.Empty, Path.GetFileName(path));
            }
        }

        [Test]
        public void HaretInTheBaselineIsACastleOfTanWithoutGarrison()
        {
            var haret = Castle("castles_00_baseline", "Castle Haret");

            Assert.That(haret.Faction, Is.EqualTo(Faction.Castle));
            Assert.That(haret.Owner, Is.EqualTo(PlayerColor.Tan));
            Assert.That(haret.Position, Is.EqualTo(new MapPosition(33, 23, 0)));
            Assert.That(haret.Garrison.Filled, Is.Empty);
            Assert.That(haret.Buildings, Is.EquivalentTo(new[]
            {
                Building.MagesGuild1,
                Building.Fort,
                Building.VillageHall,
                Building.TownHall,
                Building.CityHall,
                Building.Capitol,
                Building.CastleGuardhouse,
                Building.CastleArchersTower,
                Building.CastleGriffinTower,
            }));
        }

        [Test]
        public void TumlacInTheBaselineIsATowerOfTan()
        {
            var tumlac = Castle("castles_00_baseline", "Castle Tumlac");

            Assert.That(tumlac.Faction, Is.EqualTo(Faction.Tower));
            Assert.That(tumlac.Owner, Is.EqualTo(PlayerColor.Tan));
            Assert.That(tumlac.Position, Is.EqualTo(new MapPosition(9, 29, 0)));
            Assert.That(tumlac.Garrison.Filled, Is.Empty);
            Assert.That(tumlac.Buildings, Is.EquivalentTo(new[] { Building.MagesGuild1, Building.VillageHall, Building.TownHall }));
        }

        [Test]
        public void HaretGotABlacksmithAndAGarrison()
        {
            var before = Castle("castles_00_baseline", "Castle Haret");
            var after = Castle("castles_01_blacksmith_garrison_tumlac_to_blue", "Castle Haret");

            Assert.That(after.Buildings.Except(before.Buildings), Is.EquivalentTo(new[] { Building.Blacksmith }));
            Assert.That(before.Buildings.Except(after.Buildings), Is.Empty);

            var expected = new[] { "Pikeman", "Archer", "Griffin", "Swordsman", "Monk", "Cavalier", "Angel" };
            Assert.That(after.Garrison.Slots, Has.All.Not.Null);
            Assert.That(after.Garrison.Slots.Select(s => s!.Creature.Name), Is.EqualTo(expected));
            Assert.That(after.Garrison.Slots.Select(s => s!.Count), Has.All.EqualTo(123));
        }

        [Test]
        public void TumlacChangedOwnerToBlue()
        {
            Assert.That(Castle("castles_01_blacksmith_garrison_tumlac_to_blue", "Castle Tumlac").Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(Castle("castles_01_blacksmith_garrison_tumlac_to_blue", "Castle Tumlac").Buildings, Is.EquivalentTo(Castle("castles_00_baseline", "Castle Tumlac").Buildings));
        }

        [Test]
        public void TumlacBuiltACloudTemple()
        {
            var before = Castle("castles_01_blacksmith_garrison_tumlac_to_blue", "Castle Tumlac");
            var after = Castle("castles_02_cloud_temple", "Castle Tumlac");

            Assert.That(after.Buildings.Except(before.Buildings), Is.EquivalentTo(new[] { Building.TowerCloudTemple }));
            Assert.That(after.Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void TheVisibleBuildingsLeaveOutWhatWasReplaced()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "castles_00_baseline.GM1"));
            var haret = game.History.Current.Castles()[game.History.Current.Castles().FindByName("Castle Haret")];

            var current = TestServices.CastleDeserializer.DeserializeVisibleBuildings(haret);

            Assert.That(current, Does.Contain(Building.Capitol));
            Assert.That(current, Does.Not.Contain(Building.CityHall).And.Not.Contain(Building.TownHall).And.Not.Contain(Building.VillageHall));
        }
    }
}
