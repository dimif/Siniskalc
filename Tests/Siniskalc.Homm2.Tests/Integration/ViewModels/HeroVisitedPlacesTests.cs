namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The places a hero has visited, listed with where they are on the map (the saves of Threedom) and forgotten one at a time.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroVisitedPlacesTests
    {
        private const int ColarBlau = 50;

        private static VisitedPlaceRowViewModel Row(HeroViewModel page, string name) => page.VisitedPlaces.Single(r => r.Name == name);

        [Test]
        public void ThePlacesOfTheMapAreFoundInTheOrderOfTheMapWithTheirPositions()
        {
            var places = TestServices.LoadThreedom(71).Places;

            Assert.That(places.Of(PlaceKind.Fort), Is.EqualTo(new[] { new MapPosition(1, 4), new MapPosition(29, 18), new MapPosition(64, 36), new MapPosition(51, 46), new MapPosition(9, 54) }));
            Assert.That(places.Of(PlaceKind.WitchDoctorHut), Has.Count.EqualTo(6));
            Assert.That(places.Of(PlaceKind.MercenaryCamp), Has.Count.EqualTo(4));
            Assert.That(places.Of(PlaceKind.Gazebo), Has.Count.EqualTo(5));
            Assert.That(places.Of(PlaceKind.StandingStones), Has.Count.EqualTo(7));
            Assert.That(places.Of(PlaceKind.StandingStones)[0], Is.EqualTo(new MapPosition(5, 0)));
        }

        [Test]
        public void TheFirstPlaceOfAKindIsTheSecondBitAndTheVisitedOnesOfAHeroAreTheOnesTheSaveSays()
        {
            var game = TestServices.LoadThreedom(71);
            var page = new HeroViewModel(game, ColarBlau, _ => { }, TestServices.SpellPoints);

            var forts = Row(page, "Forts");
            Assert.That(forts.Text, Is.EqualTo("2 of 5"));
            Assert.That(forts.Places.Select(p => (p.Name, p.IsChecked)), Is.EqualTo(new[]
            {
                ("At X: 1, Y: 4", true),
                ("At X: 29, Y: 18", true),
                ("At X: 64, Y: 36", false),
                ("At X: 51, Y: 46", false),
                ("At X: 9, Y: 54", false),
            }));
            Assert.That(Row(page, "Standing stones").Places.Select(p => p.IsChecked), Is.EqualTo(new[] { true, false, false, false, false, false, false }), "Colar Blau visited the one at 5, 0");
        }

        [Test]
        public void APlaceIsForgottenAndRememberedOnItsOwn()
        {
            var game = TestServices.LoadThreedom(71);
            var page = new HeroViewModel(game, ColarBlau, _ => { }, TestServices.SpellPoints);
            var forts = Row(page, "Forts");

            forts.Places[1].IsChecked = false;
            Assert.That(game.History.Current.Heroes().GetModel(ColarBlau).Visited.Forts, Is.EqualTo(2u));
            Assert.That(forts.Text, Is.EqualTo("1 of 5"));

            forts.Places[4].IsChecked = true;
            Assert.That(game.History.Current.Heroes().GetModel(ColarBlau).Visited.Forts, Is.EqualTo(2u | (1u << 5)));

            forts.ForgetCommand.Execute(null);
            Assert.That(game.History.Current.Heroes().GetModel(ColarBlau).Visited.Forts, Is.EqualTo(0u));
        }

        [Test]
        public void XanaduIsAPlaceOfTheMapAndTheFirstBitOfItsSetMarksCharityWhoVisitedIt()
        {
            var game = TestServices.LoadThreedom(82);

            Assert.That(game.Places.Of(PlaceKind.Xanadu), Is.EqualTo(new[] { new MapPosition(37, 49) }), "found although Charity stands on it");
            Assert.That(game.History.Current.Heroes().GetModel(49).Visited.Xanadu, Is.EqualTo(2u));
            Assert.That(TestServices.LoadThreedom(81).History.Current.Heroes().GetModel(49).Visited.Xanadu, Is.EqualTo(0u));
            Assert.That(Row(new HeroViewModel(game, 49, _ => { }, TestServices.SpellPoints), "Xanadu").Text, Is.EqualTo("1 of 1"));
        }

        [Test]
        public void AnObeliskAHeroStandsOnIsStillFound()
        {
            // Colar Blau stands on the obelisk at 1, 62 in save 80.
            Assert.That(TestServices.LoadThreedom(80).History.Current.Obelisks().GetModel()!.Count, Is.EqualTo(9));
        }
    }
}
