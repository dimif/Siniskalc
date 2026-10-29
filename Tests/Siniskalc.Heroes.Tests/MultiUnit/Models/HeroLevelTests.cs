namespace Siniskalc.Heroes.Tests.MultiUnit.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the level and the experience of a hero, which every game keeps in step through its table of experience.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class HeroLevelTests
    {
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
    }
}
