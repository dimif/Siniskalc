namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// How many creatures a dwelling gains each week, as the saves of TestData/Homm2/SecondWell and BrokenAlliance show it.
    /// </summary>
    [TestFixture]
    internal sealed class CastleWeeklyGrowthTests
    {
        private const int Xabran = 3;

        private static CastleModel Castle(string name) => TestServices.LoadNamed("SecondWell", name).History.Current.Castles().GetModel(Xabran);

        [Test]
        public void TheWellAddsTwoToEveryBuiltDwellingAndTheFarmEightToTheFirstOnly()
        {
            var xabran = Castle("second_well_01_well__farm");

            Assert.That(xabran.WeeklyGrowth(1), Is.EqualTo(12 + 2 + 8));
            Assert.That(xabran.WeeklyGrowth(2), Is.EqualTo(8 + 2));
        }

        [Test]
        public void AWeekWithTheWellAndTheFarmAddsWhatTheCastleSaysItAdds()
        {
            var before = Castle("second_well_01_well__farm");
            var after = Castle("second_well_02_next_week");

            Assert.That(after.CreaturesWaiting(1), Is.EqualTo(before.CreaturesWaiting(1) + before.WeeklyGrowth(1)));
            Assert.That(after.CreaturesWaiting(2), Is.EqualTo(before.CreaturesWaiting(2) + before.WeeklyGrowth(2)));
        }

        [Test]
        public void ADwellingThatIsNotBuiltGainsNothingEvenWithTheWell()
        {
            var xabran = Castle("second_well_02_next_week");

            Assert.That(xabran.Town.HasFlag(TownBuildings.Well), Is.True);
            Assert.That(Enumerable.Range(3, 4).Select(xabran.WeeklyGrowth), Is.All.Zero);
        }

        [Test]
        public void WithoutTheWellACastleGainsTheGrowthOfItsCreature()
        {
            var xabran = TestServices.LoadBrokenAlliance(1).History.Current.Castles().GetModel(Xabran);

            Assert.That(xabran.WeeklyGrowth(1), Is.EqualTo(12));
        }
    }
}
