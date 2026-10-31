using Siniskalc.Core.Data;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the differ of a player lists for two versions of the record: the resources, the tavern and the days left to own a town, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerDifferTests
    {
        private PendingChangesFake fake = null!;

        [SetUp]
        public void StartWithNoReverts() => this.fake = new();

        private static PlayerModel Blue() => new(PlayerColor.Blue, ResourceSet.None.With(Resource.Gold, 5000), new TavernHeroes(12, 13), 7);

        private IReadOnlyList<PendingChange> Diff(PlayerModel original, PlayerModel current) =>
            new PlayerDiffer(this.fake.Factory).Diff(0, original, current, id => $"Hero {id}");

        [Test]
        public void TwoEqualPlayersHaveNoChanges()
        {
            Assert.That(this.Diff(Blue(), Blue()), Is.Empty);
        }

        [Test]
        public void ChangingTheDaysLeftIsListedAndPutBack()
        {
            var current = Blue().WithDaysLeft(3);

            var change = this.Diff(Blue(), current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Days left without a castle", "7", "3")));
            Assert.That((change.OwnerName, change.Kind, change.Index), Is.EqualTo(("Blue", Homm3ChunkKinds.Player, 0)));
            Assert.That(this.fake.PlayerReverts["Days left without a castle"](current), Is.EqualTo(Blue()));
        }

        [Test]
        public void ThePlayerWhoOwnsATownIsNotCountingDownAndThatIsWordedAsSuch()
        {
            var change = this.Diff(Blue(), Blue() with { DaysLeft = null }).Single();

            Assert.That((change.Change.Before, change.Change.After), Is.EqualTo(("7", "not counting")));
        }

        [Test]
        public void TheResourcesAndTheTavernAreListedByNameAndPutBack()
        {
            var current = Blue() with { Resources = Blue().Resources.With(Resource.Gold, 6000), Tavern = new TavernHeroes(12, 40) };

            var changes = this.Diff(Blue(), current);

            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[]
            {
                ("Gold", "5,000", "6,000"),
                ("Tavern hero 2", "Hero 13", "Hero 40"),
            }));
            Assert.That(this.fake.PlayerReverts["Gold"](current).Resources[Resource.Gold], Is.EqualTo(5000));
            Assert.That(this.fake.PlayerReverts["Tavern hero 2"](current).Tavern, Is.EqualTo(Blue().Tavern));
        }

        [Test]
        public void ChangesOfTheResourcesAndOfTheDaysLeftAreListedInThatOrder()
        {
            var current = Blue().WithDaysLeft(3) with { Resources = Blue().Resources.With(Resource.Gold, 6000) };

            Assert.That(this.Diff(Blue(), current).Select(c => c.Change.Description), Is.EqualTo(new[] { "Gold", "Days left without a castle" }));
        }

        [Test]
        public void ChangingTheFirstTavernHeroIsListedByTheNameOfTheHeroAndPutBack()
        {
            var current = Blue() with { Tavern = new TavernHeroes(40, 13) };

            var change = this.Diff(Blue(), current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Tavern hero 1", "Hero 12", "Hero 40")));
            Assert.That(this.fake.PlayerReverts["Tavern hero 1"](current), Is.EqualTo(Blue()));
        }
    }
}
