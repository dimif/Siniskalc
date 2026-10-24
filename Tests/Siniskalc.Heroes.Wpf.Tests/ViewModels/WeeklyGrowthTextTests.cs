namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;

    [TestFixture]
    internal sealed class WeeklyGrowthTextTests
    {
        [Test]
        public void AGrowthIsShownWithAPlusAndAWeek()
        {
            Assert.That(WeeklyGrowthText.Of(14), Is.EqualTo("+14 a week"));
        }

        [Test]
        public void ADwellingThatGainsNothingShowsNothing()
        {
            Assert.That(WeeklyGrowthText.Of(0), Is.Empty);
        }
    }
}
