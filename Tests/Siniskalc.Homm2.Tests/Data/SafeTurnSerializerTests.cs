namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The byte the model of the current turn writes into its record, on a record made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeTurnSerializerTests
    {
        private readonly SafeTurnSerializer serializer = new();

        private static MapObjectChunk Turn(byte record) => new("Turn", [record], 600, null, 3, PlayerRecordOrder.Pack([0, 1, 2, 3, 4, 5]));

        [Test]
        public void ARecordThatIsNotChangedLeavesTheChunkAsItIsWhateverColourTheModelHolds()
        {
            var chunk = Turn(2);

            Assert.That(this.serializer.Serialize(chunk, new TurnModel(3, 2, PlayerColor.Blue)), Is.SameAs(chunk));
        }

        [Test]
        public void AnotherRecordIsWrittenAsTheByteOfTheTurn()
        {
            var chunk = Turn(2);

            var written = this.serializer.Serialize(chunk, new TurnModel(3, 5, PlayerColor.Purple));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (0, (byte)2, (byte)5) }));
        }

        [Test]
        public void ARecordOfAPlayerThatDoesNotExistIsRefusedAndTheChunkIsNotChanged()
        {
            var chunk = Turn(2);

            Assert.That(() => this.serializer.Serialize(chunk, new TurnModel(3, 6, null)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(chunk.ToArray(), Is.EqualTo(new byte[] { 2 }));
        }

        [Test]
        public void ARecordThatIsPastTheSixPlayersIsKeptWhileTheModelHoldsItAsItIs()
        {
            var chunk = Turn(9);

            Assert.That(this.serializer.Serialize(chunk, new TurnModel(3, 9, null)), Is.SameAs(chunk));
        }
    }
}
