namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the card of the movement points and the spell points of a hero.
    /// </summary>
    [TestFixture]
    internal sealed class MovementAndSpellPointsCardViewModelTests
    {
        private long movement;
        private int spellPoints;
        private int? mostSpellPoints;

        [SetUp]
        public void StartWithAHero()
        {
            this.movement = 500;
            this.spellPoints = 10;
            this.mostSpellPoints = 40;
        }

        [Test]
        public void MovementIsClampedToTheLimitsOfTheGame()
        {
            var card = this.Make();

            card.CurrentMovement = 1_000_000;
            Assert.That(this.movement, Is.EqualTo(99_999));

            card.CurrentMovement = -5;
            Assert.That(this.movement, Is.EqualTo(0));
        }

        [Test]
        public void TheFullDayButtonGivesTheMovementOfAFullDayBackAndIsOffWhenTheHeroHasIt()
        {
            var card = this.Make();

            Assert.That(card.RestoreMovementCommand.CanExecute(null), Is.True);
            card.RestoreMovementCommand.Execute(null);

            Assert.That(this.movement, Is.EqualTo(1500));
            Assert.That(card.RestoreMovementCommand.CanExecute(null), Is.False);
            Assert.That(card.MaxMovementText, Is.EqualTo("Maximum 1500"));
        }

        [Test]
        public void ResettingTheSpellPointsFillsThemToTheMostTheHeroCanHaveAndNeverPastWhatCanBeSaved()
        {
            var card = this.Make();

            card.ResetSpellPointsCommand.Execute(null);
            Assert.That(this.spellPoints, Is.EqualTo(40));
            Assert.That(card.MaxSpellPointsText, Is.EqualTo("Maximum 40"));

            this.mostSpellPoints = 100_000;
            card.ResetSpellPointsCommand.Execute(null);
            Assert.That(this.spellPoints, Is.EqualTo(999));
        }

        [Test]
        public void AGameThatDoesNotWorkOutTheMostSpellPointsHasNoResetButAMaxOne()
        {
            this.mostSpellPoints = null;
            var card = this.Make();

            Assert.That(card.HasSpellPointsMaximum, Is.False);
            Assert.That(card.ResetSpellPointsCommand.CanExecute(null), Is.False);
            Assert.That(card.MaxSpellPointsText, Is.Empty);

            card.MaxSpellPointsCommand.Execute(null);
            Assert.That(this.spellPoints, Is.EqualTo(999));
            Assert.That(card.MaxSpellPointsCommand.CanExecute(null), Is.False);
        }

        private MovementAndSpellPointsCardViewModel Make() => new(
            () => this.movement,
            value => this.movement = value,
            () => 1500,
            new NumberRange(0, 99_999, 100),
            () => this.spellPoints,
            value => this.spellPoints = value,
            new NumberRange(0, 999),
            this.mostSpellPoints == null ? null : () => this.mostSpellPoints.Value);
    }
}
