namespace Siniskalc.Homm2.Tests.Entities
{
    using System.Linq;
    using Siniskalc.Homm2.Entities;

    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CreatureTests
    {
        [Test]
        public void EveryCreatureHasStatsAndACost()
        {
            foreach (var creature in TestCatalog.Standard.Creatures)
            {
                var info = creature.Info;

                Assert.That(info.Atk, Is.GreaterThan(0), creature.Name);
                Assert.That(info.Def, Is.GreaterThan(0), creature.Name);
                Assert.That(info.MinDamage, Is.InRange(1, (int)info.MaxDamage), creature.Name);
                Assert.That(info.Health, Is.GreaterThan(0), creature.Name);
                Assert.That(info.Speed, Is.InRange(1, 7), creature.Name);
                Assert.That(info.Growth, Is.GreaterThan(0), creature.Name);
                Assert.That(info.Gold, Is.GreaterThan(0), creature.Name);
            }
        }

        [Test]
        public void ACreatureIsRangedExactlyWhenItHasShots()
        {
            foreach (var creature in TestCatalog.Standard.Creatures)
            {
                var ranged = creature.Info.Special?.Contains(TestCatalog.Standard.Abilities["Ranged"]) ?? false;

                Assert.That(ranged, Is.EqualTo(creature.Info.Shots > 0), creature.Name);
            }
        }

        [Test]
        public void TheGameKnowsNoAiOrFightValues()
        {
            Assert.That(TestCatalog.Standard.Creatures.Select(c => c.Info.AiValue + c.Info.FightValue), Is.All.Zero);
        }

        [Test]
        public void TheCostBesidesGoldIsOneOrTwoOfOneResource()
        {
            Assert.That(TestCatalog.Standard.Creatures["Cyclops"].Info.Crystal, Is.EqualTo(1));
            Assert.That(TestCatalog.Standard.Creatures["Phoenix"].Info.Mercury, Is.EqualTo(1));
            Assert.That(TestCatalog.Standard.Creatures["BlackDragon"].Info.Sulfur, Is.EqualTo(2));
            Assert.That(TestCatalog.Standard.Creatures["Titan"].Info.Gems, Is.EqualTo(2));
            Assert.That(TestCatalog.Standard.Creatures["Pikeman"].Info.Wood + TestCatalog.Standard.Creatures["Pikeman"].Info.Ore, Is.Zero);
        }

        [Test]
        public void AnUpgradeIsRecruitedInTheUpgradedDwellingOfItsBaseCreature()
        {
            foreach (var creature in TestCatalog.Standard.Creatures.Where(c => c.BaseCreatureCode is not null))
            {
                var baseCreature = TestCatalog.Standard.Creatures.Find(creature.BaseCreatureCode!.Value)!;

                Assert.That(baseCreature.Faction, Is.EqualTo(creature.Faction), creature.Name);
                Assert.That(baseCreature.Tier, Is.EqualTo(creature.Tier), creature.Name);
                Assert.That(creature.Dwelling, Is.Not.EqualTo(baseCreature.Dwelling), creature.Name);
            }

            Assert.That(TestCatalog.Standard.Creatures["Archer"].Dwelling, Is.EqualTo(Dwellings.Dwelling2));
            Assert.That(TestCatalog.Standard.Creatures["Ranger"].Dwelling, Is.EqualTo(Dwellings.Dwelling2Upgraded));
            Assert.That(TestCatalog.Standard.Creatures["BlackDragon"].Dwelling, Is.EqualTo(Dwellings.Dwelling6UpgradedTwice));
        }

        [Test]
        public void ANeutralCreatureHasNoDwellingAndEveryOtherOneHasOne()
        {
            Assert.That(TestCatalog.Standard.Creatures.Where(c => c.Faction is null).Select(c => c.Dwelling), Is.All.Null);
            Assert.That(TestCatalog.Standard.Creatures.Where(c => c.Faction is not null).Select(c => c.Dwelling), Is.All.Not.Null);
        }
    }
}
