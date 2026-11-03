namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the tile of a map both games use.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapPositionTests
    {
        [Test]
        public void ATileOnTheSurfaceIsWrittenWithoutItsLevel()
        {
            Assert.That(new MapPosition(28, 25).ToString(), Is.EqualTo("28,25"));
            Assert.That(new MapPosition(28, 25, MapPosition.Underground).ToString(), Is.EqualTo("28,25,1"));
        }

        [Test]
        public void PositionsOnTheSameTileAreEqual()
        {
            Assert.That(new MapPosition(1, 2), Is.EqualTo(new MapPosition(1, 2, MapPosition.Above)));
            Assert.That(new MapPosition(1, 2), Is.Not.EqualTo(new MapPosition(1, 2, MapPosition.Underground)));
        }

        [Test]
        public void SomethingNotOnTheMapHasNoCoordinates()
        {
            Assert.That(MapPosition.None, Is.EqualTo(new MapPosition(-1, -1, -1)));
        }
    }
}
