namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the players of Broken Alliance hold, as written in the README of the saves (TestData/Homm2/BrokenAlliance).
    /// </summary>
    [TestFixture]
    internal sealed class PlayerTests
    {
        private const int Blue = 0;
        private const int Green = 1;

        private static PlayerModel Player(int save, int player) => TestServices.LoadBrokenAlliance(save).History.Current.Players().GetModel(player);

        [Test]
        public void BlueStartsWithTheResourcesOfTheMapAndTwoHeroesToHire()
        {
            var blue = Player(1, Blue);

            Assert.That(blue.Color, Is.EqualTo(PlayerColor.Blue));
            Assert.That(blue.Resources, Is.EqualTo(new ResourceSet(20, 5, 20, 5, 5, 5, 7500)));
            Assert.That(blue.HeroIds, Is.EqualTo(new[] { 7 }));
            Assert.That(blue.SelectedHero, Is.EqualTo(7));
            Assert.That(blue.CastleIds, Is.EqualTo(new[] { 3 }));
            Assert.That(blue.DaysLeft, Is.Null);
            Assert.That(blue.Tavern, Is.EqualTo(new TavernHeroes(1, 51)));
        }

        [Test]
        public void TheGoldFollowsTheTreasuresAndTheArcheryRange()
        {
            Assert.That(Player(4, Blue).Resources.Gold, Is.EqualTo(8700));
            Assert.That(Player(5, Blue).Resources.Gold, Is.EqualTo(7700));
        }

        [Test]
        public void ANewDayPaysTheIncomeAndTheMageGuild()
        {
            var day2 = Player(6, Blue);

            Assert.That(day2.Resources.Gold, Is.EqualTo(6700));
            Assert.That(day2.Resources.Wood, Is.EqualTo(31));
            Assert.That(day2.Resources.Ore, Is.EqualTo(15));
        }

        [Test]
        public void TheSawmillMakesTwoWoodADayFromTheDayAfterItIsTaken()
        {
            Assert.That(Player(6, Blue).Income.Wood, Is.Zero);
            Assert.That(Player(7, Blue).Income.Wood, Is.EqualTo(2));
            Assert.That(Player(7, Blue).Resources.Wood, Is.EqualTo(33));
        }

        [Test]
        public void HiringAHeroTakesHisPriceAndPutsHimInTheListAndTheTavernOffersAnother()
        {
            var before = Player(7, Blue);
            var after = Player(8, Blue);

            Assert.That(after.HeroIds, Is.EqualTo(new[] { 7, 1 }));
            Assert.That(after.Resources.Gold, Is.EqualTo(before.Resources.Gold - 2500));
            Assert.That(after.Tavern.First, Is.Not.EqualTo(before.Tavern.First));
            Assert.That(after.Tavern.Second, Is.EqualTo(before.Tavern.Second));
        }

        [Test]
        public void CapturingACastleAddsItToTheList()
        {
            Assert.That(Player(13, Blue).CastleIds, Is.EqualTo(new[] { 3 }));
            Assert.That(Player(14, Blue).CastleIds, Is.EqualTo(new[] { 3, 2 }));
            Assert.That(Player(18, Blue).CastleIds, Is.EqualTo(new[] { 3 }));
            Assert.That(Player(19, Blue).CastleIds, Is.EqualTo(new[] { 3, 1 }));
        }

        [Test]
        public void ALostLastCastleStartsASevenDayCountdownThatEndsWithTheLossOfThePlayer()
        {
            Assert.That(Player(20, Blue).DaysLeft, Is.Null);
            Assert.That(Player(21, Blue).DaysLeft, Is.EqualTo(7));
            Assert.That(Player(22, Blue).DaysLeft, Is.EqualTo(4));
            Assert.That(Player(23, Blue).DaysLeft, Is.EqualTo(3));
            Assert.That(Player(24, Blue).DaysLeft, Is.Null);
            Assert.That(Player(24, Blue).CastleIds, Is.EqualTo(new[] { 2, 3, 1 }));

            var green = Player(22, Green);
            Assert.That(green.DaysLeft, Is.Zero);
            Assert.That((green.IsBanished, green.CastleIds.Count, green.HeroIds.Count), Is.EqualTo((true, 0, 0)), "a banished player has no castle and no hero left");
            Assert.That(Player(21, Green).IsBanished, Is.False);
        }

        [Test]
        public void TheIncomeTableHoldsTheGoldAndTheResourcesOfTheMines()
        {
            var income = Player(16, Blue).Income;

            Assert.That(income.Wood, Is.EqualTo(4));
            Assert.That(income.Mercury, Is.EqualTo(1));
            Assert.That(income.Ore, Is.EqualTo(2));
            Assert.That(income.Crystal, Is.EqualTo(1));
            Assert.That(income.Gold, Is.EqualTo(2600));
        }
    }
}
