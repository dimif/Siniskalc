namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests one of the slots of secondary skills of a hero on its page: the skill and level it holds, the choices it reports, its pips
    /// and the buttons that move, clear and hide it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SkillSlotViewModelTests
    {
        private List<(int Index, string Skill)> skills = null!;
        private List<(int Index, string Level)> levels = null!;
        private List<(int Index, int Direction)> moves = null!;
        private List<int> clears = null!;
        private List<int> hides = null!;

        [SetUp]
        public void StartWithNothingDone()
        {
            this.skills = [];
            this.levels = [];
            this.moves = [];
            this.clears = [];
            this.hides = [];
        }

        private SkillSlotViewModel Slot(int index = 1) =>
            new(index, (i, s) => this.skills.Add((i, s)), (i, l) => this.levels.Add((i, l)), (i, d) => this.moves.Add((i, d)), this.clears.Add, this.hides.Add);

        [Test]
        public void ANewSlotIsEmptyAtBasicAndShownFromOne()
        {
            var slot = this.Slot(1);

            Assert.That((slot.Index, slot.DisplayNumber, slot.IsFilled, slot.SkillName, slot.LevelName), Is.EqualTo((1, 2, false, (string?)null, "Basic")));
            Assert.That(slot.Pips, Is.EqualTo(new[] { false, false, false }));
        }

        [Test]
        public void ShowingASlotHoldsTheSkillAndTheLevelWithoutTellingAnybody()
        {
            var slot = this.Slot();

            slot.Show("Archery", "Advanced", true, true);

            Assert.That((slot.SkillName, slot.LevelName, slot.IsFilled), Is.EqualTo(("Archery", "Advanced", true)));
            Assert.That(this.skills, Is.Empty);
            Assert.That(this.levels, Is.Empty);
        }

        [Test]
        public void ShowingAnEmptyLevelFallsBackToBasic()
        {
            var slot = this.Slot();

            slot.Show("Archery", null!, true, true);

            Assert.That(slot.LevelName, Is.EqualTo("Basic"));
        }

        [Test]
        public void ShowingTellsEveryBindingToReadAgain()
        {
            var slot = this.Slot();
            var raised = new List<string?>();
            slot.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            slot.Show("Archery", "Expert", false, false);

            Assert.That(raised, Is.EqualTo(new[] { string.Empty }));
        }

        [Test]
        public void PickingAnotherSkillTellsThePageWithTheSlot()
        {
            var slot = this.Slot(3);
            slot.Show("Archery", "Basic", false, false);

            slot.SkillName = "Luck";

            Assert.That(this.skills, Is.EqualTo(new[] { (3, "Luck") }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Archery")]
        public void ASkillThatIsEmptyOrTheOneAlreadyThereTellsNothing(string? skill)
        {
            var slot = this.Slot();
            slot.Show("Archery", "Basic", false, false);

            slot.SkillName = skill;

            Assert.That(this.skills, Is.Empty);
        }

        [Test]
        public void AFreeSlotCanBeGivenASkill()
        {
            var slot = this.Slot(2);

            slot.SkillName = "Luck";

            Assert.That(this.skills, Is.EqualTo(new[] { (2, "Luck") }));
        }

        [Test]
        public void PickingAnotherLevelTellsThePageWithTheSlot()
        {
            var slot = this.Slot(3);
            slot.Show("Archery", "Basic", false, false);

            slot.LevelName = "Expert";

            Assert.That(this.levels, Is.EqualTo(new[] { (3, "Expert") }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Basic")]
        public void ALevelThatIsEmptyOrTheOneAlreadyThereTellsNothing(string? level)
        {
            var slot = this.Slot();
            slot.Show("Archery", "Basic", false, false);

            slot.LevelName = level!;

            Assert.That(this.levels, Is.Empty);
        }

        [Test]
        public void AFreeSlotHasNoLevelToChange()
        {
            var slot = this.Slot();

            slot.LevelName = "Expert";

            Assert.That(this.levels, Is.Empty);
        }

        [TestCase("Basic", new[] { true, false, false })]
        [TestCase("Advanced", new[] { true, true, false })]
        [TestCase("Expert", new[] { true, true, true })]
        [TestCase("None", new[] { false, false, false })]
        public void ThePipsShowTheLevelOfAFilledSlot(string level, bool[] pips)
        {
            var slot = this.Slot();

            slot.Show("Archery", level, false, false);

            Assert.That(slot.Pips, Is.EqualTo(pips));
        }

        [Test]
        public void AFreeSlotShowsNoPipsWhateverItsLevel()
        {
            var slot = this.Slot();

            slot.Show(null, "Expert", false, false);

            Assert.That(slot.Pips, Is.EqualTo(new[] { false, false, false }));
        }

        [Test]
        public void TheMoveButtonsAreOfferedOnlyWhereTheSlotCanMoveThatWayAndTellThePageTheSlotAndDirection()
        {
            var slot = this.Slot(2);
            slot.Show("Archery", "Basic", true, false);
            Assert.That((slot.MoveUpCommand.CanExecute(null), slot.MoveDownCommand.CanExecute(null)), Is.EqualTo((true, false)));

            slot.Show("Archery", "Basic", false, true);
            Assert.That((slot.MoveUpCommand.CanExecute(null), slot.MoveDownCommand.CanExecute(null)), Is.EqualTo((false, true)));

            slot.MoveUpCommand.Execute(null);
            slot.MoveDownCommand.Execute(null);
            Assert.That(this.moves, Is.EqualTo(new[] { (2, -1), (2, 1) }));
        }

        [Test]
        public void ClearAndHideAreOfferedOnlyForAFilledSlotAndTellThePageTheSlot()
        {
            var slot = this.Slot(2);
            Assert.That((slot.ClearCommand.CanExecute(null), slot.HideCommand.CanExecute(null)), Is.EqualTo((false, false)));

            slot.Show("Archery", "Basic", false, false);
            Assert.That((slot.ClearCommand.CanExecute(null), slot.HideCommand.CanExecute(null)), Is.EqualTo((true, true)));

            slot.ClearCommand.Execute(null);
            slot.HideCommand.Execute(null);
            Assert.That((this.clears, this.hides), Is.EqualTo((new[] { 2 }, new[] { 2 })));
        }
    }
}
