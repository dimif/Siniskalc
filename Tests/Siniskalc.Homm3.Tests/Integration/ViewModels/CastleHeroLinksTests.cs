using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// Tests the links from the page of a castle, the overview and the tavern to the heroes that are in them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleHeroLinksTests
    {
        private static Game Load(string folder, string save) => SavedGames.Get(Path.Combine("TestData", folder, save + ".GM1"));

        private static PlayerViewModel Red(Game game, List<int> opened, List<int>? castlesOpened = null)
        {
            var session = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Red));
            return new PlayerViewModel(session, TestServices.ByteEditTargets, TestServices.BanishingText, game, openHero: opened.Add, openCastle: (castlesOpened ?? []).Add);
        }

        [Test]
        public void TheCastleOfAPlayerListsItsGarrisonedAndItsVisitingHeroAsLinks()
        {
            var game = Load("Roe", "roe_61_inferno_garrison_and_visiting");
            var opened = new List<int>();
            using var red = Red(game, opened);

            var inferno = red.Castles.Single(c => c.Name == "4_Inferno");

            Assert.That(inferno.Heroes.Select(h => (h.Role, h.Name)), Is.EqualTo(new[] { ("Garrisoned", "Nymus"), ("Visiting", "Tazar") }));
            Assert.That(red.Castles.Where(c => c.Name != "4_Inferno").SelectMany(c => c.Heroes.Select(h => (c.Name, h.Role, h.Name))), Is.EqualTo(new[] { ("7_Strong", "Visiting", "Crag Hack") }));

            inferno.Heroes[1].OpenCommand.Execute(null);

            Assert.That(opened, Is.EqualTo(new[] { game.History.Current.Heroes().FindByName("Tazar") }));
        }

        [Test]
        public void TheOverviewListsTheHeroesOfACastleToo()
        {
            var game = Load("Roe", "roe_61_inferno_garrison_and_visiting");
            var opened = new List<int>();
            var overview = new OverviewViewModel(game, TestServices.BanishingText, opened.Add, _ => { });

            var red = overview.Players.Single(p => p.Name == "Red");

            Assert.That(red.Castles.Single(c => c.Name == "4_Inferno").Heroes.Select(h => h.Name), Is.EqualTo(new[] { "Nymus", "Tazar" }));
        }

        [Test]
        public void TheHeroesOfTheTavernOpenTheirPage()
        {
            var game = Load("Roe", "roe_67_damacon_defeated");
            var opened = new List<int>();
            using var red = Red(game, opened);

            Assert.That(red.TavernPlaces[1].OpenCommand.CanExecute(null), Is.True);
            red.TavernPlaces[1].OpenCommand.Execute(null);
            Assert.That(opened, Is.EqualTo(new[] { game.History.Current.Heroes().FindByName("Solmyr") }));

            red.TavernPlaces[1].Selected = red.TavernPlaces[1].Choices[0];

            Assert.That(red.TavernPlaces[1].OpenCommand.CanExecute(null), Is.False, "an empty place has nobody to open");
        }
    }
}
