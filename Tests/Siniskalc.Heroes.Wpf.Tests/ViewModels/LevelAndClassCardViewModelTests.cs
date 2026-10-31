namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the card of the level, the experience and the class of a hero.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class LevelAndClassCardViewModelTests
    {
        private int level;
        private string heroClass = "Knight";
        private string gender = "Male";

        [SetUp]
        public void StartWithAKnight()
        {
            this.level = 3;
            this.heroClass = "Knight";
            this.gender = "Male";
        }

        [Test]
        public void ALevelOutsideTheLimitsIsIgnored()
        {
            var card = this.Make(canChangeClass: true, withGender: true);

            card.Level = 0;
            Assert.That(this.level, Is.EqualTo(3));

            card.Level = 12;
            Assert.That(this.level, Is.EqualTo(12));
        }

        [Test]
        public void AGameWhoseHeroesCannotChangeClassShowsTheClassAndLeavesItAlone()
        {
            var card = this.Make(canChangeClass: false, withGender: false);

            card.HeroClass = "Wizard";

            Assert.That(card.CanChangeClass, Is.False);
            Assert.That(card.HeroClass, Is.EqualTo("Knight"));
        }

        [Test]
        public void AGameWithoutGendersHasNoGenderToEdit()
        {
            var card = this.Make(canChangeClass: true, withGender: false);

            card.Gender = "Female";

            Assert.That(card.HasGender, Is.False);
            Assert.That(this.gender, Is.EqualTo("Male"));
        }

        [Test]
        public void TheClassAndTheGenderAreWrittenWhenTheyChange()
        {
            var card = this.Make(canChangeClass: true, withGender: true);

            card.HeroClass = "Wizard";
            card.Gender = "Female";

            Assert.That(this.heroClass, Is.EqualTo("Wizard"));
            Assert.That(this.gender, Is.EqualTo("Female"));
        }

        private LevelAndClassCardViewModel Make(bool canChangeClass, bool withGender) => new(
            () => this.level,
            value => this.level = value,
            new NumberRange(1, 75),
            () => 0,
            _ => { },
            new NumberRange(0, 1_000_000, 100),
            () => "next",
            ["Knight", "Wizard"],
            () => this.heroClass,
            canChangeClass ? value => this.heroClass = value : null,
            withGender ? ["Male", "Female"] : null,
            withGender ? () => this.gender : null,
            withGender ? value => this.gender = value : null);
    }
}
