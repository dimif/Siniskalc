namespace Siniskalc.Homm2.Tests.Entities
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The tables of entities, against the numbers the savegames proved and the shape of the game (66 creatures, 65 spells, 14 skills).
    /// </summary>
    [TestFixture]
    internal sealed class EntityTests
    {
        [Test]
        public void TheGameHasSixtySixCreaturesNumberedFromZeroWithoutGaps()
        {
            Assert.That(Creature.All, Has.Count.EqualTo(66));
            Assert.That(Creature.All.Select(c => (int)c.Code), Is.EqualTo(Enumerable.Range(0, 66)));
            Assert.That(Creature.All.Select(c => c.Name), Is.Unique);
        }

        [TestCase((byte)0, "Peasant", 12)]
        [TestCase((byte)1, "Archer", 8)]
        [TestCase((byte)37, "Black Dragon", 1)]
        [TestCase((byte)38, "Halfling", 8)]
        [TestCase((byte)65, "Water Elemental", 4)]
        public void CreaturesHaveTheNumbersAndTheWeeklyGrowthTheSavegamesShowed(byte code, string name, int growth)
        {
            var creature = Creature.FromByte(code)!;

            Assert.That(creature.Name, Is.EqualTo(name));
            Assert.That(creature.Info.Growth, Is.EqualTo(growth));
        }

        [Test]
        public void EveryRaceHasNineCreaturesInSixLevelsAndTheNeutralOnesHaveNoRace()
        {
            foreach (var faction in Faction.All)
            {
                var creatures = Creature.ForFaction(faction);

                Assert.That(creatures.Select(c => c.Tier).Distinct().Order(), Is.EqualTo(Enumerable.Range(1, 6)), faction.ToString());
                Assert.That(creatures, Has.Count.InRange(9, 12), faction.ToString());
            }

            Assert.That(Creature.All.Where(c => c.Faction is null).Select(c => c.Name), Is.EqualTo(new[] { "Rogue", "Nomad", "Ghost", "Genie", "Medusa", "Earth Elemental", "Air Elemental", "Fire Elemental", "Water Elemental" }));
        }

        [Test]
        public void TheGameHasSixtyFiveSpellsNumberedFromOne()
        {
            Assert.That(Spell.All, Has.Count.EqualTo(65));
            Assert.That(Spell.All.Select(s => (int)s.Code), Is.EqualTo(Enumerable.Range(1, 65)));
            Assert.That(Spell.All.Select(s => s.Name), Is.Unique);
        }

        [TestCase((byte)6, "Cure", 6)]
        [TestCase((byte)18, "Steelskin", 6)]
        [TestCase((byte)26, "Magic Arrow", 3)]
        [TestCase((byte)35, "Disrupting Ray", 7)]
        [TestCase((byte)50, "View Resources", 1)]
        [TestCase((byte)51, "View Artifacts", 2)]
        public void SpellsHaveTheNumbersTheSpellBooksOfTheSavegamesUse(byte code, string name, int cost)
        {
            var spell = Spell.FromByte(code)!;

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
            Assert.That(Artifact.FromByte(code)!.Name, Is.EqualTo(name));
        }

        [Test]
        public void TheArtifactsOfThePriceOfLoyaltyStartAtTheSpellScrollAndTheOthersAreInTheBaseGame()
        {
            Assert.That(Artifact.All.Where(a => a.GameVersion == GameVersion.PriceOfLoyalty).Select(a => (int)a.Code), Is.EqualTo(Enumerable.Range(86, 17)));
            Assert.That(Artifact.All.Where(a => a.GameVersion == GameVersion.BaseGame).Select(a => (int)a.Code), Is.EqualTo(Enumerable.Range(0, 82)));
            Assert.That(Artifact.All.Select(a => a.Name), Is.Unique);
            Assert.That(Artifact.FromByte(0xFF), Is.Null);
            Assert.That(Artifact.FromByte(83), Is.Null);
            Assert.That(Artifact.NameOf(83), Is.EqualTo("Unknown artifact 83"));
        }

        [Test]
        public void TheFourteenSecondarySkillsAreInTheOrderOfTheTablesOfAHero()
        {
            Assert.That(SecondarySkill.All.Select(s => s.Name), Is.EqualTo(new[]
            {
                "Pathfinding", "Archery", "Logistics", "Scouting", "Diplomacy", "Navigation", "Leadership",
                "Wisdom", "Mysticism", "Luck", "Ballistics", "Eagle Eye", "Necromancy", "Estates",
            }));
            Assert.That(SecondarySkill.Leadership.Code, Is.EqualTo(6));
            Assert.That(SecondarySkill.Ballistics.Code, Is.EqualTo(10));
        }
    }
}
