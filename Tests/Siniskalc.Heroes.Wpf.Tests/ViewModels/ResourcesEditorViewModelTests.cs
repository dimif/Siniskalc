namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the resources of a player as both games' pages show them: a row for each, and the buttons that add to them up to a limit.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ResourcesEditorViewModelTests
    {
        private static readonly ResourceLimits Limits = new(0, 1000, 100, 100, 1, 99, 500);

        private ResourceSet state = ResourceSet.None;

        [SetUp]
        public void StartWithNothing() => this.state = ResourceSet.None;

        private ResourcesEditorViewModel Editor(Func<Resource, string>? note = null) => new(
            [Resource.Gold, Resource.Wood, Resource.Ore],
            Limits,
            () => this.state,
            () => ResourceSet.None,
            edit => this.state = edit(this.state),
            note);

        [Test]
        public void ThereIsARowForEachResourceInTheOrderTheGameLists()
        {
            Assert.That(this.Editor().Rows.Select(r => r.Resource), Is.EqualTo(new[] { Resource.Gold, Resource.Wood, Resource.Ore }));
        }

        [Test]
        public void GoldHasLimitsOfItsOwn()
        {
            var rows = this.Editor().Rows;

            Assert.That((rows[0].Maximum, rows[0].Step, rows[1].Maximum, rows[1].Step), Is.EqualTo((1000L, 100L, 100L, 1L)));
        }

        [Test]
        public void TheAllButtonAddsToEveryResourceButGoldAndStopsAtTheLimit()
        {
            this.state = this.state.With(Resource.Wood, 50).With(Resource.Gold, 7);

            this.Editor().SetAllCommand.Execute(null);

            Assert.That((this.state[Resource.Wood], this.state[Resource.Ore], this.state[Resource.Gold]), Is.EqualTo((100L, 99L, 7L)));
        }

        [Test]
        public void TheRichButtonAddsToGoldAloneAndStopsAtTheLimit()
        {
            this.state = this.state.With(Resource.Gold, 700).With(Resource.Wood, 3);

            this.Editor().SetRichCommand.Execute(null);

            Assert.That((this.state[Resource.Gold], this.state[Resource.Wood]), Is.EqualTo((1000L, 3L)));
        }

        [Test]
        public void ARowChangesTheResourcesThroughTheUpdateItWasGiven()
        {
            this.Editor().Rows.Single(r => r.Resource == Resource.Ore).Value = 42;

            Assert.That(this.state[Resource.Ore], Is.EqualTo(42));
        }

        [Test]
        public void TheNoteBesideAResourceComesFromTheGame()
        {
            var rows = this.Editor(resource => resource == Resource.Gold ? "+1,000 a day" : string.Empty).Rows;

            Assert.That(rows.Select(r => r.Note), Is.EqualTo(new[] { "+1,000 a day", string.Empty, string.Empty }));
        }
    }
}
