using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Homm3.Tests.ViewModels;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// Tests the pills of the castles: the Grail structure and the Capitol, on the page and in the tree.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleGrailAndCapitolPillTests
    {
        private static Game Load(string folder, string save) => SavedGames.Get(Path.Combine("TestData", folder, save + ".GM1"));

        private static PlayerViewModel Red(Game game, List<int> opened, List<int>? castlesOpened = null)
        {
            var session = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Red));
            return new PlayerViewModel(session, TestServices.ByteEditTargets, TestServices.BanishingText, openHero: opened.Add, openCastle: (castlesOpened ?? []).Add);
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
        public void TheCastlesOfTheNavigationTreeShowTheCapitolAndTheGrailAndFollowEdits()
        {
            var main = TestServices.CreateHomm3Editor();
            var path = Path.Combine("TestData", "Sod", "conflux_34_grail.GM1");
            main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var nodes = main.NavigationNodes.Descendants().Where(n => n.Kind == NavNodeKind.Castle).ToList();

            var conflux = nodes.Single(n => n.Title == "Conflux");
            var necropolis = nodes.Single(n => n.Title == "5_necropolis");
            Assert.That((conflux.HasBadge(NavigationBuilder.CapitalBadge), conflux.HasBadge(NavigationBuilder.GrailBadge), conflux.BadgeTooltip(NavigationBuilder.GrailBadge)), Is.EqualTo((true, true, "Grail: Aurora Borealis")));
            Assert.That((necropolis.HasBadge(NavigationBuilder.CapitalBadge), necropolis.HasBadge(NavigationBuilder.GrailBadge)), Is.EqualTo((false, false)));

            main.CurrentGame!.History.CreateCastleSession(conflux.Index).Update(c => CastleBuilder.Unbuild(c, Building.FromType(c.Faction, BuildingType.Capitol)!));

            Assert.That(main.NavigationNodes.Descendants().Single(n => n.Title == "Conflux").HasBadge(NavigationBuilder.CapitalBadge), Is.False);
        }
    }
}
