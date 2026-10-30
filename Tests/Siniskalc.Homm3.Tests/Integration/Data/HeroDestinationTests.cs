using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;

    /// <summary>
    /// The destination of a hero (the marker of its path) is x and y as 32 bit numbers and then the level as a 16 bit number.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroDestinationTests
    {
        private static readonly string MarkerSave = Path.Combine("TestData", "Hota", "Dwellings", "dwellings_29_marker_on_the_entrance.GM2");

        [Test]
        public void TheDestinationIsTheTileTheMarkerIsOnOneEastAndOneNorthOfTheHero()
        {
            var game = SavedGames.Get(MarkerSave);
            var heroes = game.History.Current.Heroes();
            var tazar = heroes.GetModel(heroes.FindByName("Tazar"));

            Assert.That((tazar.Position.X, tazar.Position.Y), Is.EqualTo((40, 48)));
            Assert.That(tazar.Destination, Is.EqualTo(new MapPosition(41, 47)));
        }

        [Test]
        public void ChangingTheDestinationWritesTwoWholeNumbersAndTheLevelAndNothingElse()
        {
            var game = SavedGames.Get(MarkerSave);
            var heroes = game.History.Current.Heroes();
            var index = heroes.FindByName("Tazar");
            var chunk = heroes[index];

            game.History.CreateHeroSession(index).Update(h => h with { Destination = new MapPosition(300, 7, 1) });

            var written = game.GetBytesToSave();
            var at = chunk.StartIndex + 45;
            Assert.That(new[] { BitConverter.ToInt32(written, at), BitConverter.ToInt32(written, at + 4), BitConverter.ToInt16(written, at + 8) }, Is.EqualTo(new[] { 300, 7, 1 }));
            var saved = TestServices.SavefileCodec.Read(MarkerSave);
            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != written[i]).ToList();
            Assert.That(changed.All(i => i >= at && i < at + 10), Is.True);
        }

        [Test]
        public void AHeroWithNoDestinationReadsAsMinusOneForEachNumber()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "Mines", "mines_00_game_begin.GM2"));
            var heroes = game.History.Current.Heroes();

            Assert.That(heroes.GetModel(heroes.FindByName("Tazar")).Destination, Is.EqualTo(new MapPosition(-1, -1, -1)));
        }
    }
}
