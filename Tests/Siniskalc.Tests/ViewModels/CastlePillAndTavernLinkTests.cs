using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.ViewModels
{
    /// <summary>
    /// The castles of a player as they are listed on the overview and the page of a player (the grail mark, the heroes in the castle as links),
    /// and the heroes of the tavern as links.
    /// </summary>
    [TestFixture]
    internal sealed class CastlePillAndTavernLinkTests
    {
        private static Game Load(string folder, string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", folder, save + ".GM1"));

        private static PlayerViewModel Red(Game game, List<int> opened, List<int>? castlesOpened = null)
        {
            var session = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Red));
            return new PlayerViewModel(session, TestServices.ByteEditTargets, openHero: opened.Add, openCastle: (castlesOpened ?? []).Add);
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
            var overview = new OverviewViewModel(game, opened.Add, _ => { });

            var red = overview.Players.Single(p => p.Name == "Red");

            Assert.That(red.Castles.Single(c => c.Name == "4_Inferno").Heroes.Select(h => h.Name), Is.EqualTo(new[] { "Nymus", "Tazar" }));
        }

        [Test]
        public void ACastleWithTheGrailStructureIsMarkedAndItsStructureIsNamed()
        {
            var game = Load("Sod", "conflux_34_grail");
            using var red = Red(game, []);

            var conflux = red.Castles.Single(c => c.Name == "Conflux");
            var other = red.Castles.Single(c => c.Name == "5_necropolis");

            Assert.That(conflux.HasGrail, Is.True);
            Assert.That(conflux.GrailText, Is.EqualTo("Aurora Borealis"));
            Assert.That(conflux.Heroes.Select(h => (h.Role, h.Name)), Is.EqualTo(new[] { ("Visiting", "Tazar") }));
            Assert.That(other.HasGrail, Is.False);
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

        [Test]
        public void TheCastlesOfTheNavigationTreeShowTheCapitolAndTheGrailAndFollowEdits()
        {
            var main = TestServices.CreateMainViewModel();
            var path = Path.Combine("TestData", "Sod", "conflux_34_grail.GM1");
            main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var nodes = main.NavigationNodes.Descendants().Where(n => n.Kind == NavNodeKind.Castle).ToList();

            var conflux = nodes.Single(n => n.Title == "Conflux");
            var necropolis = nodes.Single(n => n.Title == "5_necropolis");
            Assert.That((conflux.IsCapital, conflux.HasGrail, conflux.GrailText), Is.EqualTo((true, true, "Aurora Borealis")));
            Assert.That((necropolis.IsCapital, necropolis.HasGrail), Is.EqualTo((false, false)));

            main.CurrentGame!.History.CreateCastleSession(conflux.CastleIndex).Update(c => CastleBuilder.Unbuild(c, Building.Capitol));

            Assert.That(main.NavigationNodes.Descendants().Single(n => n.Title == "Conflux").IsCapital, Is.False);
        }
    }
}
