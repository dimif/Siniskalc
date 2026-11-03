namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the movement points of a hero.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MovementPointsTests
    {
        [Test]
        public void MovementPointsAreAValueThatComparesByWhatItHolds()
        {
            Assert.That(new MovementPoints(500, 1000) with { Current = 1000 }, Is.EqualTo(new MovementPoints(1000, 1000)));
        }
    }
}
