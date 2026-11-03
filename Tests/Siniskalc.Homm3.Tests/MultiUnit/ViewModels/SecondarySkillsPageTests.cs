using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The secondary skills of a hero on its page: the slots, their order and their levels, over a hero session with no savefile.
    /// The hero has Archery (basic), Wisdom (expert) and Luck (advanced), in that order.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class SecondarySkillsPageTests
    {
        private FakeHeroSession session = null!;
        private HeroViewModel page = null!;

        private SecondarySkillsViewModel Skills => this.page.SecondarySkillsViewModel;

        [SetUp]
        public void StartWithArcheryWisdomAndLuck()
        {
            var levels = TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.All.ToDictionary(s => s, _ => SkillLevel.None);
            levels[TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Archery")] = SkillLevel.Beginner;
            levels[TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom")] = SkillLevel.Expert;
            levels[TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Luck")] = SkillLevel.Intermediate;
            var hero = HeroModels.Christian() with { SecondarySkills = new SecondarySkills(levels, [TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Archery"), TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom"), TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Luck")]) };
            this.session = new FakeHeroSession(hero);
            this.page = HeroPages.Create(this.session);
        }

        [TearDown]
        public void ClosePage()
        {
            this.page.Dispose();
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
            Assert.That(slots[3].LevelName, Is.EqualTo(SkillLevelNames.Basic), "a free slot offers basic for the skill that will be picked");
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
            Assert.That(this.session.Current.SecondarySkills.GetSkillInSlot(0), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom")));
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
        public void ASkillPickedInTheFirstFreeSlotStartsAtBasic()
        {
            var slots = this.Skills.Slots;

            slots[3].SkillName = "Tactics";

            Assert.That(slots[3].SkillName, Is.EqualTo("Tactics"));
            Assert.That(slots[3].LevelName, Is.EqualTo(SkillLevelNames.Basic));
        }

        [Test]
        public void ReplacingASkillKeepsTheLevelOfTheSlot()
        {
            var slots = this.Skills.Slots;

            slots[1].SkillName = "Offense";

            Assert.That(slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Offense", "Luck" }));
            Assert.That(slots[1].LevelName, Is.EqualTo(SkillLevelNames.Expert));
        }

        [Test]
        public void TheLevelOfASlotCanBeChanged()
        {
            var slots = this.Skills.Slots;

            slots[1].LevelName = SkillLevelNames.Basic;

            Assert.That(slots[1].LevelName, Is.EqualTo(SkillLevelNames.Basic));
            Assert.That(this.session.Current.SecondarySkills.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom")), Is.EqualTo(SkillLevel.Beginner));
        }

        [Test]
        public void ASkillTheHeroAlreadyHasIsNotAddedTwiceAndTheBoxGoesBackToWhatIsStored()
        {
            var slots = this.Skills.Slots;

            slots[0].SkillName = "Luck";

            Assert.That(slots[0].SkillName, Is.EqualTo("Archery"));
        }

        [Test]
        public void OnlyTheFirstFreeSlotCanBeFilled()
        {
            var slots = this.Skills.Slots;

            slots[5].SkillName = "Scouting";

            Assert.That(slots[5].IsFilled, Is.False);
        }

        [Test]
        public void EverySkillRowShowsItsLevelAndWhereItIs()
        {
            var all = this.Skills.AllSkills;

            Assert.That(all.Single(r => r.Name == "Archery").LevelName, Is.EqualTo(SkillLevelNames.Basic));
            Assert.That(all.Single(r => r.Name == "Archery").Placement, Is.EqualTo("Slot 1"));
            Assert.That(all.Single(r => r.Name == "Sorcery").LevelName, Is.EqualTo(SkillLevelNames.None));
            Assert.That(all.Single(r => r.Name == "Sorcery").Placement, Is.Empty);
        }

        [Test]
        public void BeyondTheEightSlotsASkillWithALevelBecomesAnExtraNotShown()
        {
            var all = this.Skills.AllSkills;
            SkillRowViewModel Row(string name) => all.Single(r => r.Name == name);

            foreach (var name in new[] { "Tactics", "Scouting", "Learning", "Offense", "Armorer", "Sorcery", "Mysticism" })
            {
                Row(name).LevelName = SkillLevelNames.Advanced;
            }

            Assert.That(this.Skills.Slots.Count(s => s.IsFilled), Is.EqualTo(Constants.SkillSlots.Max));
            Assert.That(Row("Armorer").Placement, Is.EqualTo("Slot 8"));
            Assert.That(Row("Mysticism").LevelName, Is.EqualTo(SkillLevelNames.Advanced));
            Assert.That(Row("Mysticism").IsExtra, Is.True);
            Assert.That(Row("Mysticism").Placement, Does.Contain("Not shown"));
            Assert.That(this.Skills.HasExtraSkills, Is.True);
            Assert.That(this.Skills.ExtraSummary, Does.StartWith("2 skills"));
            Assert.That(this.session.Current.SecondarySkills.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Mysticism")), Is.EqualTo(SkillLevel.Intermediate));
        }

        [Test]
        public void AnExtraSkillCanBeRaisedAndRemovedWithLevelNoneWithoutTouchingTheSlots()
        {
            var all = this.Skills.AllSkills;
            SkillRowViewModel Row(string name) => all.Single(r => r.Name == name);
            foreach (var name in new[] { "Tactics", "Scouting", "Learning", "Offense", "Armorer", "Sorcery", "Mysticism" })
            {
                Row(name).LevelName = SkillLevelNames.Advanced;
            }

            Row("Mysticism").LevelName = SkillLevelNames.Expert;
            Assert.That(this.session.Current.SecondarySkills.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Mysticism")), Is.EqualTo(SkillLevel.Expert));
            Row("Mysticism").LevelName = SkillLevelNames.None;
            Row("Sorcery").LevelName = SkillLevelNames.None;

            Assert.That(this.Skills.HasExtraSkills, Is.False);
            Assert.That(this.Skills.Slots.Count(s => s.IsFilled), Is.EqualTo(Constants.SkillSlots.Max));
        }

        [Test]
        public void HidingASkillKeepsItsLevelAndShowingPutsItBackInAFreeSlot()
        {
            var wisdom = this.Skills.AllSkills.Single(r => r.Name == "Wisdom");

            wisdom.HideCommand.Execute(null);

            Assert.That(this.Skills.Slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Luck", null }));
            Assert.That(wisdom.LevelName, Is.EqualTo(SkillLevelNames.Expert));
            Assert.That(wisdom.IsExtra, Is.True);
            Assert.That(wisdom.ShowCommand.CanExecute(null), Is.True);

            wisdom.ShowCommand.Execute(null);

            Assert.That(this.Skills.Slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Luck", "Wisdom" }));
            Assert.That(wisdom.IsExtra, Is.False);
        }

        [Test]
        public void PuttingTheHeroBackShowsTheSkillsItHadAgain()
        {
            this.Skills.Slots[0].ClearCommand.Execute(null);

            this.session.Update(_ => this.session.Original);

            Assert.That(this.Skills.Slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Wisdom", "Luck" }));
        }
    }
}
