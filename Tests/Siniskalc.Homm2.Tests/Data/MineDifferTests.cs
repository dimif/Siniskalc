namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the differ of the mines of the map lists, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineDifferTests
    {
        private static MineModel Mine() => new(2, MineKind.GoldMine, new MapPosition(14, 15), PlayerColor.Blue, TestCatalog.Standard);

        [Test]
        public void TwoEqualMinesHaveNoChanges()
        {
            Assert.That(new MineDiffer().Diff(0, Mine(), Mine()), Is.Empty);
        }

        [Test]
        public void ANewOwnerOfAMineIsOneChangeOfThePlaceNamedByTheMine()
        {
            var change = new MineDiffer().Diff(2, Mine(), Mine() with { Owner = PlayerColor.Neutral }).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Owner", "Blue", "Neutral")));
            Assert.That((change.OwnerName, change.Kind, change.Index), Is.EqualTo((Mine().Name, Homm2ChunkKinds.Mines, 2)));
        }
    }
}
