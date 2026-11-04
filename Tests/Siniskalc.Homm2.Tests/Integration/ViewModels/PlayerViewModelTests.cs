namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the towns of a player, as the player page and the overview card of HoMM2 list them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerViewModelTests
    {
        [Test]
        public void EachTownOfAPlayerSaysWhatItIsItsMageGuildAndHowManyDwellingsStandInIt()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var player = new PlayerViewModel(game, PlayerColor.Blue, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { }, TestServices.ByteEditTargets);

            Assert.That(player.Castles, Has.Count.EqualTo(3), "the three castles of Blue in save 24");
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

            var player = new PlayerViewModel(game, owner, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { }, TestServices.ByteEditTargets);

            var link = player.Castles.First(c => c.Heroes.Count > 0);
            Assert.That(link.Heroes.Single().Role, Is.EqualTo("Visiting"));
        }

        [Test]
        public void ThePlayerPageSaysWhoPlaysTheColourAndWhetherItIsHisTurn()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var player = new PlayerViewModel(game, PlayerColor.Blue, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { }, TestServices.ByteEditTargets);

            Assert.That(game.CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
            Assert.That(player.ControlText, Is.EqualTo("Human · current turn"));
            Assert.That(new PlayerViewModel(game, PlayerColor.Red, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { }, TestServices.ByteEditTargets).ControlText, Is.EqualTo("Computer"));
        }

        [Test]
        public void APlayerWhoOwnsNoTownCanBeGivenOtherDaysLeftOnItsPageAndTheChangeIsPending()
        {
            var game = TestServices.LoadBrokenAlliance(21);
            var player = new PlayerViewModel(game, PlayerColor.Blue, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { }, TestServices.ByteEditTargets);
            Assert.That((player.DaysLeftCard.HasBanishing, player.DaysLeftCard.DaysLeft), Is.EqualTo((true, (int?)7)), "Blue lost his last castle in save 21");

            player.DaysLeftCard.DayChoices.Single(c => c.Days == 3).SelectCommand.Execute(null);

            Assert.That(game.History.Current.Players().GetModel(PlayerColor.Blue.Code).DaysLeft, Is.EqualTo(3));
            Assert.That(player.DaysLeftCard.DayChoices.Single(c => c.IsCurrent).Days, Is.EqualTo(3));
            Assert.That(player.BanishingText, Is.EqualTo("No towns: 3 days left"));
            Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Does.Contain("Days left without a castle"));
        }

        [Test]
        public void APlayerWhoOwnsATownHasNoBanishingCardAndABanishedOneCannotBeGivenDays()
        {
            Assert.That(new PlayerViewModel(TestServices.LoadBrokenAlliance(24), PlayerColor.Blue, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { }, TestServices.ByteEditTargets).DaysLeftCard.HasBanishing, Is.False);

            var green = new PlayerViewModel(TestServices.LoadBrokenAlliance(22), PlayerColor.Green, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { }, TestServices.ByteEditTargets);

            Assert.That(green.DaysLeftCard.IsBanished, Is.True);
            Assert.That(green.DaysLeftCard.DayChoices.Select(c => c.SelectCommand.CanExecute(null)), Has.All.False);
        }
    }
}
