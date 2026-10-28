namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The destination of a hero (the marker the game puts where it is going) through the reader, the page and the writer, in the saves of
    /// MarkerFacing (TestData/Homm2/MarkerFacing): Colar Blau stands at 1, 65 in all of them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroDestinationTests
    {
        private const int ColarBlau = 50;

        private static HeroModel Hero(int save) => TestServices.LoadMarkerFacing(save).History.Current.Heroes().GetModel(ColarBlau);

        private static HeroModel Hero(byte[] bytes) => TestServices.Parser.Parse(bytes, null).History.Current.Heroes().GetModel(ColarBlau);

        [TestCase(83, 1, 66)]
        [TestCase(84, 2, 66)]
        [TestCase(85, 2, 65)]
        [TestCase(86, 2, 64)]
        [TestCase(87, 1, 64)]
        [TestCase(88, 0, 64)]
        [TestCase(89, 0, 65)]
        [TestCase(90, 0, 66)]
        public void TheDestinationIsTheTileTheMarkerWasPutOnAndTheHeroFacesEast(int save, int x, int y)
        {
            var hero = Hero(save);

            Assert.That((hero.Position.X, hero.Position.Y), Is.EqualTo((1, 65)));
            Assert.That(hero.Destination, Is.EqualTo(new MapPosition(x, y)));
            Assert.That(hero.Position.FacingDirection, Is.EqualTo(FacingDirection.East));
        }

        [Test]
        public void TheDestinationCardEditsTheTileAndWritesOnlyTheBytesOfTheDestination()
        {
            var game = TestServices.LoadMarkerFacing(91);
            var page = new HeroViewModel(game, ColarBlau, _ => { }, TestServices.SpellPoints);
            Assert.That(page.Destination.RemoveCommand.CanExecute(null), Is.False, "there is none to take away");

            page.Destination.X = 3;
            page.Destination.Y = 4;

            var bytes = game.GetBytesToSave();
            var changed = Enumerable.Range(0, bytes.Length).Where(i => bytes[i] != game.Bytes[i]).ToList();
            var start = game.Layout.HeroStart(ColarBlau);
            Assert.That(changed, Is.All.Matches<int>(i => i >= start + HeroLayout.Destination && i < start + HeroLayout.Destination + 8), "nothing else of the record changes");
            Assert.That(Hero(bytes).Destination, Is.EqualTo(new MapPosition(3, 4)));
        }

        [Test]
        public void RemovingTheDestinationWritesTheNoneMarkerToTheSavegame()
        {
            var game = TestServices.LoadMarkerFacing(91);
            var page = new HeroViewModel(game, ColarBlau, _ => { }, TestServices.SpellPoints);
            page.Destination.X = 3;
            page.Destination.Y = 4;

            page.Destination.RemoveCommand.Execute(null);

            Assert.That(game.History.Current.Heroes().GetModel(ColarBlau).Destination, Is.EqualTo(MapPosition.None));
            Assert.That(Hero(game.GetBytesToSave()).Destination, Is.EqualTo(MapPosition.None));
        }

        [Test]
        public void ADestinationWithOnlyOnePartSetIsKeptAsItWasTyped()
        {
            var game = TestServices.LoadMarkerFacing(91);
            var page = new HeroViewModel(game, ColarBlau, _ => { }, TestServices.SpellPoints);

            page.Destination.X = 5;

            Assert.That(Hero(game.GetBytesToSave()).Destination.X, Is.EqualTo(5));
            Assert.That(Hero(game.GetBytesToSave()).Destination.Y, Is.EqualTo(-1));
        }
    }
}
