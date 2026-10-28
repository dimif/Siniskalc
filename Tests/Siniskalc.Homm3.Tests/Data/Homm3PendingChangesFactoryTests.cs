namespace Siniskalc.Homm3.Tests.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;

    /// <summary>
    /// Tests the pending changes the editor of Heroes of Might and Magic III makes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class Homm3PendingChangesFactoryTests
    {
        private static readonly Change SomeChange = new(ChangeAreas.General, "Owner", "Neutral", "Red");

        [Test]
        public void TheChangeOfAHeroIsFiledUnderTheHeroChunksAtItsIndex()
        {
            var change = new Homm3PendingChangesFactory().ForHero("Christian", 4, SomeChange, hero => hero);

            Assert.That((change.OwnerName, change.Kind, change.Index, change.Change), Is.EqualTo(("Christian", Homm3ChunkKinds.Hero, 4, SomeChange)));
        }

        [Test]
        public void TheChangeOfAPlayerIsFiledUnderThePlayerChunksAtItsIndex()
        {
            var change = new Homm3PendingChangesFactory().ForPlayer("Blue", 0, SomeChange, player => player);

            Assert.That((change.OwnerName, change.Kind, change.Index, change.Change), Is.EqualTo(("Blue", Homm3ChunkKinds.Player, 0, SomeChange)));
        }

        [Test]
        public void TheChangeOfACastleIsFiledUnderTheCastleChunksAtItsIndex()
        {
            var change = new Homm3PendingChangesFactory().ForCastle("Still Water", 2, SomeChange, castle => castle);

            Assert.That((change.OwnerName, change.Kind, change.Index, change.Change), Is.EqualTo(("Still Water", Homm3ChunkKinds.Castle, 2, SomeChange)));
        }

        [Test]
        public void TheChangeOfTheObelisksIsNamedObelisksAndFiledUnderTheObeliskChunks()
        {
            var change = new Homm3PendingChangesFactory().ForObelisks(1, SomeChange, visits => visits);

            Assert.That((change.OwnerName, change.Kind, change.Index, change.Change), Is.EqualTo(("Obelisks", Homm3ChunkKinds.Obelisks, 1, SomeChange)));
        }
    }
}
