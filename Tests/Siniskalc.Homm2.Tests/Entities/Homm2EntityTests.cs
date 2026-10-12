namespace Siniskalc.Homm2.Tests.Entities
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The tables of entities, against the numbers the savegames proved and the shape of the game (66 creatures, 65 spells, 14 skills).
    /// </summary>
    [TestFixture]
    internal sealed class Homm2EntityTests
    {
        [Test]
        public void TheGameHasSixtySixCreaturesNumberedFromZeroWithoutGaps()
        {
            Assert.That(Homm2Creature.All, Has.Count.EqualTo(66));
            Assert.That(Homm2Creature.All.Select(c => (int)c.Code), Is.EqualTo(Enumerable.Range(0, 66)));
            Assert.That(Homm2Creature.All.Select(c => c.Name), Is.Unique);
        }

        [TestCase((byte)0, "Peasant", 12)]
        [TestCase((byte)1, "Archer", 8)]
        [TestCase((byte)37, "Black Dragon", 1)]
        [TestCase((byte)38, "Halfling", 8)]
        [TestCase((byte)65, "Water Elemental", 4)]
        public void CreaturesHaveTheNumbersAndTheWeeklyGrowthTheSavegamesShowed(byte code, string name, int growth)
        {
            var creature = Homm2Creature.FromByte(code)!;

            Assert.That(creature.Name, Is.EqualTo(name));
            Assert.That(creature.Growth, Is.EqualTo(growth));
        }

        [Test]
        public void EveryRaceHasNineCreaturesInSixLevelsAndTheNeutralOnesHaveNoRace()
        {
            foreach (var race in Enum.GetValues<Homm2Race>())
            {
                var creatures = Homm2Creature.ForRace(race);

                Assert.That(creatures.Select(c => c.Level).Distinct().Order(), Is.EqualTo(Enumerable.Range(1, 6)), race.ToString());
                Assert.That(creatures, Has.Count.InRange(9, 12), race.ToString());
            }

            Assert.That(Homm2Creature.All.Where(c => c.Race is null).Select(c => c.Name), Is.EqualTo(new[] { "Rogue", "Nomad", "Ghost", "Genie", "Medusa", "Earth Elemental", "Air Elemental", "Fire Elemental", "Water Elemental" }));
        }

        [Test]
        public void TheGameHasSixtyFiveSpellsNumberedFromOne()
        {
            Assert.That(Homm2Spell.All, Has.Count.EqualTo(65));
            Assert.That(Homm2Spell.All.Select(s => (int)s.Code), Is.EqualTo(Enumerable.Range(1, 65)));
            Assert.That(Homm2Spell.All.Select(s => s.Name), Is.Unique);
        }

        [TestCase((byte)6, "Cure", 6)]
        [TestCase((byte)18, "Steelskin", 6)]
        [TestCase((byte)26, "Magic Arrow", 3)]
        [TestCase((byte)35, "Disrupting Ray", 7)]
        [TestCase((byte)50, "View Resources", 1)]
        [TestCase((byte)51, "View Artifacts", 2)]
        public void SpellsHaveTheNumbersTheSpellBooksOfTheSavegamesUse(byte code, string name, int cost)
        {
            var spell = Homm2Spell.FromByte(code)!;

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
            Assert.That(Homm2Artifact.FromByte(code)!.Name, Is.EqualTo(name));
        }

        [Test]
        public void TheArtifactsOfThePriceOfLoyaltyStartAtTheSpellScrollAndTheOthersAreInTheBaseGame()
        {
            Assert.That(Homm2Artifact.All.Where(a => a.GameVersion == Homm2GameVersion.PriceOfLoyalty).Select(a => (int)a.Code), Is.EqualTo(Enumerable.Range(86, 17)));
            Assert.That(Homm2Artifact.All.Where(a => a.GameVersion == Homm2GameVersion.BaseGame).Select(a => (int)a.Code), Is.EqualTo(Enumerable.Range(0, 82)));
            Assert.That(Homm2Artifact.All.Select(a => a.Name), Is.Unique);
            Assert.That(Homm2Artifact.FromByte(0xFF), Is.Null);
            Assert.That(Homm2Artifact.FromByte(83), Is.Null);
            Assert.That(Homm2Artifact.NameOf(83), Is.EqualTo("Unknown artifact 83"));
        }

        [Test]
        public void TheFourteenSecondarySkillsAreInTheOrderOfTheTablesOfAHero()
        {
            Assert.That(Homm2SecondarySkill.All.Select(s => s.Name), Is.EqualTo(new[]
            {
                "Pathfinding", "Archery", "Logistics", "Scouting", "Diplomacy", "Navigation", "Leadership",
                "Wisdom", "Mysticism", "Luck", "Ballistics", "Eagle Eye", "Necromancy", "Estates",
            }));
            Assert.That(Homm2SecondarySkill.Leadership.Code, Is.EqualTo(6));
            Assert.That(Homm2SecondarySkill.Ballistics.Code, Is.EqualTo(10));
        }
    }
}
