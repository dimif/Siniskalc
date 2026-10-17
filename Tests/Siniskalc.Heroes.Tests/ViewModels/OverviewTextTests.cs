namespace Siniskalc.Heroes.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// Tests the sentences the overview pages of the games share.
    /// </summary>
    [TestFixture]
    internal sealed class OverviewTextTests
    {
        [Test]
        public void TheDifficultyAndTheRatingNameTheLevelAndItsPercentage()
        {
            Assert.That(OverviewText.Difficulty(GameDifficulty.Queen), Is.EqualTo("Difficulty: Queen (160%)"));
            Assert.That(OverviewText.MapRating(GameDifficulty.Rook), Is.EqualTo("Rating: Rook (130%)"));
        }

        [Test]
        public void ACountOfOneUsesTheSingularAndAnyOtherCountThePlural()
        {
            Assert.That(OverviewText.Count(1, "hero", "heroes"), Is.EqualTo("1 hero"));
            Assert.That(OverviewText.Count(0, "hero", "heroes"), Is.EqualTo("0 heroes"));
        }
    }
}
