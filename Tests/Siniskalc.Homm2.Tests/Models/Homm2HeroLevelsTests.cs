namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Homm2.Models;

    [TestFixture]
    internal sealed class Homm2HeroLevelsTests
    {
        [TestCase(0, 1)]
        [TestCase(999, 1)]
        [TestCase(14_700, 10)]
        [TestCase(15_500, 11)]
        [TestCase(50_000, 17)]
        [TestCase(200_000, 25)]
        public void TheLevelIsWorkedOutFromTheExperienceAsTheGameShowedIt(long experience, int level)
        {
            Assert.That(Homm2HeroLevels.LevelOf(experience), Is.EqualTo(level));
        }

        [Test]
        public void ALevelStartsWhereTheLevelBeforeEnds()
        {
            for (var level = 1; level < 60; level++)
            {
                Assert.That(Homm2HeroLevels.LevelOf(Homm2HeroLevels.ExperienceOf(level)), Is.EqualTo(level));
                Assert.That(Homm2HeroLevels.LevelOf(Homm2HeroLevels.ExperienceOf(level + 1) - 1), Is.EqualTo(level));
            }
        }
    }
}
