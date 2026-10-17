namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the card of the position or the marker of a hero.
    /// </summary>
    [TestFixture]
    internal sealed class MapPointCardViewModelTests
    {
        private int x = -1;
        private int y = -1;
        private FacingDirection facing = FacingDirection.North;

        [SetUp]
        public void PutTheHeroOffTheMap()
        {
            this.x = -1;
            this.y = -1;
            this.facing = FacingDirection.North;
        }

        [Test]
        public void ACoordinateIsClampedToTheMapAndWrittenOnlyWhenItChanges()
        {
            var writes = 0;
            var card = new MapPointCardViewModel("Position", () => this.x, v => { this.x = v; writes++; }, () => this.y, v => this.y = v, new NumberRange(-1, 143));

            card.X = 500;
            card.X = 500;
            card.Y = -7;

            Assert.That(this.x, Is.EqualTo(143));
            Assert.That(writes, Is.EqualTo(1));
            Assert.That(this.y, Is.EqualTo(-1));
        }

        [Test]
        public void ACardWithoutLevelFacingOrRemoveButtonOffersNoneOfThem()
        {
            var card = new MapPointCardViewModel("Marker", () => this.x, v => this.x = v, () => this.y, v => this.y = v, new NumberRange(-1, 143));

            Assert.That(card.HasLevel, Is.False);
            Assert.That(card.HasFacing, Is.False);
            Assert.That(card.CanRemove, Is.False);
            Assert.That(card.RemoveCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void TurningTheHeroWrapsAroundTheCompass()
        {
            var card = new MapPointCardViewModel("Position", () => this.x, v => this.x = v, () => this.y, v => this.y = v, new NumberRange(0, 143), getFacing: () => this.facing, setFacing: v => this.facing = v);

            card.RotateLeftCommand.Execute(null);
            Assert.That(this.facing, Is.EqualTo(FacingDirection.NorthWest));

            card.RotateRightCommand.Execute(null);
            card.RotateRightCommand.Execute(null);
            Assert.That(this.facing, Is.EqualTo(FacingDirection.NorthEast));
        }

        [Test]
        public void TheRemoveButtonIsOffWhileThereIsNothingToRemove()
        {
            var removed = 0;
            var card = new MapPointCardViewModel("Marker", () => this.x, v => this.x = v, () => this.y, v => this.y = v, new NumberRange(-1, 143), remove: () => removed++, isSet: () => this.x >= 0);

            Assert.That(card.RemoveCommand.CanExecute(null), Is.False);

            this.x = 5;
            Assert.That(card.RemoveCommand.CanExecute(null), Is.True);
            card.RemoveCommand.Execute(null);
            Assert.That(removed, Is.EqualTo(1));
        }
    }
}
