namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Tests how the lists of heroes and castles of a player follow a hero or a castle that changes hands, on lists made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerListsTests
    {
        private static readonly PlayerLists Loaded = new(Numbers.Of(7, 1, 4), 1, Numbers.Of(3, 2), 2);

        [Test]
        public void AHeroThatTheListNeverHadGoesAfterTheOthersAndTheSelectionStays()
        {
            var lists = Loaded.WithHero(9, owns: true, Loaded);

            Assert.That((lists.HeroIds, lists.SelectedHero), Is.EqualTo((Numbers.Of(7, 1, 4, 9), (int?)1)));
        }

        [Test]
        public void AHeroThatLeavesTakesTheSelectionWithItAndTheOthersMoveUp()
        {
            var lists = Loaded.WithHero(1, owns: false, Loaded);

            Assert.That((lists.HeroIds, lists.SelectedHero), Is.EqualTo((Numbers.Of(7, 4), (int?)null)));
        }

        [Test]
        public void AHeroThatComesBackTakesItsOldPlaceAndTheSelectionComesBackOnceTheListIsAsLoaded()
        {
            var lists = Loaded.WithHero(1, owns: false, Loaded).WithHero(1, owns: true, Loaded);

            Assert.That(lists, Is.EqualTo(Loaded));
        }

        [Test]
        public void ListsThatAlreadySayWhetherThePlayerHasTheHeroAreReturnedAsTheyAre()
        {
            Assert.That(Loaded.WithHero(7, owns: true, Loaded), Is.SameAs(Loaded));
            Assert.That(Loaded.WithHero(9, owns: false, Loaded), Is.SameAs(Loaded));
        }

        [Test]
        public void AHeroJoiningAFullListOfHeroesMakesItLongerForTheCallerToRefuse()
        {
            var full = Loaded with { HeroIds = new Numbers(Enumerable.Range(20, Siniskalc.Homm2.Constants.Player.HeroListSize)) };

            Assert.That(full.WithHero(9, owns: true, Loaded).HeroIds.Count, Is.EqualTo(Siniskalc.Homm2.Constants.Player.HeroListSize + 1));
        }

        [Test]
        public void ACastleThatLeavesAndComesBackTakesItsPlaceAndTheSelectionBack()
        {
            var gone = Loaded.WithCastle(2, owns: false, Loaded);
            Assert.That((gone.CastleIds, gone.SelectedCastle), Is.EqualTo((Numbers.Of(3), (int?)null)));

            Assert.That(gone.WithCastle(2, owns: true, Loaded), Is.EqualTo(Loaded));
        }

        [Test]
        public void ACastleJoiningAFullListOfCastlesMakesItLongerForTheCallerToRefuse()
        {
            var full = Loaded with { CastleIds = new Numbers(Enumerable.Range(40, Siniskalc.Homm2.Constants.Player.CastleListSize)) };

            Assert.That(full.WithCastle(5, owns: true, Loaded).CastleIds.Count, Is.EqualTo(Siniskalc.Homm2.Constants.Player.CastleListSize + 1));
        }
    }
}
