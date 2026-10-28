using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Homm3.Tests.Integration.Models;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// Tests the secondary skills of a hero on its page: the slots, their order and their levels.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SecondarySkillsViewModelTests
    {
        private Game game;

        private HeroViewModel hero;

        private SecondarySkillsViewModel Skills => this.hero.SecondarySkillsViewModel;

        [SetUp]
        public void SetUp()
        {
            // Christian has Archery (basic), Wisdom (expert) and Luck (advanced), in that order.
            this.game = HeroEditingTests.LoadPrepared();
            this.hero = TestServices.CreateHeroViewModel(this.game.History.CreateHeroSession(this.game.History.Current.Heroes().FindByName("Christian")));
        }

        [TearDown]
        public void TearDown()
        {
            this.hero.Dispose();
        }

        [Test]
        public void SlotsShowTheSkillsInOrderAndFreeSlotsAreEmpty()
        {
            var slots = this.Skills.Slots;

            Assert.That(slots, Has.Count.EqualTo(Constants.SkillSlots.Max));
            Assert.That(slots.Take(4).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Wisdom", "Luck", null }));
            Assert.That(slots[1].LevelName, Is.EqualTo(SkillLevelNames.Expert));
            Assert.That(slots[2].LevelName, Is.EqualTo(SkillLevelNames.Advanced));
            Assert.That(slots[1].Pips, Is.EqualTo(new[] { true, true, true }));
            Assert.That(slots[3].IsFilled, Is.False);
            Assert.That(this.Skills.SlotSummary, Is.EqualTo("3 of 8 slots used"));
        }

        [Test]
        public void MovingASkillSwapsItWithItsNeighbour()
        {
            var slots = this.Skills.Slots;

            Assert.That(slots[0].MoveUpCommand.CanExecute(null), Is.False);
            Assert.That(slots[2].MoveDownCommand.CanExecute(null), Is.False);

            slots[0].MoveDownCommand.Execute(null);

            Assert.That(slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Wisdom", "Archery", "Luck" }));
            Assert.That(slots.Take(2).Select(s => s.LevelName), Is.EqualTo(new[] { SkillLevelNames.Expert, SkillLevelNames.Basic }));
        }

        [Test]
        public void ClearingASkillMovesTheLaterOnesUp()
        {
            var slots = this.Skills.Slots;

            slots[0].ClearCommand.Execute(null);

            Assert.That(slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Wisdom", "Luck", null }));
            Assert.That(slots[2].ClearCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void ASkillPickedInASlotKeepsTheLevelOfTheSlotAndASkillTheHeroHasIsNotAddedTwice()
        {
            var slots = this.Skills.Slots;

            slots[3].SkillName = "Tactics";
            Assert.That(slots[3].SkillName, Is.EqualTo("Tactics"));
            Assert.That(slots[3].LevelName, Is.EqualTo(SkillLevelNames.Basic));

            // Replacing a skill keeps the slot's level.
            slots[1].SkillName = "Offense";
            Assert.That(slots.Take(4).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Offense", "Luck", "Tactics" }));
            Assert.That(slots[1].LevelName, Is.EqualTo(SkillLevelNames.Expert));

            slots[1].LevelName = SkillLevelNames.Basic;
            Assert.That(slots[1].LevelName, Is.EqualTo(SkillLevelNames.Basic));

            // A skill the hero already has is not added twice, and the box goes back to what is stored.
            slots[0].SkillName = "Luck";
            Assert.That(slots[0].SkillName, Is.EqualTo("Archery"));

            // Only the first free slot can be filled.
            slots[5].SkillName = "Scouting";
            Assert.That(slots[5].IsFilled, Is.False);
        }

        [Test]
        public void AnySkillCanBeGivenALevelAndBeyondTheSlotsItBecomesExtra()
        {
            var all = this.Skills.AllSkills;
            SkillRowViewModel Row(string name) => all.Single(r => r.Name == name);

            Assert.That(Row("Archery").LevelName, Is.EqualTo(SkillLevelNames.Basic));
            Assert.That(Row("Archery").Placement, Is.EqualTo("Slot 1"));
            Assert.That(Row("Sorcery").LevelName, Is.EqualTo(SkillLevelNames.None));
            Assert.That(Row("Sorcery").Placement, Is.Empty);

            // Fill the remaining slots, then one more.
            foreach (var name in new[] { "Tactics", "Scouting", "Learning", "Offense", "Armorer", "Sorcery", "Mysticism" })
            {
                Row(name).LevelName = SkillLevelNames.Advanced;
            }

            // Three skills plus five of the new ones fill the eight slots; Sorcery and Mysticism have no slot left.
            Assert.That(this.Skills.Slots.Count(s => s.IsFilled), Is.EqualTo(Constants.SkillSlots.Max));
            Assert.That(Row("Armorer").Placement, Is.EqualTo("Slot 8"));
            Assert.That(Row("Mysticism").LevelName, Is.EqualTo(SkillLevelNames.Advanced));
            Assert.That(Row("Mysticism").IsExtra, Is.True);
            Assert.That(Row("Mysticism").Placement, Does.Contain("Not shown"));
            Assert.That(this.Skills.HasExtraSkills, Is.True);
            Assert.That(this.Skills.ExtraSummary, Does.StartWith("2 skills"));
            Assert.That(this.hero.HeroModel.SecondarySkills.GetSkillLevel(SecondarySkill.Mysticism), Is.EqualTo(SkillLevel.Intermediate));

            // An extra skill can be raised, and removing it with level "None" leaves the slots alone.
            Row("Mysticism").LevelName = SkillLevelNames.Expert;
            Assert.That(this.hero.HeroModel.SecondarySkills.GetSkillLevel(SecondarySkill.Mysticism), Is.EqualTo(SkillLevel.Expert));
            Row("Mysticism").LevelName = SkillLevelNames.None;
            Row("Sorcery").LevelName = SkillLevelNames.None;
            Assert.That(this.Skills.HasExtraSkills, Is.False);
            Assert.That(this.Skills.Slots.Count(s => s.IsFilled), Is.EqualTo(Constants.SkillSlots.Max));
        }

        [Test]
        public void HidingASkillKeepsItsLevelAndShowingPutsItBackInAFreeSlot()
        {
            var all = this.Skills.AllSkills;
            var wisdom = all.Single(r => r.Name == "Wisdom");

            wisdom.HideCommand.Execute(null);

            Assert.That(this.Skills.Slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Luck", null }));
            Assert.That(wisdom.LevelName, Is.EqualTo(SkillLevelNames.Expert));
            Assert.That(wisdom.IsExtra, Is.True);
            Assert.That(wisdom.ShowCommand.CanExecute(null), Is.True);

            wisdom.ShowCommand.Execute(null);

            Assert.That(this.Skills.Slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Luck", "Wisdom" }));
            Assert.That(wisdom.IsExtra, Is.False);
        }
    }
}
