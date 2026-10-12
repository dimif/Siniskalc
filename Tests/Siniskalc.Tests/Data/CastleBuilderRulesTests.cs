using static Siniskalc.Tests.Data.CastleMasks;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The rules of <see cref="CastleBuilder"/> one at a time, on the Rampart castle "Still Water" and the Bulwark castle
    /// "Jotunngard" of the bulwark_* savefiles and the Rampart castle "Fortune Keep".
    /// </summary>
    internal sealed class CastleBuilderRulesTests
    {
        [Test]
        public void RemovingABuildingGivesTheMasksOfNeverHavingBuiltIt()
        {
            var removals = 0;
            var failures = new List<string>();
            foreach (var path in CastleSamples.All().Where(p => VerifiedFactions.Any(f => Path.GetDirectoryName(p)!.EndsWith(f.Name)) && !p.EndsWith("cove_inland_all_cheat.GM1")))
            {
                foreach (var chunk in CastleSamples.Load(path).History.Current.Castles())
                {
                    var saved = TestServices.CastleDeserializer.Deserialize(chunk);
                    if (saved.Faction == null || !VerifiedFactions.Contains(saved.Faction))
                    {
                        continue;
                    }

                    foreach (var building in saved.Buildings.Where(b => CastleBuilder.GetUnbuildFailureReason(saved, b) == CastleBuildFailureReason.None))
                    {
                        var unbuilt = CastleBuilder.Unbuild(saved, building);
                        var removed = saved.Buildings.Except(unbuilt.Buildings).ToList();

                        var model = saved with { Buildings = new HashSet<Building> { Building.VillageHall }, CurrentBuildings = new HashSet<Building> { Building.VillageHall } };
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
            var castles = CastleSamples.Load(path).History.Current.Castles();
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
        public void UpgradingADwellingMovesTheCreaturesWaiting()
        {
            var before = Castle("Bulwark", "bulwark_24_nothing__fair", "Jotunngard");
            var after = Castle("Bulwark", "bulwark_25_mystic_pond__upg_colliery", "Jotunngard");

            var upgraded = CastleBuilder.Build(before, Building.BulwarkUpgColliery);

            Assert.That(upgraded.CreaturesAvailable, Is.EqualTo(after.CreaturesAvailable));
            Assert.That(upgraded.UpgradedCreaturesAvailable, Is.EqualTo(after.UpgradedCreaturesAvailable));
            Assert.That(upgraded.CurrentBuildings, Is.EquivalentTo(after.CurrentBuildings));

            var back = CastleBuilder.Unbuild(upgraded, Building.BulwarkUpgColliery);
            Assert.That(back.CreaturesAvailable, Is.EqualTo(before.CreaturesAvailable));
            Assert.That(back.UpgradedCreaturesAvailable, Is.All.EqualTo(0));
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
            Assert.That(built.SpellCounts, Is.EqualTo(next.SpellCounts));
        }

        [Test]
        public void TheMageGuildCanBeBuiltInOneGoAndRemovedAgain()
        {
            var baseline = Castle("Bulwark", "bulwark_08_resource_silo", "Still Water");

            var guild = CastleBuilder.SetMageGuildLevel(baseline, 5);
            Assert.That(guild.SpellCounts, Is.EqualTo(new[] { 5, 4, 3, 2, 1 }));
            Assert.That(guild.CurrentBuildings, Does.Contain(Building.MagesGuild5).And.Not.Contain(Building.MagesGuild4));

            var smaller = CastleBuilder.SetMageGuildLevel(guild, 2);
            Assert.That(smaller.MageGuildLevel, Is.EqualTo(2));
            Assert.That(smaller.SpellCounts, Is.EqualTo(new[] { 5, 4, 0, 0, 0 }));
            Assert.That(smaller.CurrentBuildings, Does.Contain(Building.MagesGuild2).And.Not.Contain(Building.MagesGuild3));

            var none = CastleBuilder.SetMageGuildLevel(smaller, 0);
            Assert.That(none.CurrentBuildings, Is.EquivalentTo(baseline.CurrentBuildings));
            Assert.That(none.SpellCounts, Is.All.EqualTo(0));
        }

        [Test]
        public void TheMageGuildOfBulwarkStopsAtLevelFour()
        {
            var castle = Castle("Bulwark", "bulwark_08_resource_silo", "Jotunngard");

            Assert.That(CastleBuilder.SetMageGuildLevel(castle, 4).MageGuildLevel, Is.EqualTo(4));
            Assert.Throws<GameRuleException>(() => CastleBuilder.SetMageGuildLevel(castle, 5));
        }

        [Test]
        public void AnUpgradeNeedsItsBaseBuildingAndTheMapsPermission()
        {
            var castle = Castle("Bulwark", "bulwark_00_baseline", "Still Water");

            Assert.That(TestServices.CastleBuildMessages.DescribeBuild(castle, Building.Citadel), Is.EqualTo("Requires Fort"));
            Assert.That(TestServices.CastleBuildMessages.DescribeBuild(castle, Building.CityHall), Does.StartWith("Requires"));
            Assert.That(TestServices.CastleBuildMessages.DescribeBuild(castle, Building.VillageHall), Is.EqualTo("Already built"));
            Assert.That(CastleBuilder.GetBuildFailureReason(castle, Building.Fort), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.That(TestServices.CastleBuildMessages.DescribeBuild(castle, Building.CastleGuardhouse), Is.EqualTo("This town cannot have it"));
            Assert.That(TestServices.CastleBuildMessages.DescribeBuild(castle, Building.RampartUpgMinersGuild), Does.StartWith("The game builds it by itself"));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Build(castle, Building.Citadel));

            var forbidden = castle with { EnabledBuildings = castle.EnabledBuildings.Except([Building.Fort]).ToHashSet() };
            Assert.That(TestServices.CastleBuildMessages.DescribeBuild(forbidden, Building.Fort), Is.EqualTo("Not allowed by the map"));
        }

        [Test]
        public void ABuildingThatAnotherNeedsCannotBeRemoved()
        {
            var castle = Castle("Bulwark", "bulwark_13_mage_guild_4", "Still Water");

            Assert.That(TestServices.CastleBuildMessages.DescribeUnbuild(castle, Building.Fort), Is.EqualTo("Needed by Citadel"));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, Building.Tavern), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.That(TestServices.CastleBuildMessages.DescribeUnbuild(castle, Building.VillageHall), Is.EqualTo("Every town has a village hall"));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, Building.Marketplace), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(castle, Building.MagesGuild4), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.That(TestServices.CastleBuildMessages.DescribeUnbuild(castle, Building.Capitol), Is.EqualTo("Not built"));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Unbuild(castle, Building.Fort));
        }

        [Test]
        public void BuildingsCanBeBuiltAndRemovedInAnyOrderExceptUpgradesOverTheirBase()
        {
            var rampart = Castle("Rampart", "rampart_00_baseline", "Fortune Keep");
            var village = new HashSet<Building> { Building.VillageHall };
            var castle = rampart with { Faction = Faction.Castle, Buildings = village, CurrentBuildings = village, EnabledBuildings = Building.ForFaction(Faction.Castle).ToHashSet() };

            // The lighthouse works without a shipyard, and the other way round.
            Assert.That(CastleBuilder.GetBuildFailureReason(castle, Building.CastleLighthouse), Is.EqualTo(CastleBuildFailureReason.None));
            var built = CastleBuilder.Build(castle, Building.CastleLighthouse);
            Assert.That(built.HasBuilt(Building.CastleLighthouse), Is.True);
            Assert.That(CastleBuilder.UnmetRequirements(built, Building.CastleLighthouse), Is.EqualTo(new[] { Building.Shipyard }));
            var both = CastleBuilder.Build(built, Building.Shipyard);
            Assert.That(CastleBuilder.GetUnbuildFailureReason(both, Building.Shipyard), Is.EqualTo(CastleBuildFailureReason.None));
            Assert.That(CastleBuilder.Unbuild(both, Building.Shipyard).HasBuilt(Building.CastleLighthouse), Is.True);

            // A dwelling does not need the fort.
            Assert.That(CastleBuilder.GetBuildFailureReason(castle, Building.CastleGuardhouse), Is.EqualTo(CastleBuildFailureReason.None));

            // An upgraded dwelling needs its dwelling.
            var rampartVillage = rampart with { Buildings = village, CurrentBuildings = village };
            Assert.That(TestServices.CastleBuildMessages.DescribeBuild(rampartVillage, Building.RampartUpgDwarfCottage), Is.EqualTo("Requires Dwarf Cottage"));
            Assert.Throws<GameRuleException>(() => CastleBuilder.Build(rampartVillage, Building.RampartUpgDwarfCottage));

            // The dwelling stays while its upgrade stands.
            var withBoth = rampartVillage with { Buildings = new HashSet<Building> { Building.VillageHall, Building.RampartDwarfCottage, Building.RampartUpgDwarfCottage } };
            Assert.That(TestServices.CastleBuildMessages.DescribeUnbuild(withBoth, Building.RampartDwarfCottage), Is.EqualTo("Needed by Upg. Dwarf Cottage"));
            Assert.That(CastleBuilder.GetUnbuildFailureReason(withBoth, Building.RampartUpgDwarfCottage), Is.EqualTo(CastleBuildFailureReason.None));
        }

        [Test]
        public void TheUpgradedHordeBuildingComesWithTheUpgradeOfItsDwellingInEitherOrder()
        {
            var castle = Castle("Rampart", "rampart_00_baseline", "Fortune Keep");

            var hordeFirst = CastleBuilder.Build(CastleBuilder.Build(castle, Building.RampartMinersGuild), Building.RampartUpgDwarfCottage);
            Assert.That(hordeFirst.Buildings, Does.Contain(Building.RampartUpgMinersGuild));
            Assert.That(hordeFirst.CurrentBuildings, Does.Contain(Building.RampartUpgMinersGuild).And.Not.Contain(Building.RampartMinersGuild).And.Not.Contain(Building.RampartUpgDwarfCottage));

            var upgradeFirst = CastleBuilder.SetBuilt(CastleBuilder.SetBuilt(castle, Building.RampartUpgDwarfCottage, true), Building.RampartMinersGuild, true);
            Assert.That(upgradeFirst.CurrentBuildings, Is.EquivalentTo(hordeFirst.CurrentBuildings));

            var removed = CastleBuilder.Unbuild(hordeFirst, Building.RampartMinersGuild);
            Assert.That(removed.Buildings, Does.Not.Contain(Building.RampartUpgMinersGuild));
            Assert.That(removed.CurrentBuildings, Does.Contain(Building.RampartUpgDwarfCottage));
        }
    }
}
