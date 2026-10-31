using static Siniskalc.Homm3.Tests.Data.CastleMasks;
using Siniskalc.Homm3.Tests.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// The rules of <see cref="CastleBuilder"/> one at a time, on the Rampart castle "Still Water" and the Bulwark castle
    /// "Jotunngard" of the bulwark_* savefiles and the Rampart castle "Fortune Keep".
    /// </summary>
    [Category(TestKinds.Integration)]
    internal sealed class CastleBuilderSavefileRulesTests
    {
        [Test]
        public void RemovingABuildingGivesTheMasksOfNeverHavingBuiltIt()
        {
            var removals = 0;
            var failures = new List<string>();
            foreach (var path in CastleSamples.All().Where(p => VerifiedFactions.Any(f => Path.GetDirectoryName(p)!.EndsWith(f.Name)) && !p.EndsWith("cove_inland_all_cheat.GM1")))
            {
                var castles = SavedGames.Get(path).History.Current.Castles()
                    .Select(chunk => (Chunk: chunk, Model: TestServices.CastleDeserializer.Deserialize(chunk)))
                    .Where(c => VerifiedFactions.Contains(c.Model.Faction)).ToList();
                Assert.That(castles, Is.Not.Empty, $"{path}: at least one castle has a verified faction");
                foreach (var (chunk, saved) in castles)
                {
                    foreach (var building in saved.Buildings.Where(b => CastleBuilder.GetUnbuildFailureReason(saved, b) == CastleBuildFailureReason.None))
                    {
                        var unbuilt = CastleBuilder.Unbuild(saved, building);
                        var removed = saved.Buildings.Except(unbuilt.Buildings).ToList();

                        var model = saved with { Buildings = new HashSet<Building> { Building.VillageHall }, VisibleBuildings = new HashSet<Building> { Building.VillageHall } };
                        var empty = TestServices.SerializeCastle(chunk, model);
                        foreach (var kept in saved.Buildings.Except(removed).Where(b => b != Building.VillageHall && !CastleBuilder.IsAutomatic(b)))
                        {
                            model = CastleBuilder.SetBuilt(model, kept, true);
                        }

                        var expected = TestServices.SerializeCastle(empty, model);
                        var actual = TestServices.SerializeCastle(empty, unbuilt);
                        if (Masks(expected) != Masks(actual))
                        {
                            failures.Add($"{Path.GetFileName(path)} {chunk.Name} - {building.Name}: got {Hex(Masks(actual))}, expected {Hex(Masks(expected))}");
                        }

                        removals++;
                    }
                }
            }

            Assert.That(failures, Is.Empty);
            Assert.That(removals, Is.GreaterThan(1000));
        }

        private static CastleModel Castle(string folder, string save, string name)
        {
            var path = CastleSamples.Folder(folder).Single(p => Path.GetFileNameWithoutExtension(p) == save);
            var castles = SavedGames.Get(path).History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);
        }

        [Test]
        public void ADwellingStartsWithOneWeekOfCreatures()
        {
            var before = Castle("Bulwark", "bulwark_13_mage_guild_4", "Still Water");

            var built = CastleBuilder.Build(before, Building.RampartCentaurStables);

            Assert.That(built.CreaturesAvailable[0], Is.EqualTo(14));
            Assert.That(built.CreaturesAvailable, Is.EqualTo(Castle("Bulwark", "bulwark_14_centaur_stables__colliery", "Still Water").CreaturesAvailable));
        }

        [Test]
        public void UpgradingADwellingKeepsTheCreaturesWaiting()
        {
            var before = Castle("Bulwark", "bulwark_24_nothing__fair", "Jotunngard");
            var after = Castle("Bulwark", "bulwark_25_mystic_pond__upg_colliery", "Jotunngard");

            var upgraded = CastleBuilder.Build(before, Building.BulwarkUpgColliery);

            Assert.That(upgraded.CreaturesAvailable, Is.EqualTo(after.CreaturesAvailable));
            Assert.That(upgraded.VisibleBuildings, Is.EquivalentTo(after.VisibleBuildings));

            var back = CastleBuilder.Unbuild(upgraded, Building.BulwarkUpgColliery);
            Assert.That(back.CreaturesAvailable, Is.EqualTo(before.CreaturesAvailable));
        }

        [Test]
        public void RemovingADwellingEmptiesIt()
        {
            var castle = Castle("Bulwark", "bulwark_24_nothing__fair", "Jotunngard");

            var removed = CastleBuilder.Unbuild(castle, Building.BulwarkFrosthome);

            Assert.That(removed.CreaturesAvailable[6], Is.EqualTo(0));
            Assert.That(removed.CreaturesAvailable[5], Is.EqualTo(castle.CreaturesAvailable[5]));
        }

        [TestCase("bulwark_09_mage_guild_1", "bulwark_08_resource_silo", 1)]
        [TestCase("bulwark_11_mage_guild_2", "bulwark_10_city_hall", 2)]
        [TestCase("bulwark_13_mage_guild_4", "bulwark_12_mage_guild_3", 4)]
        public void TheMageGuildLevelAndItsSpellsFollowTheBuilding(string after, string before, int level)
        {
            var next = Castle("Bulwark", after, "Still Water");

            var built = CastleBuilder.SetMageGuildLevel(Castle("Bulwark", before, "Still Water"), level);

            Assert.That(built.MageGuildLevel, Is.EqualTo(level));
            Assert.That(built.MageGuildSpellCounts, Is.EqualTo(next.MageGuildSpellCounts));
        }
    }
}
