namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The destination of a hero (the marker the game puts where it is going) and the direction it faces, in the saves of MarkerFacing
    /// (TestData/Homm2/MarkerFacing): Colar Blau stands at 1, 65 in all of them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroDestinationAndFacingTests
    {
        private const int ColarBlau = 50;

        private static HeroModel Hero(int save) => TestServices.LoadMarkerFacing(save).History.Current.Heroes().GetModel(ColarBlau);

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

        [TestCase(91, FacingDirection.West)]
        [TestCase(92, FacingDirection.NorthWest)]
        [TestCase(93, FacingDirection.NorthEast)]
        public void WithoutAMarkerTheDestinationIsNoneAndTheFacingIsTheOneTheHeroTurnedTo(int save, FacingDirection facing)
        {
            var hero = Hero(save);

            Assert.That(hero.Destination, Is.EqualTo(MapPosition.None));
            Assert.That(hero.Position.FacingDirection, Is.EqualTo(facing));
        }

        [Test]
        public void EveryHeroOfEverySaveFacesOneOfEightDirections()
        {
            foreach (var path in Saves.All())
            {
                foreach (var chunk in TestServices.Load(path).History.Current.Heroes())
                {
                    Assert.That(chunk.GetByteAt(HeroLayout.Facing), Is.LessThanOrEqualTo(7), $"{Path.GetFileName(path)}: {chunk.Name}");
                }
            }
        }

        [Test]
        public void TheCardsOfThePageEditTheDestinationAndTheFacingAndWriteOnlyThoseBytes()
        {
            var game = TestServices.LoadMarkerFacing(91);
            var page = new HeroViewModel(game, ColarBlau, _ => { }, TestServices.SpellPoints);

            Assert.That(page.Position.Facing, Is.EqualTo(FacingDirection.West));
            Assert.That(page.Destination.RemoveCommand.CanExecute(null), Is.False, "there is none to take away");

            page.Position.Facing = FacingDirection.South;
            page.Destination.X = 3;
            page.Destination.Y = 4;
            var bytes = game.GetBytesToSave();
            var changed = Enumerable.Range(0, bytes.Length).Where(i => bytes[i] != game.Bytes[i]).ToList();
            var start = game.Layout.HeroStart(ColarBlau);
            Assert.That(changed, Is.All.Matches<int>(i => (i >= start + HeroLayout.Destination && i < start + HeroLayout.Destination + 8) || i == start + HeroLayout.Facing), "nothing else of the record changes");
            Assert.That(changed, Does.Contain(start + HeroLayout.Facing));
            Assert.That(Hero(bytes).Destination, Is.EqualTo(new MapPosition(3, 4)));
            Assert.That(Hero(bytes).Position.FacingDirection, Is.EqualTo(FacingDirection.South));

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

        private static HeroModel Hero(byte[] bytes) => TestServices.Parser.Parse(bytes, null).History.Current.Heroes().GetModel(ColarBlau);
    }
}
