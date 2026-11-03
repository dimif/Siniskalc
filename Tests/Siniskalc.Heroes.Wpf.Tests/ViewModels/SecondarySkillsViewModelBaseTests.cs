namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests what every game's page of secondary skills does: shows the skills of the hero in its slots and in the list of all skills,
    /// and passes the choices of the user to the game. The game's part is a fake that notes what it was asked.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SecondarySkillsViewModelBaseTests
    {
        private static TestSkills Page(bool supportsHiding = false)
        {
            var page = new TestSkills(3, supportsHiding);
            page.InSlots.Add(("Archery", "Basic"));
            page.InSlots.Add(("Wisdom", "Expert"));
            page.Refresh();
            return page;
        }

        private static SkillRowViewModel Row(TestSkills page, string name) => page.AllSkills.Single(r => r.Name == name);

        [Test]
        public void ThePageHasAFixedNumberOfSlotsAndARowForEverySkill()
        {
            var page = new TestSkills(4);

            Assert.That(page.Title, Is.EqualTo("Secondary Skills"));
            Assert.That(page.Slots.Select(s => s.Index), Is.EqualTo(new[] { 0, 1, 2, 3 }));
            Assert.That(page.AllSkills.Select(r => r.Name), Is.EqualTo(new[] { "Archery", "Luck", "Wisdom", "Tactics", "Scouting" }));
            Assert.That(page.SkillNames, Has.Count.EqualTo(5));
        }

        [Test]
        public void TheSlotsAndTheSkillsAreSplitInTwoColumnsTheLeftOneTheLongerWhenOdd()
        {
            var page = new TestSkills(4);

            Assert.That(page.LeftSlots.Select(s => s.Index), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(page.RightSlots.Select(s => s.Index), Is.EqualTo(new[] { 2, 3 }));
            Assert.That(page.LeftSkills.Select(r => r.Name), Is.EqualTo(new[] { "Archery", "Luck", "Wisdom" }));
            Assert.That(page.RightSkills.Select(r => r.Name), Is.EqualTo(new[] { "Tactics", "Scouting" }));
        }

        [Test]
        public void ASlotOffersTheThreeLevelsAndARowOffersNoneAsWell()
        {
            var page = new TestSkills();

            Assert.That(page.SlotLevelNames, Is.EqualTo(new[] { "Basic", "Advanced", "Expert" }));
            Assert.That(page.RowLevelNames, Is.EqualTo(new[] { "None", "Basic", "Advanced", "Expert" }));
        }

        [Test]
        public void TheSummaryTellsHowManySlotsAreUsed()
        {
            Assert.That(Page().SlotSummary, Is.EqualTo("2 of 3 slots used"));
        }

        [TestCase(0, false, "0 skills are applied by the game but not shown on the hero screen.")]
        [TestCase(1, true, "1 skill is applied by the game but not shown on the hero screen.")]
        [TestCase(2, true, "2 skills are applied by the game but not shown on the hero screen.")]
        public void TheExtraSkillsAreCountedAndWorded(int extra, bool has, string text)
        {
            var page = new TestSkills { Extra = extra };

            Assert.That((page.HasExtraSkills, page.ExtraSummary), Is.EqualTo((has, text)));
        }

        [Test]
        public void RefreshShowsTheSkillsInTheirSlotsAndLeavesTheRestEmptyAtBasic()
        {
            var page = Page();

            Assert.That(page.Slots.Select(s => (s.SkillName, s.LevelName)), Is.EqualTo(new (string?, string)[] { ("Archery", "Basic"), ("Wisdom", "Expert"), (null, "Basic") }));
        }

        [Test]
        public void OnlyTheFilledSlotsInTheMiddleCanMoveBothWays()
        {
            var page = Page();

            Assert.That(page.Slots.Select(s => s.MoveUpCommand.CanExecute(null)), Is.EqualTo(new[] { false, true, false }));
            Assert.That(page.Slots.Select(s => s.MoveDownCommand.CanExecute(null)), Is.EqualTo(new[] { true, false, false }));
        }

        [Test]
        public void RefreshShowsEachSkillRowItsLevelAndItsSlot()
        {
            var page = Page();

            Assert.That(page.AllSkills.Select(r => (r.Name, r.LevelName, r.Placement)), Is.EqualTo(new[]
            {
                ("Archery", "Basic", "Slot 1"),
                ("Luck", "None", string.Empty),
                ("Wisdom", "Expert", "Slot 2"),
                ("Tactics", "None", string.Empty),
                ("Scouting", "None", string.Empty),
            }));
        }

        [Test]
        public void ASkillTheGameAppliesButDoesNotShowIsExtraAndCanBeShownWhileASlotIsFree()
        {
            var page = Page(supportsHiding: true);
            page.Extras["Luck"] = "Advanced";
            page.Refresh();

            var luck = Row(page, "Luck");

            Assert.That((luck.IsExtra, luck.ShowCommand.CanExecute(null)), Is.EqualTo((true, true)));
        }

        [Test]
        public void AnExtraSkillCannotBeShownWhenAllTheSlotsAreFull()
        {
            var page = Page(supportsHiding: true);
            page.InSlots.Add(("Tactics", "Basic"));
            page.Extras["Luck"] = "Advanced";
            page.Refresh();

            Assert.That(Row(page, "Luck").ShowCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void RefreshingWritesNothingToTheGameAndTellsEveryBinding()
        {
            var page = Page();
            page.Calls.Clear();
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            page.Refresh();

            Assert.That(page.Calls, Is.Empty);
            Assert.That(raised, Does.Contain(string.Empty));
        }

        [Test]
        public void PickingAnotherSkillForAFilledSlotReplacesItAndShowsTheNewSkill()
        {
            var page = Page();

            page.Slots[0].SkillName = "Luck";

            Assert.That(page.Calls, Is.EqualTo(new[] { "replace 0 Luck" }));
            Assert.That(page.Slots[0].SkillName, Is.EqualTo("Luck"));
        }

        [Test]
        public void PickingASkillForTheFirstFreeSlotAddsIt()
        {
            var page = Page();

            page.Slots[2].SkillName = "Tactics";

            Assert.That(page.Calls, Is.EqualTo(new[] { "add Tactics" }));
        }

        [Test]
        public void ASlotBeyondTheFirstFreeOneCannotBeFilled()
        {
            var page = new TestSkills(4);
            page.InSlots.Add(("Archery", "Basic"));
            page.Refresh();

            page.Slots[2].SkillName = "Tactics";

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void ASkillTheGameDoesNotHaveIsRefused()
        {
            var page = Page();

            page.Slots[0].SkillName = "Diplomacy";
            page.Slots[2].SkillName = "Diplomacy";

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void PickingALevelForAFilledSlotSetsItAndShowsIt()
        {
            var page = Page();

            page.Slots[0].LevelName = "Advanced";

            Assert.That(page.Calls, Is.EqualTo(new[] { "level in slot 0 Advanced" }));
            Assert.That(page.Slots[0].LevelName, Is.EqualTo("Advanced"));
        }

        [Test]
        public void PickingALevelOfASkillSetsItEvenForASkillNotInASlot()
        {
            var page = Page();

            Row(page, "Luck").LevelName = "Expert";

            Assert.That(page.Calls, Is.EqualTo(new[] { "level of Luck Expert" }));
            Assert.That(Row(page, "Luck").LevelName, Is.EqualTo("Expert"));
        }

        [Test]
        public void TakingASkillBackToNoneIsAskedOfTheGame()
        {
            var page = Page();

            Row(page, "Archery").LevelName = "None";

            Assert.That(page.Calls, Is.EqualTo(new[] { "level of Archery None" }));
        }

        [Test]
        public void MovingASkillSwapsItWithItsNeighbourOnlyWithinTheUsedSlots()
        {
            var page = Page();

            page.MoveSkill(0, 1);
            page.MoveSkill(1, -1);
            page.MoveSkill(1, 1);
            page.MoveSkill(2, -1);
            page.MoveSkill(0, -1);
            page.MoveSkill(-1, 1);

            Assert.That(page.Calls, Is.EqualTo(new[] { "swap 0 1", "swap 1 0" }));
        }

        [Test]
        public void ClearingASkillRemovesItsSlotOnlyForAUsedSlot()
        {
            var page = Page();

            page.ClearSkill(2);
            page.ClearSkill(-1);
            page.ClearSkill(0);

            Assert.That(page.Calls, Is.EqualTo(new[] { "remove 0" }));
        }

        [Test]
        public void TheButtonsOfASlotMoveAndClearIt()
        {
            var page = Page();

            page.Slots[0].MoveDownCommand.Execute(null);
            page.Slots[1].ClearCommand.Execute(null);

            Assert.That(page.Calls, Is.EqualTo(new[] { "swap 0 1", "remove 1" }));
        }

        [Test]
        public void WhileThePageIsBeingRefreshedNoChoiceReachesTheGame()
        {
            var page = Page(supportsHiding: true);
            page.Extras["Scouting"] = "Basic";
            page.Refresh();
            page.OnLevelRead = () =>
            {
                page.Slots[0].SkillName = "Luck";
                page.Slots[0].LevelName = "Expert";
                Row(page, "Luck").LevelName = "Expert";
                page.MoveSkill(0, 1);
                page.ClearSkill(0);
                Row(page, "Archery").HideCommand.Execute(null);
                page.Slots[1].HideCommand.Execute(null);
                Row(page, "Scouting").ShowCommand.Execute(null);
            };
            page.Calls.Clear();

            page.Refresh();

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void AGameThatCannotHideSkillsIgnoresTheHideButtons()
        {
            var page = Page(supportsHiding: false);

            page.Slots[0].HideCommand.Execute(null);
            Row(page, "Wisdom").HideCommand.Execute(null);

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void AGameThatCanHideSkillsHidesTheSlotOrTheSkillByName()
        {
            var page = Page(supportsHiding: true);

            page.Slots[0].HideCommand.Execute(null);
            Row(page, "Wisdom").HideCommand.Execute(null);

            Assert.That(page.Calls, Is.EqualTo(new[] { "hide 0", "hide 0" }), "after the first hide Wisdom is in the first slot");
        }

        [Test]
        public void HidingAnEmptySlotDoesNothing()
        {
            var page = Page(supportsHiding: true);

            page.Slots[2].HideCommand.Execute(null);

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void TheLevelNoneIsNotOneOfASlotAndIsIgnored()
        {
            var page = Page();

            page.Slots[0].LevelName = "None";

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void ARowOfASkillInASlotCanBeHiddenButNotShownAndARowOfAnotherSkillCanDoNeither()
        {
            var page = Page(supportsHiding: true);

            Assert.That((Row(page, "Archery").HideCommand.CanExecute(null), Row(page, "Archery").ShowCommand.CanExecute(null)), Is.EqualTo((true, false)));
            Assert.That((Row(page, "Scouting").HideCommand.CanExecute(null), Row(page, "Scouting").ShowCommand.CanExecute(null)), Is.EqualTo((false, false)));
        }

        [Test]
        public void ARowSaysWhereItsSkillIsAndAnExtraSkillIsNotShownOnTheHeroScreen()
        {
            var page = Page(supportsHiding: true);
            page.Extras["Luck"] = "Advanced";
            page.Refresh();

            Assert.That((Row(page, "Archery").IsExtra, Row(page, "Archery").HasSkill, Row(page, "Archery").IsInSlot), Is.EqualTo((false, true, true)));
            Assert.That((Row(page, "Luck").IsExtra, Row(page, "Luck").Placement), Is.EqualTo((true, "Not shown on the hero screen")));
            Assert.That((Row(page, "Tactics").IsExtra, Row(page, "Tactics").HasSkill, Row(page, "Tactics").IsInSlot), Is.EqualTo((false, false, false)));
        }

        [TestCase("")]
        [TestCase("Basic")]
        public void ARowLevelThatIsEmptyOrTheOneItAlreadyHasTellsNothing(string level)
        {
            var page = Page();

            Row(page, "Archery").LevelName = level;

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void HidingASkillThatIsInNoSlotDoesNothing()
        {
            var page = Page(supportsHiding: true);

            Row(page, "Scouting").HideCommand.Execute(null);

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void AnExtraSkillIsShownAgainByItsButtonOnlyWhereTheGameCanHide()
        {
            var hiding = Page(supportsHiding: true);
            hiding.Extras["Luck"] = "Advanced";
            hiding.Refresh();
            var plain = Page(supportsHiding: false);
            plain.Extras["Luck"] = "Advanced";
            plain.Refresh();

            Row(hiding, "Luck").ShowCommand.Execute(null);
            Row(plain, "Luck").ShowCommand.Execute(null);

            Assert.That(hiding.Calls, Is.EqualTo(new[] { "show Luck" }));
            Assert.That(plain.Calls, Is.Empty);
        }
    }
}
