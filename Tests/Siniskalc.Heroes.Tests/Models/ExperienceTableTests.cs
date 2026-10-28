namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests how a level is worked out from experience, whatever thresholds a game gives.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ExperienceTableTests
    {
        private static readonly long[] Thresholds = [0, 1000, 2500, 5000];

        [TestCase(0, 1)]
        [TestCase(999, 1)]
        [TestCase(1000, 2)]
        [TestCase(4999, 3)]
        [TestCase(5000, 4)]
        [TestCase(1_000_000, 4)]
        public void ALevelIsReachedAtItsThreshold(long experience, int level)
        {
            var table = new ExperienceTable(Thresholds, 4);

            Assert.That(table.LevelOf(experience), Is.EqualTo(level));
        }

        [Test]
        public void TheNextLevelIsTheOneAboveTheCurrentOneAndNothingAtTheTop()
        {
            var table = new ExperienceTable(Thresholds, 4);

            Assert.That(table.NextLevel(1200), Is.EqualTo((3, 2500L)));
            Assert.That(table.NextLevel(5000), Is.Null);
        }

        [Test]
        public void LevelsAfterTheLastThresholdFollowTheRule()
        {
            var table = new ExperienceTable(Thresholds, 8, (previous, beforeThat) => previous + (2 * (previous - beforeThat)));

            Assert.That(table.ExperienceOf(5), Is.EqualTo(5000 + (2 * 2500)));
            Assert.That(table.LevelOf(table.ExperienceOf(7) - 1), Is.EqualTo(6));
            Assert.That(table.ExperienceOf(99), Is.EqualTo(table.ExperienceOf(8)), "above the highest level reads as the highest");
        }

        [Test]
        public void ARuleThatGrowsPastWhatANumberCanHoldStaysAtTheLargest()
        {
            var table = new ExperienceTable(Thresholds, 200, (previous, _) => previous * 10);

            Assert.That(table.ExperienceOf(200), Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void ATableWithoutARuleEndsWithItsLastThreshold()
        {
            Assert.That(() => new ExperienceTable(Thresholds, 9), Throws.ArgumentException);
        }
    }
}
