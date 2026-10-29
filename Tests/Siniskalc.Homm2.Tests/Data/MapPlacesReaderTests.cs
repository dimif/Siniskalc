namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// How the places a hero can visit are found among the tiles of the map, on a savegame made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapPlacesReaderTests
    {
        private readonly MapPlacesReader reader = new();

        [Test]
        public void ASavegameWithNoTableOfTilesHasNoPlaces()
        {
            var save = new MapSavegame(6, withTiles: false);

            var places = this.reader.Read(save.Bytes, save.Layout);

            Assert.That(new[] { PlaceKind.Gazebo, PlaceKind.Fort, PlaceKind.WitchDoctorHut, PlaceKind.MercenaryCamp, PlaceKind.StandingStones, PlaceKind.Xanadu }.Select(k => places.Of(k)), Has.All.Empty);
        }

        [Test]
        public void EachKindOfPlaceIsListedAtTheTilesWhereItsObjectIs()
        {
            var save = new MapSavegame(6);
            save.Object(1, 0, 0x8A);
            save.Object(4, 3, 0x8A);
            save.Object(2, 2, 0xBE);
            save.Object(5, 5, 0xC5);
            save.Object(0, 4, 0xC9);
            save.Object(3, 1, 0xD6);

            var places = this.reader.Read(save.Bytes, save.Layout);

            Assert.That(places.Of(PlaceKind.Gazebo), Is.EqualTo(new[] { new MapPosition(1, 0), new MapPosition(4, 3) }));
            Assert.That(places.Of(PlaceKind.Fort), Is.EqualTo(new[] { new MapPosition(2, 2) }));
            Assert.That(places.Of(PlaceKind.WitchDoctorHut), Is.EqualTo(new[] { new MapPosition(5, 5) }));
            Assert.That(places.Of(PlaceKind.MercenaryCamp), Is.EqualTo(new[] { new MapPosition(0, 4) }));
            Assert.That(places.Of(PlaceKind.Xanadu), Is.EqualTo(new[] { new MapPosition(3, 1) }));
            Assert.That(places.Of(PlaceKind.StandingStones), Is.Empty);
        }

        [Test]
        public void ThePlacesAreInTheOrderOfTheTilesRowByRow()
        {
            var save = new MapSavegame(6);
            save.Object(5, 4, 0x8A);
            save.Object(0, 1, 0x8A);
            save.Object(3, 4, 0x8A);

            var places = this.reader.Read(save.Bytes, save.Layout);

            Assert.That(places.Of(PlaceKind.Gazebo), Is.EqualTo(new[] { new MapPosition(0, 1), new MapPosition(3, 4), new MapPosition(5, 4) }));
        }

        [Test]
        public void TheStandingStonesAreTwoTilesWideAndAreAPlaceAtTheLeftOne()
        {
            var save = new MapSavegame(6);
            save.Object(1, 1, 0xC2);
            save.Object(2, 1, 0xC2);
            save.Object(4, 3, 0xC2);
            save.Object(5, 3, 0xC2);
            save.Object(0, 4, 0xC2);
            save.Object(1, 4, 0xC2);

            var places = this.reader.Read(save.Bytes, save.Layout);

            Assert.That(
                places.Of(PlaceKind.StandingStones),
                Is.EqualTo(new[] { new MapPosition(1, 1), new MapPosition(4, 3), new MapPosition(0, 4) }),
                "the stone at the start of a row is a place of its own although the last tile of the row above holds the same object");
        }

        [Test]
        public void AHeroStandingOnAPlaceDoesNotHideItBecauseTheHeroKeepsTheObjectUnderIt()
        {
            var save = new MapSavegame(6);
            save.Hero(0, 2, 3, 0x8A);

            var places = this.reader.Read(save.Bytes, save.Layout);

            Assert.That(places.Of(PlaceKind.Gazebo), Is.EqualTo(new[] { new MapPosition(2, 3) }));
        }

        [Test]
        public void AHeroOutsideTheMapIsIgnored()
        {
            var save = new MapSavegame(6);
            save.Hero(0, 2, 3, 0x8A);
            BitConverter.GetBytes(99u).CopyTo(save.Bytes, save.Layout.HeroStart(0) + HeroLayout.X);

            var places = this.reader.Read(save.Bytes, save.Layout);

            Assert.That(places.Of(PlaceKind.Gazebo), Is.Empty);
        }
    }
}
