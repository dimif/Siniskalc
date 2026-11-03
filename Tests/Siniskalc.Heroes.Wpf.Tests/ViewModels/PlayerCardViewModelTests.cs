namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the card of a player on the overview: its name and colour, the resources in words, the heroes and the castles.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerCardViewModelTests
    {
        private static string N(long number) => number.ToString("N0", CultureInfo.CurrentCulture);

        private static PlayerCardViewModel Card(ResourceSet? resources = null, IReadOnlyList<ILink>? heroes = null, IReadOnlyList<CastleLinkViewModel>? castles = null) =>
            new("Red", "#FF0000", resources, heroes ?? [], new DelegateCommand(_ => { }), "Human", castles);

        [Test]
        public void ACardShowsWhatItWasGiven()
        {
            var open = new DelegateCommand(_ => { });

            var card = new PlayerCardViewModel("Red", "#FF0000", null, [], open, "Human");

            Assert.That((card.Name, card.ColorHex, card.ControlText), Is.EqualTo(("Red", "#FF0000", "Human")));
            Assert.That(card.OpenCommand, Is.SameAs(open));
        }

        [Test]
        public void ACardWithNoResourcesHasNoResourceText()
        {
            var card = Card();

            Assert.That((card.HasResources, card.Gold, card.OtherResources), Is.EqualTo((false, string.Empty, string.Empty)));
        }

        [Test]
        public void TheGoldIsShownAloneAndTheOtherResourcesInTheOrderOfTheGameInOneLine()
        {
            var resources = ResourceSet.None.With(Resource.Gold, 12345).With(Resource.Wood, 5).With(Resource.Gems, 1000);

            var card = Card(resources);

            Assert.That(card.HasResources, Is.True);
            Assert.That(card.Gold, Is.EqualTo(N(12345)));
            Assert.That(card.OtherResources, Is.EqualTo($"{N(5)} wood · 0 mercury · 0 ore · 0 sulfur · 0 crystal · {N(1000)} gems"));
        }

        [Test]
        public void TheBanishingTextIsOnlyThereWhenItIsNotEmpty()
        {
            var plain = Card();
            var banished = new PlayerCardViewModel("Red", "#FF0000", null, [], new DelegateCommand(_ => { }), "Human", null, "Soon", true);

            Assert.That((plain.HasBanishing, plain.IsBanished), Is.EqualTo((false, false)));
            Assert.That((banished.HasBanishing, banished.BanishingText, banished.IsBanished), Is.EqualTo((true, "Soon", true)));
        }

        [Test]
        public void TheCardHasHeroesAndCastlesOnlyWhenItWasGivenSome()
        {
            var none = Card();
            var some = Card(heroes: [new LinkViewModel("Orrin", string.Empty, new DelegateCommand(_ => { }))], castles: [new CastleLinkViewModel("Haret", "Castle", new DelegateCommand(_ => { }))]);

            Assert.That((none.HasHeroes, none.HasCastles), Is.EqualTo((false, false)));
            Assert.That((some.HasHeroes, some.HasCastles), Is.EqualTo((true, true)));
        }
    }
}
