namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the row of one primary skill of a hero: the base value kept within its range, the total and the note about artifacts.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PrimarySkillRowViewModelTests
    {
        private int stored;
        private int bonus;
        private List<int> written = null!;

        [SetUp]
        public void StartWithFive()
        {
            this.stored = 5;
            this.bonus = 0;
            this.written = [];
        }

        private PrimarySkillRowViewModel Row() =>
            new("Attack", new NumberRange(0, 99), () => this.stored, v =>
            {
                this.written.Add(v);
                this.stored = v;
            }, () => this.stored + this.bonus, () => this.bonus);

        [Test]
        public void ARowShowsItsLabelItsRangeAndItsBaseAndTotal()
        {
            this.bonus = 3;

            var row = this.Row();

            Assert.That((row.Label, row.Range.Minimum, row.Range.Maximum, row.Base, row.Total), Is.EqualTo(("Attack", 0L, 99L, 5L, 8)));
        }

        [Test]
        public void ABaseValueOutsideTheRangeIsShownAsTheNearestLimit()
        {
            this.stored = 150;
            Assert.That(this.Row().Base, Is.EqualTo(99));

            this.stored = -4;
            Assert.That(this.Row().Base, Is.EqualTo(0));
        }

        [Test]
        public void ANewBaseValueIsWrittenAndOneBeyondTheRangeIsWrittenAsTheLimit()
        {
            var row = this.Row();

            row.Base = 12;
            row.Base = 500;
            row.Base = -3;

            Assert.That(this.written, Is.EqualTo(new[] { 12, 99, 0 }));
        }

        [Test]
        public void TheValueItAlreadyHasIsNotWrittenAgain()
        {
            var row = this.Row();

            row.Base = 5;

            Assert.That(this.written, Is.Empty);
        }

        [Test]
        public void ABaseBeyondTheRangeThatIsAlreadyShownAsTheLimitIsNotWrittenAgain()
        {
            this.stored = 150;
            var row = this.Row();

            row.Base = 99;

            Assert.That(this.written, Is.Empty);
        }

        [TestCase(0, "")]
        [TestCase(6, "+6 from artifacts")]
        [TestCase(-2, "-2 from artifacts")]
        public void TheNoteSaysHowMuchTheArtifactsAddOrTakeAwayAndNothingForNoBonus(int artifactBonus, string note)
        {
            this.bonus = artifactBonus;

            Assert.That(this.Row().ArtifactNote, Is.EqualTo(note));
        }

        [Test]
        public void RefreshTellsEveryBindingToReadAgain()
        {
            var row = this.Row();
            var raised = new List<string?>();
            row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            row.Refresh();

            Assert.That(raised, Is.EqualTo(new[] { string.Empty }));
        }
    }
}
