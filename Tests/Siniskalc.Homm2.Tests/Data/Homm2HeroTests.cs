namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// What the heroes of Broken Alliance hold, as written in the README of the saves (TestData/Homm2/BrokenAlliance).
    /// </summary>
    [TestFixture]
    internal sealed class Homm2HeroTests
    {
        private const int Maximus = 7;
        private const int SirGallant = 1;
        private const int Natasha = 19;

        private static Homm2HeroModel Hero(int save, int hero) => Homm2TestServices.LoadBrokenAlliance(save).History.Current.Heroes().GetModel(hero);

        [Test]
        public void MaximusStartsAsALevelOneKnightWithTwoSkillsAnArmyAndNoSpellBook()
        {
            var maximus = Hero(1, Maximus);

            Assert.That(maximus.Name, Is.EqualTo("Maximus"));
            Assert.That(maximus.HeroId, Is.EqualTo(Maximus));
            Assert.That(maximus.Race, Is.EqualTo(Homm2Race.Knight));
            Assert.That(maximus.Owner, Is.EqualTo(Homm2Color.Blue));
            Assert.That(maximus.Position, Is.EqualTo(new Homm2Position(28, 27)));
            Assert.That(maximus.Destination, Is.Null);
            Assert.That(maximus.Level, Is.EqualTo(1));
            Assert.That(maximus.Experience, Is.Zero);
            Assert.That(maximus.PrimarySkills, Is.EqualTo(new Homm2PrimarySkills(2, 2, 1, 1)));
            Assert.That(maximus.SecondarySkills.Entries, Is.EqualTo(new[]
            {
                new Homm2SkillEntry(Homm2SecondarySkill.Leadership, Homm2SkillLevel.Basic),
                new Homm2SkillEntry(Homm2SecondarySkill.Ballistics, Homm2SkillLevel.Basic),
            }));
            Assert.That(maximus.Army.Slots, Is.EqualTo(new[] { new Homm2ArmySlot(Homm2Creature.Peasant, 37), new Homm2ArmySlot(Homm2Creature.Archer, 4), null, null, null }));
            Assert.That(maximus.Artifacts, Is.EqualTo(Homm2Artifacts.None));
            Assert.That(maximus.HasSpellBook, Is.False);
            Assert.That(maximus.Spells.Count, Is.Zero);
            Assert.That((maximus.MaxMovement, maximus.MovementLeft), Is.EqualTo((1000L, 1000L)));
            Assert.That(maximus.SpellPoints, Is.EqualTo(10));
        }

        [Test]
        public void SplittingTheArmyGivesThreeStacksOfOnePeasant()
        {
            var army = Hero(2, Maximus).Army;

            Assert.That(army.Slots.Select(s => s?.Count), Is.EqualTo(new int?[] { 34, 4, 1, 1, 1 }));
            Assert.That(army.Slots.Select(s => s?.Creature), Is.EqualTo(new[] { Homm2Creature.Peasant, Homm2Creature.Archer, Homm2Creature.Peasant, Homm2Creature.Peasant, Homm2Creature.Peasant }));
        }

        [Test]
        public void MovingOneSquareChangesThePositionAndTheMovementLeft()
        {
            var moved = Hero(3, Maximus);

            Assert.That(moved.Position, Is.EqualTo(new Homm2Position(29, 28)));
            Assert.That(moved.MovementLeft, Is.EqualTo(888));
        }

        [Test]
        public void RunningOutOfMovementLeavesTheDestinationOfThePathThatWasNotFinished()
        {
            var tired = Hero(5, Maximus);

            Assert.That(tired.MovementLeft, Is.Zero);
            Assert.That(tired.Destination, Is.EqualTo(new Homm2Position(23, 23)));
        }

        [Test]
        public void BuyingTheSpellBookGivesTheMagicBookAndTheSixSpellsOfTheMageGuild()
        {
            var maximus = Hero(10, Maximus);

            Assert.That(maximus.HasSpellBook, Is.True);
            Assert.That(maximus.Artifacts.Slots[0], Is.EqualTo(Homm2Artifact.MagicBook));
            Assert.That(maximus.Spells.Spells, Is.EqualTo(new[]
            {
                Homm2Spell.Cure,
                Homm2Spell.Steelskin,
                Homm2Spell.MagicArrow,
                Homm2Spell.DisruptingRay,
                Homm2Spell.ViewResources,
                Homm2Spell.ViewArtifacts,
            }));
        }

        [Test]
        public void HiringSirGallantPutsHimInTheCastleForBlue()
        {
            var hired = Hero(8, SirGallant);
            var before = Hero(7, SirGallant);

            Assert.That(before.Owner, Is.Null);
            Assert.That(hired.Owner, Is.EqualTo(Homm2Color.Blue));
            Assert.That(hired.Position, Is.EqualTo(new Homm2Position(28, 25)));
        }

        [Test]
        public void ALevelUpRaisesTheLevelTheDefenseAndLearnsASkill()
        {
            var before = Hero(8, SirGallant);
            var after = Hero(9, SirGallant);

            Assert.That(before.Level, Is.EqualTo(1));
            Assert.That(after.Level, Is.EqualTo(2));
            Assert.That(after.Experience, Is.EqualTo(1550));
            Assert.That(after.PrimarySkills.Defense, Is.EqualTo(before.PrimarySkills.Defense + 1));
            Assert.That(after.SecondarySkills.Count, Is.EqualTo(before.SecondarySkills.Count + 1));
            Assert.That(after.SecondarySkills.Entries[^1], Is.EqualTo(new Homm2SkillEntry(Homm2SecondarySkill.Pathfinding, Homm2SkillLevel.Basic)));
        }

        [Test]
        public void ThePickedUpArtifactsAreInTheFirstFreeSlots()
        {
            Assert.That(Hero(11, SirGallant).Artifacts.Slots[0], Is.EqualTo(Homm2Artifact.MedalOfCourage));
            Assert.That(Hero(15, Maximus).Artifacts.Slots.Take(2), Is.EqualTo(new[] { Homm2Artifact.MagicBook, Homm2Artifact.AmmoCart }));
            Assert.That(Hero(16, SirGallant).Artifacts.Slots.Take(2), Is.EqualTo(new[] { Homm2Artifact.MedalOfCourage, Homm2Artifact.IceCloak }));
        }

        [Test]
        public void TheFountainTurnedTheArmyIntoFiveBlackDragons()
        {
            Assert.That(Hero(13, Maximus).Army.Slots, Is.EqualTo(new[] { new Homm2ArmySlot(Homm2Creature.BlackDragon, 5), null, null, null, null }));
        }

        [Test]
        public void CastingMagicArrowCostsThreeSpellPointsAndADayGivesOneBack()
        {
            Assert.That(Hero(16, Maximus).SpellPoints, Is.EqualTo(12));
            Assert.That(Hero(17, Maximus).SpellPoints, Is.EqualTo(10));
        }

        [Test]
        public void ADefeatedHeroServesNobodyAndLosesHerArtifact()
        {
            var before = Hero(15, Natasha);
            var after = Hero(16, Natasha);

            Assert.That(before.Owner, Is.EqualTo(Homm2Color.Red));
            Assert.That(before.Artifacts.Slots.Take(2), Is.EqualTo(new[] { Homm2Artifact.MagicBook, Homm2Artifact.IceCloak }));
            Assert.That(after.Owner, Is.Null);
            Assert.That(after.Artifacts.Slots.Take(2), Is.EqualTo(new Homm2Artifact?[] { Homm2Artifact.MagicBook, null }));
            Assert.That(after.SpellPoints, Is.EqualTo(27));
        }

        [Test]
        public void EveryHeroHasItsNumberAndTheNinePerRaceAreInTheOrderOfTheRaces()
        {
            var heroes = Homm2TestServices.LoadBrokenAlliance(1).History.Current.Heroes().GetModels();

            Assert.That(heroes, Has.Count.EqualTo(54));
            Assert.That(heroes.Select(h => h.HeroId), Is.EqualTo(Enumerable.Range(0, 54)));
            Assert.That(heroes[0].Name, Is.EqualTo("Lord Kilburn"));
            Assert.That(heroes[51].Name, Is.EqualTo("Roxana"));
            Assert.That(heroes.Select(h => h.Race).Distinct(), Is.EqualTo(Enum.GetValues<Homm2Race>()));
            Assert.That(heroes.GroupBy(h => h.Race).Select(g => g.Count()), Is.All.EqualTo(9));
            Assert.That(heroes.Where(h => h.Owner == Homm2Color.Blue).Select(h => h.Name), Is.EqualTo(new[] { "Maximus" }));
        }

        [Test]
        public void HeroesAreFoundByNameWhateverTheCase()
        {
            var heroes = Homm2TestServices.LoadBrokenAlliance(1).History.Current.Heroes();

            Assert.That(heroes.FindByName("sir gallant"), Is.EqualTo(SirGallant));
            Assert.That(heroes.FindByName("nobody"), Is.EqualTo(-1));
        }
    }
}
