namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the numbered places of each kind on the map.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapPlacesTests
    {
        [Test]
        public void ThePlacesOfAKindAreTheOnesTheLoaderNumberedWithTheirNumbers()
        {
            var places = new MapPlaces<string>(new Dictionary<string, IReadOnlyList<(int Number, MapPosition Position)>>
            {
                ["Fort"] = [(1, new MapPosition(2, 3)), (4, new MapPosition(7, 1))],
            });

            Assert.That(places.Of("Fort"), Is.EqualTo(new[] { (1, new MapPosition(2, 3)), (4, new MapPosition(7, 1)) }));
        }

        [Test]
        public void AKindTheMapHasNoneOfHasNoPlaces()
        {
            var places = new MapPlaces<string>(new Dictionary<string, IReadOnlyList<(int Number, MapPosition Position)>>
            {
                ["Fort"] = [(1, new MapPosition(2, 3))],
            });

            Assert.That(places.Of("Gazebo"), Is.Empty);
            Assert.That(MapPlaces<string>.None.Of("Fort"), Is.Empty);
        }
    }
}
