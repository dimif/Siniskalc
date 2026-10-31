namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The bytes the lists of heroes and castles of a player write into its record, on records made by hand: a list moves its places and its
    /// count together, and what reads as the model says is left as it is.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafePlayerListsSerializerTests
    {
        private readonly SafePlayerListsSerializer serializer = new();
        private readonly PlayerListsDeserializer reader = new();

        private static PlayerChunk Record(Action<byte[]>? set = null) => BlankRecords.Player("Blue", set);

        private byte[] Written(PlayerChunk chunk, PlayerLists lists) => this.serializer.Serialize(chunk, lists).ToArray();

        private PlayerLists ListsOf(PlayerChunk chunk) => this.reader.Deserialize(chunk);

        [Test]
        public void TheListsThatWereReadFromARecordLeaveEveryByteOfItAlone()
        {
            var chunk = Record(b =>
            {
                new byte[] { 7, 0xFF, 9, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.Heroes);
                b[PlayerLayout.HeroCount] = 2;
                b[PlayerLayout.SelectedHero] = 9;
            });

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, this.ListsOf(chunk))), Is.Empty);
        }

        [Test]
        public void TheListOfHeroesAndItsCountAreWrittenTogetherAndEmptyPlacesGetTheEmptyMark()
        {
            var chunk = Record(b =>
            {
                new byte[] { 1, 2, 3, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.Heroes);
                b[PlayerLayout.HeroCount] = 3;
            });

            var written = this.Written(chunk, this.ListsOf(chunk) with { HeroIds = Numbers.Of(7, 9) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (PlayerLayout.HeroCount, (byte)3, (byte)2),
                (PlayerLayout.Heroes, (byte)1, (byte)7),
                (PlayerLayout.Heroes + 1, (byte)2, (byte)9),
                (PlayerLayout.Heroes + 2, (byte)3, (byte)0xFF),
            }));
        }

        [Test]
        public void AListThatReadsAsTheModelSaysIsLeftWithItsCountEvenWhenTheRecordHasGapsInIt()
        {
            var chunk = Record(b =>
            {
                new byte[] { 3, 0xFF, 4, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.Castles);
                b[PlayerLayout.CastleCount] = 9;
            });

            var written = this.Written(chunk, this.ListsOf(chunk) with { SelectedCastle = 4 });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (PlayerLayout.SelectedCastle, (byte)0xFF, (byte)4) }));
        }

        [Test]
        public void AnIdTakenOutOfASelectionLeavesTheEmptyMark()
        {
            var chunk = Record(b => b[PlayerLayout.SelectedHero] = 7);

            var written = this.Written(chunk, this.ListsOf(chunk) with { SelectedHero = null });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (PlayerLayout.SelectedHero, (byte)7, (byte)0xFF) }));
        }
    }
}
