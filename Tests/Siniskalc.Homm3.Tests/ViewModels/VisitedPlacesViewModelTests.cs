using Siniskalc.Heroes.Models;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// Tests the card of the places a hero has visited: a row for each kind that the game has, with a box for each place of the map in its
    /// bit of the mask the hero keeps, and the buttons that tick or untick them all.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class VisitedPlacesViewModelTests
    {
        private HeroModel hero = null!;

        [SetUp]
        public void StartWithAHeroWhoVisitedNothing()
        {
            this.hero = HeroModels.Christian();
        }

        private static MapPlaces Places() => new(new Dictionary<VisitedPlaceKind, IReadOnlyList<(int Number, MapPosition Position)>>
        {
            [TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone")] = [(1, new MapPosition(5, 6)), (3, new MapPosition(7, 8, 1))],
        });

        private static VisitedPlaceRowViewModel Stones(VisitedPlacesViewModel page) => page.PlacesVisited.Kinds.Single(k => k.Name == TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone").Name);

        private VisitedPlacesViewModel Page(GameVersion version = GameVersion.ShadowOfDeath, MapPlaces? places = null) =>
            new(() => this.hero, edit => this.hero = edit(this.hero), places ?? Places(), version);

        [Test]
        public void TheCardHasARowForEachKindOfTheGameAndTheKindsOfHornOfTheAbyssOnlyThere()
        {
            var shadow = this.Page(GameVersion.ShadowOfDeath).PlacesVisited;
            var hota = this.Page(GameVersion.HornOfTheAbyss).PlacesVisited;

            Assert.That(shadow.Kinds.Select(k => k.Name), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.All.Where(k => !k.IsHotaOnly).Select(k => k.Name)));
            Assert.That(hota.Kinds, Has.Count.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.All.Count));
            Assert.That(hota.Kinds.Count, Is.GreaterThan(shadow.Kinds.Count));
            Assert.That(shadow.Title, Is.EqualTo("Visited once per hero"));
            Assert.That(shadow.Others, Is.Empty);
        }

        [Test]
        public void ARowHasABoxForEachPlaceOfTheMapLabelledWithItsTileAndTheToolTipOfItsKind()
        {
            var row = Stones(this.Page());

            Assert.That(row.Places.Select(p => p.Name), Is.EqualTo(new[] { "At 5, 6", "At 7, 8" }));
            Assert.That(row.ToolTip, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone").ToolTip));
            Assert.That(row.HasPlaces, Is.True);
            Assert.That(Stones(this.Page(places: MapPlaces.None)).HasPlaces, Is.False);
        }

        [Test]
        public void ABoxIsTickedWhenTheBitOfThePlaceNumberIsSetInTheMaskOfTheHero()
        {
            this.hero = this.hero with { PlaceVisits = this.hero.PlaceVisits.With(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone"), 0b1000) };

            var row = Stones(this.Page());

            Assert.That(row.Places.Select(p => p.IsChecked), Is.EqualTo(new[] { false, true }));
            Assert.That(row.Text, Is.EqualTo("1 of 2"));
        }

        [Test]
        public void TickingAndUntickingABoxSetsAndClearsOnlyItsBit()
        {
            this.hero = this.hero with { PlaceVisits = this.hero.PlaceVisits.With(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone"), 0b10001) };
            var row = Stones(this.Page());

            row.Places[0].IsChecked = true;
            Assert.That(this.hero.PlaceVisits.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone")), Is.EqualTo(0b10011u));

            row.Places[0].IsChecked = false;
            Assert.That(this.hero.PlaceVisits.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone")), Is.EqualTo(0b10001u));
        }

        [Test]
        public void ForgetClearsTheMaskOfTheKindAndLeavesTheOtherKinds()
        {
            this.hero = this.hero with { PlaceVisits = this.hero.PlaceVisits.With(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone"), 0b1010).With(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("Arena"), 5) };
            var row = Stones(this.Page());

            row.ForgetCommand.Execute(null);

            Assert.That(this.hero.PlaceVisits.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone")), Is.EqualTo(0u));
            Assert.That(this.hero.PlaceVisits.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("Arena")), Is.EqualTo(5u));
        }

        [Test]
        public void RememberAddsEveryPlaceOfTheMapAndKeepsTheBitsOfOtherPlaces()
        {
            this.hero = this.hero with { PlaceVisits = this.hero.PlaceVisits.With(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone"), 0b10000) };
            var row = Stones(this.Page());

            row.RememberCommand.Execute(null);

            Assert.That(this.hero.PlaceVisits.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone")), Is.EqualTo(0b11010u));
        }

        [Test]
        public void RefreshShowsWhatTheHeroNowHas()
        {
            var page = this.Page();
            var row = Stones(page);
            this.hero = this.hero with { PlaceVisits = this.hero.PlaceVisits.With(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone"), 0b10) };

            var rowRaised = new List<string?>();
            row.PropertyChanged += (_, e) => rowRaised.Add(e.PropertyName);
            var toggleRaised = new List<string?>();
            row.Places[0].PropertyChanged += (_, e) => toggleRaised.Add(e.PropertyName);

            page.Refresh();

            Assert.That(rowRaised, Does.Contain("Text"));
            Assert.That(toggleRaised, Is.Not.Empty);
            Assert.That(row.Places[0].IsChecked, Is.True);
            Assert.That(row.Text, Is.EqualTo("1 of 2"));
        }
    }
}
