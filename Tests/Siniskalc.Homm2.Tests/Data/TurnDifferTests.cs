namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the differ of the current turn lists, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class TurnDifferTests
    {
        private readonly TurnDiffer differ = new();

        [Test]
        public void TwoEqualTurnsHaveNoChanges()
        {
            Assert.That(this.differ.Diff(0, new TurnModel(0, 1, PlayerColor.Green), new TurnModel(0, 1, PlayerColor.Green)), Is.Empty);
        }

        [Test]
        public void AnotherPlayerOnTheTurnIsOneChangeNamedByColour()
        {
            var changes = this.differ.Diff(4, new TurnModel(0, 1, PlayerColor.Green), new TurnModel(0, 2, PlayerColor.Red));

            var change = changes.Single();
            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Player", "Green", "Red")));
            Assert.That((change.OwnerName, change.Kind, change.Index), Is.EqualTo(("Current turn", Homm2ChunkKinds.Turn, 4)));
        }

        [Test]
        public void TwoDifferentRecordsThatAreNoColourAreAChangeAsWell()
        {
            var change = this.differ.Diff(0, new TurnModel(0, 8, null), new TurnModel(0, 9, null)).Single();

            Assert.That((change.Change.Before, change.Change.After), Is.EqualTo(("record 8", "record 9")));
        }

        [Test]
        public void ARecordThatIsNoColourIsNamedByItsNumber()
        {
            var change = this.differ.Diff(0, new TurnModel(0, 1, PlayerColor.Green), new TurnModel(0, 9, null)).Single();

            Assert.That(change.Change.After, Is.EqualTo("record 9"));
        }
    }
}
