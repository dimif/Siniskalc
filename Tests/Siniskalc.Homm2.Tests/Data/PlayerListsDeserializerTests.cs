namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// What the lists of heroes and castles of a record of a player, and its selections, read as, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerListsDeserializerTests
    {
        private readonly PlayerListsDeserializer deserializer = new();

        [Test]
        public void TheListsHoldTheIdsThatAreNotTheEmptyMarkWhereverTheyStand()
        {
            var lists = this.deserializer.Deserialize(BlankRecords.Player("Blue", b =>
            {
                new byte[] { 7, 0xFF, 9, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.Heroes);
                new byte[] { 3, 4, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.Castles);
            }));

            Assert.That(lists.HeroIds, Is.EqualTo(new[] { 7, 9 }));
            Assert.That(lists.CastleIds, Is.EqualTo(new[] { 3, 4 }));
        }

        [Test]
        public void TheSelectionsAreIdsOrNoneForTheEmptyMark()
        {
            var empty = this.deserializer.Deserialize(BlankRecords.Player("Blue"));
            var set = this.deserializer.Deserialize(BlankRecords.Player("Blue", b =>
            {
                b[PlayerLayout.SelectedHero] = 7;
                b[PlayerLayout.SelectedCastle] = 3;
            }));

            Assert.That((empty.SelectedHero, empty.SelectedCastle), Is.EqualTo(((int?)null, (int?)null)));
            Assert.That((set.SelectedHero, set.SelectedCastle), Is.EqualTo(((int?)7, (int?)3)));
        }

        [Test]
        public void ARecordWithNoListReadsAsAPlayerThatOwnsNothing()
        {
            Assert.That(this.deserializer.Deserialize(BlankRecords.Player("Blue")).OwnsAnything, Is.False);
        }
    }
}
