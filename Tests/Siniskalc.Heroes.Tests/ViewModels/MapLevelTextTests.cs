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
                Is.EqualTo(("X: 59, Y: 9, Underground (1)", "X: 59, Y: 9")));
        }
    }
}
