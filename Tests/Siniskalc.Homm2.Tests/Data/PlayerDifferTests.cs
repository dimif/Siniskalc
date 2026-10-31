namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the differ of a player lists for two versions of the record: the stock, the days left, the tavern, the lists of heroes and
    /// castles and what is selected, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerDifferTests
    {
        private PendingChangesFake fake = null!;

        [SetUp]
        public void StartWithNoReverts() => this.fake = new();

        private IReadOnlyList<PendingChange> Diff(PlayerModel original, PlayerModel current) =>
            new PlayerDiffer(this.fake.Factory).Diff(0, original, current, id => $"Hero {id}");

        [Test]
        public void TwoEqualPlayersHaveNoChanges()
        {
            Assert.That(this.Diff(DifferSamples.Player(), DifferSamples.Player()), Is.Empty);
        }

        [Test]
        public void TheSelectedHeroAndCastleAreListedWithTheirNumbersAndPutBack()
        {
            var original = DifferSamples.Player();
            var current = original with { SelectedHero = 9, SelectedCastle = null };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[]
            {
                ("Selected hero", ChangeText.Number(7), ChangeText.Number(9)),
                ("Selected castle", ChangeText.Number(3), ChangeText.Number(null)),
            }));
            Assert.That(this.fake.PlayerReverts["Selected hero"](current).SelectedHero, Is.EqualTo(7));
            Assert.That(this.fake.PlayerReverts["Selected castle"](current).SelectedCastle, Is.EqualTo(3));
        }

        [Test]
        public void TheTavernHeroesAreWordedWithTheNamesTheCallerGives()
        {
            var original = DifferSamples.Player();
            var current = original with { Tavern = new TavernHeroes(12, 20) };

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Description, Does.Contain("Tavern"));
            Assert.That((change.Change.Before, change.Change.After), Is.EqualTo(("Hero 13", "Hero 20")));
        }

        [Test]
        public void TheListsOfHeroesAndCastlesAreListedAfterTheStockAndBeforeWhatIsSelected()
        {
            var original = DifferSamples.Player();
            var current = original with
            {
                Resources = original.Resources.With(Resource.Gold, 1),
                HeroIds = Numbers.Of(7),
                CastleIds = new Numbers([]),
                SelectedHero = null,
            };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => c.Change.Description), Is.EqualTo(new[] { "Gold", "Heroes", "Castles", "Selected hero" }));
            Assert.That((changes[1].Change.Before, changes[1].Change.After), Is.EqualTo(("7, 9", "7")));
            Assert.That((changes[2].Change.Before, changes[2].Change.After), Is.EqualTo(("3", "none")));
            Assert.That(changes.Select(c => c.OwnerName).Distinct(), Is.EqualTo(new[] { "Blue" }));
        }

        [Test]
        public void TheDaysLeftToOwnACastleAreAChangeOfThePlayer()
        {
            var original = DifferSamples.Player();
            var current = original with { DaysLeft = PlayerModel.DaysToOwnACastle };

            var change = this.Diff(original, current).Single();

            Assert.That(this.fake.PlayerReverts[change.Change.Description](current).DaysLeft, Is.Null);
        }
    }
}
