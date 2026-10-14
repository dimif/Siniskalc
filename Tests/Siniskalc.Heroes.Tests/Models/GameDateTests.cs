namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the date of a game.
    /// </summary>
    [TestFixture]
    internal sealed class GameDateTests
    {
        [Test]
        public void ADateIsToldAsMonthWeekAndDay()
        {
            Assert.That(new GameDate(2, 1, 3).ToString(), Is.EqualTo("Month 2, Week 1, Day 3"));
        }

        [Test]
        public void TwoDatesAreEqualWhenTheirMonthWeekAndDayAre()
        {
            Assert.That(new GameDate(1, 2, 3), Is.EqualTo(new GameDate(1, 2, 3)));
            Assert.That(new GameDate(1, 2, 3), Is.Not.EqualTo(new GameDate(3, 2, 1)));
        }
    }
}
