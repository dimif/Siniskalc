namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Data;
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
        public void TheTavernHeroesAreWordedWithTheNamesTheCallerGives()
        {
            var original = DifferSamples.Player();
            var current = original with { Tavern = new TavernHeroes(12, 20) };

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Description, Does.Contain("Tavern"));
            Assert.That((change.Change.Before, change.Change.After), Is.EqualTo(("Hero 13", "Hero 20")));
        }

        [Test]
        public void TheStockIsListedBeforeTheDaysLeftAndTheTavernAndEveryChangeIsOfThePlayer()
        {
            var original = DifferSamples.Player();
            var current = original with
            {
                Resources = original.Resources.With(Resource.Gold, 1),
                DaysLeft = 3,
                Tavern = new TavernHeroes(12, 20),
            };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => c.Change.Description).First(), Is.EqualTo("Gold"));
            Assert.That(changes.Count, Is.EqualTo(3));
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
