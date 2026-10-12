using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// What the player page, the overview and the navigation tree show for a player who owns no town and counts down to being banished
    /// (the saves in TestData/Hota/Banishing).
    /// </summary>
    [TestFixture]
    internal sealed class BanishingViewModelTests
    {
        private static string PathOf(string save) => Directory.GetFiles(Path.Combine("TestData", "Hota", "Banishing"), save + ".GM?").Single();

        private static Homm3EditorViewModel Open(string save)
        {
            var main = TestServices.CreateHomm3Editor();
            main.LoadGameFromBytes(TestServices.SavefileCodec.Read(PathOf(save)), PathOf(save));
            return main;
        }

        private static PlayerViewModel BluePage(Homm3EditorViewModel main)
        {
            var game = main.LoadedGame();
            main.ShowPlayer(game.History.Current.Players().FindByColor(PlayerColor.Blue));
            return main.CurrentPageOf<PlayerViewModel>();
        }

        [TestCase("banishing_01_blue_7_days_left", "No towns: 7 days left", "7 days left")]
        [TestCase("banishing_06_blue_6_days_left", "No towns: 6 days left", "6 days left")]
        [TestCase("banishing_12_m1w2d2", "No towns: 1 day left", "1 day left")]
        [TestCase("banishing_13_blue_banished", "Banished", "banished")]
        [TestCase("banishing_03_blue_with_town", "", "")]
        public void ThePillTheOverviewAndTheNavigationTreeTellHowManyDaysBlueHas(string save, string pill, string nav)
        {
            var main = Open(save);

            Assert.That(BluePage(main).BanishingText, Is.EqualTo(pill));
            Assert.That(main.LoadedOverview().Players.Single(p => p.Name == "Blue").BanishingText, Is.EqualTo(pill));
            Assert.That(main.PlayerNodes().Single(n => n.Title == "Blue").BanishingText, Is.EqualTo(nav));
            Assert.That(main.PlayerNodes().Single(n => n.Title == "Red").BanishingText, Is.Empty);
        }

        [Test]
        public void TheBanishingCardIsOnlyShownWhileThePlayerOwnsNoTown()
        {
            Assert.That(BluePage(Open("banishing_03_blue_with_town")).HasBanishing, Is.False);
            Assert.That(BluePage(Open("banishing_06_blue_6_days_left")).HasBanishing, Is.True);
            Assert.That(BluePage(Open("banishing_13_blue_banished")).HasBanishing, Is.True);
        }

        [Test]
        public void SevenButtonsOfSevenDownToOneMarkTheDaysLeft()
        {
            var page = BluePage(Open("banishing_06_blue_6_days_left"));

            Assert.That(page.DayChoices.Select(c => c.Days), Is.EqualTo(new[] { 7, 6, 5, 4, 3, 2, 1 }));
            Assert.That(page.DayChoices.Where(c => c.IsCurrent).Select(c => c.Days), Is.EqualTo(new[] { 6 }));
        }

        [Test]
        public void PickingADayChangesTheDaysLeftEverywhereAndCanBeReset()
        {
            var main = Open("banishing_06_blue_6_days_left");
            var page = BluePage(main);

            page.DayChoices.Single(c => c.Days == 2).SelectCommand.Execute(null);

            Assert.That(page.DaysLeft, Is.EqualTo(2));
            Assert.That(page.BanishingText, Is.EqualTo("No towns: 2 days left"));
            Assert.That(page.DayChoices.Single(c => c.IsCurrent).Days, Is.EqualTo(2));
            Assert.That(main.PlayerNodes().Single(n => n.Title == "Blue").BanishingText, Is.EqualTo("2 days left"));

            page.ResetCommand.Execute(null);

            Assert.That(page.DaysLeft, Is.EqualTo(6));
            Assert.That(main.PlayerNodes().Single(n => n.Title == "Blue").BanishingText, Is.EqualTo("6 days left"));
        }

        [Test]
        public void ABanishedPlayerCannotBeGivenDays()
        {
            var page = BluePage(Open("banishing_13_blue_banished"));

            Assert.That(page.IsBanished, Is.True);
            Assert.That(page.DayChoices.Select(c => c.SelectCommand.CanExecute(null)), Has.All.False);
        }
    }
}
