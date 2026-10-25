namespace Siniskalc.Heroes.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// Tests the words for a place on the map.
    /// </summary>
    [TestFixture]
    internal sealed class MapLevelTextTests
    {
        [Test]
        public void APlaceSaysItsLevelOnlyInAGameWhoseMapHasTwo()
        {
            var below = new MapPosition(59, 9, MapPosition.Underground);

            Assert.That(
                (MapLevelText.Coordinates(below, hasLevels: true), MapLevelText.Coordinates(below, hasLevels: false)),
                Is.EqualTo(("Pos: 59, 9, Underground", "Pos: 59, 9")));
        }

        [Test]
        public void APlaceInAListOfOneKindOfThingIsShortAndSaysTheLevelOnlyWhenTheMapHasTwo()
        {
            var surface = new MapPosition(12, 7, MapPosition.Above);
            var below = new MapPosition(59, 9, MapPosition.Underground);

            Assert.That(
                (MapLevelText.At(surface, hasLevels: true), MapLevelText.At(below, hasLevels: true), MapLevelText.At(below, hasLevels: false)),
                Is.EqualTo(("At 12, 7, Surface", "At 59, 9, Underground", "At 59, 9")));
        }
    }
}
