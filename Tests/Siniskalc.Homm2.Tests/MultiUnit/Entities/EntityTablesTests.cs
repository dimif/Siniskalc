namespace Siniskalc.Homm2.Tests.MultiUnit.Entities
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The tables of entities, against the numbers the savegames proved and the shape of the game (66 creatures, 65 spells, 14 skills).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class EntityTablesTests
    {
        [Test]
        public void TheGameHasSixtySixCreaturesNumberedFromZeroWithoutGaps()
        {
            Assert.That(TestCatalog.Standard.Creatures, Has.Count.EqualTo(66));
            Assert.That(TestCatalog.Standard.Creatures.Select(c => (int)c.Code), Is.EqualTo(Enumerable.Range(0, 66)));
            Assert.That(TestCatalog.Standard.Creatures.Select(c => c.Name), Is.Unique);
        }

        [TestCase((byte)0, "Peasant", 12)]
        [TestCase((byte)1, "Archer", 8)]
        [TestCase((byte)37, "Black Dragon", 1)]
        [TestCase((byte)38, "Halfling", 8)]
        [TestCase((byte)65, "Water Elemental", 4)]
        public void CreaturesHaveTheNumbersAndTheWeeklyGrowthTheSavegamesShowed(byte code, string name, int growth)
        {
            var creature = TestCatalog.Standard.Creatures.Find(code)!;

            Assert.That(creature.Name, Is.EqualTo(name));
            Assert.That(creature.Info.Growth, Is.EqualTo(growth));
        }

        [Test]
        public void EveryRaceHasNineCreaturesInSixLevelsAndTheNeutralOnesHaveNoRace()
        {
            foreach (var faction in TestCatalog.Standard.Factions.Where(f => !f.IsNeutral))
            {
                var creatures = faction.Creatures;

                Assert.That(creatures.Select(c => c.Tier).Distinct().Order(), Is.EqualTo(Enumerable.Range(1, 6)), faction.ToString());
                Assert.That(creatures, Has.Count.InRange(9, 12), faction.ToString());
            }

            Assert.That(TestCatalog.Standard.Creatures.Where(c => c.Faction is null).Select(c => c.Name), Is.EqualTo(new[] { "Rogue", "Nomad", "Ghost", "Genie", "Medusa", "Earth Elemental", "Air Elemental", "Fire Elemental", "Water Elemental" }));
        }

        [Test]
        public void TheGameHasSixtyFiveSpellsNumberedFromOne()
        {
            Assert.That(TestCatalog.Standard.Spells, Has.Count.EqualTo(65));
            Assert.That(TestCatalog.Standard.Spells.Select(s => (int)s.Code), Is.EqualTo(Enumerable.Range(1, 65)));
            Assert.That(TestCatalog.Standard.Spells.Select(s => s.Name), Is.Unique);
        }

        [TestCase((byte)6, "Cure", 6)]
        [TestCase((byte)18, "Steelskin", 6)]
        [TestCase((byte)26, "Magic Arrow", 3)]
        [TestCase((byte)35, "Disrupting Ray", 7)]
        [TestCase((byte)50, "View Resources", 1)]
        [TestCase((byte)51, "View Artifacts", 2)]
        public void SpellsHaveTheNumbersTheSpellBooksOfTheSavegamesUse(byte code, string name, int cost)
        {
            var spell = TestCatalog.Standard.Spells.Find(code)!;

            Assert.That(spell.Name, Is.EqualTo(name));
            Assert.That(spell.SpellPoints, Is.EqualTo(cost));
        }

        [TestCase((byte)13, "Medal of Courage")]
        [TestCase((byte)45, "Ice Cloak")]
        [TestCase((byte)68, "Ammo Cart")]
        [TestCase((byte)81, "Magic Book")]
        [TestCase((byte)86, "Spell Scroll")]
        public void ArtifactsHaveTheNumbersTheSlotsOfTheSavegamesUse(byte code, string name)
        {
            Assert.That(TestCatalog.Standard.Artifacts.Find(code)!.Name, Is.EqualTo(name));
        }

        [Test]
        public void TheArtifactsOfThePriceOfLoyaltyStartAtTheSpellScrollAndTheOthersAreInTheBaseGame()
        {
            Assert.That(TestCatalog.Standard.Artifacts.Where(a => a.GameVersion == GameVersion.PriceOfLoyalty).Select(a => (int)a.Code), Is.EqualTo(Enumerable.Range(86, 17)));
            Assert.That(TestCatalog.Standard.Artifacts.Where(a => a.GameVersion == GameVersion.BaseGame).Select(a => (int)a.Code), Is.EqualTo(Enumerable.Range(0, 82)));
            Assert.That(TestCatalog.Standard.Artifacts.Select(a => a.Name), Is.Unique);
            Assert.That(TestCatalog.Standard.Artifacts.Find(0xFF), Is.Null);
            Assert.That(TestCatalog.Standard.Artifacts.Find(83), Is.Null);
            Assert.That(TestCatalog.Standard.Artifacts.Find(83)?.Name ?? "Unknown artifact 83", Is.EqualTo("Unknown artifact 83"));
        }

        [Test]
        public void TheFourteenSecondarySkillsAreInTheOrderOfTheTablesOfAHero()
        {
            Assert.That(TestCatalog.Standard.Skills.Select(s => s.Name), Is.EqualTo(new[]
            {
                "Pathfinding", "Archery", "Logistics", "Scouting", "Diplomacy", "Navigation", "Leadership",
                "Wisdom", "Mysticism", "Luck", "Ballistics", "Eagle Eye", "Necromancy", "Estates",
            }));
            Assert.That(TestCatalog.Standard.Skills["Leadership"].Code, Is.EqualTo(6));
            Assert.That(TestCatalog.Standard.Skills["Ballistics"].Code, Is.EqualTo(10));
        }
    }
}
