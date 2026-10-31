namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the heroes of Broken Alliance hold, as written in the README of the saves (TestData/Homm2/BrokenAlliance).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroTests
    {
        private const int Maximus = 7;
        private const int SirGallant = 1;
        private const int Natasha = 19;

        private static HeroModel Hero(int save, int hero) => TestServices.LoadBrokenAlliance(save).History.Current.Heroes().GetModel(hero);

        [Test]
        public void MaximusStartsAsALevelOneKnightWithTwoSkillsAnArmyAndNoSpellBook()
        {
            var maximus = Hero(1, Maximus);

            Assert.That(maximus.Name, Is.EqualTo("Maximus"));
            Assert.That(maximus.PortraitId, Is.EqualTo(Maximus));
            Assert.That(maximus.Faction, Is.EqualTo(Faction.Knight));
            Assert.That(maximus.Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That((maximus.Position.X, maximus.Position.Y), Is.EqualTo((28, 27)));
            Assert.That(maximus.Destination, Is.EqualTo(MapPosition.None));
            Assert.That(maximus.HeroLevel.Level, Is.EqualTo(1));
            Assert.That(maximus.HeroLevel.Experience, Is.Zero);
            Assert.That(maximus.PrimarySkills, Is.EqualTo(new PrimarySkills(2, 2, 1, 1)));
            Assert.That(maximus.SecondarySkills.Entries, Is.EqualTo(new[]
            {
                new SkillEntry(SecondarySkill.Leadership, SkillLevel.Beginner),
                new SkillEntry(SecondarySkill.Ballistics, SkillLevel.Beginner),
            }));
            Assert.That(maximus.Army.Slots, Is.EqualTo(new[] { new ArmySlot<Creature>(Creature.Peasant, 37), new ArmySlot<Creature>(Creature.Archer, 4), null, null, null }));
            Assert.That(maximus.Artifacts, Is.EqualTo(Artifacts.None));
            Assert.That(maximus.HasSpellBook, Is.False);
            Assert.That(maximus.Spells.Count, Is.Zero);
            Assert.That((maximus.MovementPoints.Max, maximus.MovementPoints.Current), Is.EqualTo((1000L, 1000L)));
            Assert.That(maximus.SpellPoints, Is.EqualTo(10));
        }

        [Test]
        public void SplittingTheArmyGivesThreeStacksOfOnePeasant()
        {
            var army = Hero(2, Maximus).Army;

            Assert.That(army.Slots.Select(s => s?.Count), Is.EqualTo(new int?[] { 34, 4, 1, 1, 1 }));
            Assert.That(army.Slots.Select(s => s?.Creature), Is.EqualTo(new[] { Creature.Peasant, Creature.Archer, Creature.Peasant, Creature.Peasant, Creature.Peasant }));
        }

        [Test]
        public void MovingOneSquareChangesThePositionAndTheMovementLeft()
        {
            var moved = Hero(3, Maximus);

            Assert.That((moved.Position.X, moved.Position.Y), Is.EqualTo((29, 28)));
            Assert.That(moved.MovementPoints.Current, Is.EqualTo(888));
        }

        [Test]
        public void RunningOutOfMovementLeavesTheDestinationOfThePathThatWasNotFinished()
        {
            var tired = Hero(5, Maximus);

            Assert.That(tired.MovementPoints.Current, Is.Zero);
            Assert.That(tired.Destination, Is.EqualTo(new MapPosition(23, 23)));
        }

        [Test]
        public void BuyingTheSpellBookGivesTheMagicBookAndTheSixSpellsOfTheMageGuild()
        {
            var maximus = Hero(10, Maximus);

            Assert.That(maximus.HasSpellBook, Is.True);
            Assert.That(maximus.Artifacts.Slots[0], Is.EqualTo(Artifact.MagicBook));
            Assert.That(maximus.Spells, Is.EqualTo(new[]
            {
                Spell.Cure,
                Spell.Steelskin,
                Spell.MagicArrow,
                Spell.DisruptingRay,
                Spell.ViewResources,
                Spell.ViewArtifacts,
            }));
        }

        [Test]
        public void HiringSirGallantPutsHimInTheCastleForBlue()
        {
            var hired = Hero(8, SirGallant);
            var before = Hero(7, SirGallant);

            Assert.That(before.Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(hired.Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That((hired.Position.X, hired.Position.Y), Is.EqualTo((28, 25)));
        }

        [Test]
        public void ALevelUpRaisesTheLevelTheDefenseAndLearnsASkill()
        {
            var before = Hero(8, SirGallant);
            var after = Hero(9, SirGallant);

            Assert.That(before.HeroLevel.Level, Is.EqualTo(1));
            Assert.That(after.HeroLevel.Level, Is.EqualTo(2));
            Assert.That(after.HeroLevel.Experience, Is.EqualTo(1550));
            Assert.That(after.PrimarySkills.Defense, Is.EqualTo(before.PrimarySkills.Defense + 1));
            Assert.That(after.SecondarySkills.SlotCount, Is.EqualTo(before.SecondarySkills.SlotCount + 1));
            Assert.That(after.SecondarySkills.Entries[^1], Is.EqualTo(new SkillEntry(SecondarySkill.Pathfinding, SkillLevel.Beginner)));
        }

        [Test]
        public void ThePickedUpArtifactsAreInTheFirstFreeSlots()
        {
            Assert.That(Hero(11, SirGallant).Artifacts.Slots[0], Is.EqualTo(Artifact.MedalOfCourage));
            Assert.That(Hero(15, Maximus).Artifacts.Slots.Take(2), Is.EqualTo(new[] { Artifact.MagicBook, Artifact.AmmoCart }));
            Assert.That(Hero(16, SirGallant).Artifacts.Slots.Take(2), Is.EqualTo(new[] { Artifact.MedalOfCourage, Artifact.IceCloak }));
        }

        [Test]
        public void TheFountainTurnedTheArmyIntoFiveBlackDragons()
        {
            Assert.That(Hero(13, Maximus).Army.Slots, Is.EqualTo(new[] { new ArmySlot<Creature>(Creature.BlackDragon, 5), null, null, null, null }));
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

            Assert.That(before.Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(before.Artifacts.Slots.Take(2), Is.EqualTo(new[] { Artifact.MagicBook, Artifact.IceCloak }));
            Assert.That(after.Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(after.Artifacts.Slots.Take(2), Is.EqualTo(new Artifact?[] { Artifact.MagicBook, null }));
            Assert.That(after.SpellPoints, Is.EqualTo(27));
        }

        [Test]
        public void EveryHeroHasItsNumberAndTheNinePerRaceAreInTheOrderOfTheRaces()
        {
            var heroes = TestServices.LoadBrokenAlliance(1).History.Current.Heroes().GetModels();

            Assert.That(heroes, Has.Count.EqualTo(54));
            Assert.That(heroes.Select(h => h.PortraitId), Is.EqualTo(Enumerable.Range(0, 54)));
            Assert.That(heroes[0].Name, Is.EqualTo("Lord Kilburn"));
            Assert.That(heroes[51].Name, Is.EqualTo("Roxana"));
            Assert.That(heroes.Select(h => h.Faction).Distinct(), Is.EqualTo(Faction.All));
            Assert.That(heroes.GroupBy(h => h.Faction).Select(g => g.Count()), Is.All.EqualTo(9));
            Assert.That(heroes.Where(h => h.Owner == PlayerColor.Blue).Select(h => h.Name), Is.EqualTo(new[] { "Maximus" }));
        }

        [Test]
        public void HeroesAreFoundByNameWhateverTheCase()
        {
            var heroes = TestServices.LoadBrokenAlliance(1).History.Current.Heroes();

            Assert.That(heroes.FindByName("sir gallant"), Is.EqualTo(SirGallant));
            Assert.That(heroes.FindByName("nobody"), Is.EqualTo(-1));
        }
    }
}
