namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the towns of a player, as the player page and the overview card of HoMM2 list them.
    /// </summary>
    [TestFixture]
    internal sealed class PlayerTownsTests
    {
        [Test]
        public void EachTownOfAPlayerSaysWhatItIsItsMageGuildAndHowManyDwellingsStandInIt()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var player = new PlayerViewModel(game, PlayerColor.Blue, TestServices.BanishingText, _ => { }, _ => { });

            Assert.That(player.Castles, Is.Not.Empty);
            foreach (var castle in player.Castles)
            {
                Assert.That(castle.Status, Does.Match(@"^(Castle|Town)( · Mage Guild \d)? · \d+ dwellings?$"), castle.Name);
            }
        }

        [Test]
        public void ATownWithAHeroInItListsTheHeroAsVisiting()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var castles = game.History.Current.Castles().GetModels().Where(c => c.IsUsed && c.VisitingHeroId != null).ToList();
            Assert.That(castles, Is.Not.Empty, "the save has a hero in a town");
            var owner = castles.First(c => c.Owner != null).Owner!;

            var player = new PlayerViewModel(game, owner, TestServices.BanishingText, _ => { }, _ => { });

            var link = player.Castles.First(c => c.Heroes.Count > 0);
            Assert.That(link.Heroes.Single().Role, Is.EqualTo("Visiting"));
        }

        [Test]
        public void ThePlayerPageSaysWhoPlaysTheColourAndWhetherItIsHisTurn()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var player = new PlayerViewModel(game, PlayerColor.Blue, TestServices.BanishingText, _ => { }, _ => { });

            Assert.That(game.CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
            Assert.That(player.ControlText, Is.EqualTo("Human · current turn"));
            Assert.That(new PlayerViewModel(game, PlayerColor.Red, TestServices.BanishingText, _ => { }, _ => { }).ControlText, Is.EqualTo("Computer"));
        }
    }
}
