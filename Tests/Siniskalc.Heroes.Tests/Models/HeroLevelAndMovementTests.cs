namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the level, the experience and the movement points of a hero, and the exception for a rule of the game.
    /// </summary>
    [TestFixture]
    internal sealed class HeroLevelAndMovementTests
    {
        [Test]
        public void LevelsAndMovementPointsAreValuesThatCompareByWhatTheyHold()
        {
            Assert.That(new HeroLevel(1500, 2), Is.EqualTo(new HeroLevel(1500, 2)));
            Assert.That(new MovementPoints(500, 1000) with { Current = 1000 }, Is.EqualTo(new MovementPoints(1000, 1000)));
        }

        [Test]
        public void ALevelIsReachedWithTheExperienceTheTableStartsItAt()
        {
            var table = new ExperienceTable([0, 1000, 2500, 4500], 4);

            Assert.That(HeroLevel.AtLevel(table, 3), Is.EqualTo(new HeroLevel(2500, 3)));
            Assert.That(HeroLevel.AtLevel(table, 0), Is.EqualTo(new HeroLevel(0, 1)), "below the first level reads as the first");
            Assert.That(HeroLevel.AtLevel(table, 9), Is.EqualTo(new HeroLevel(4500, 4)), "above the last reads as the last");
        }

        [Test]
        public void ExperienceKeepsItsPointsAndTakesTheLevelTheTableGivesIt()
        {
            var table = new ExperienceTable([0, 1000, 2500, 4500], 4);

            Assert.That(HeroLevel.AtExperience(table, 2499), Is.EqualTo(new HeroLevel(2499, 2)));
            Assert.That(HeroLevel.AtExperience(table, 2500), Is.EqualTo(new HeroLevel(2500, 3)));
        }

        [Test]
        public void ARuleOfTheGameBrokenIsAnInvalidOperationWithTheMessageOfTheRule()
        {
            var exception = new GameRuleException("Every artifact slot is taken.");

            Assert.That(exception, Is.InstanceOf<InvalidOperationException>());
            Assert.That(exception.Message, Is.EqualTo("Every artifact slot is taken."));
        }
    }
}
