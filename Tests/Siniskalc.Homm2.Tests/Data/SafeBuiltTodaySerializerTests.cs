namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The bits the model of the castles that built something this turn writes into its nine bytes, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeBuiltTodaySerializerTests
    {
        private readonly SafeBuiltTodaySerializer serializer = new();

        private static MapObjectChunk BuiltToday(params byte[] bytes) => new("Built this turn", [.. bytes, .. new byte[MapObjectLayout.BuiltTodaySize - bytes.Length]], 700, null, 0);

        [Test]
        public void TheModelThatWasReadLeavesTheChunkAsItIs()
        {
            var chunk = BuiltToday(0b0000_0101, 0, 0, 0, 0, 0, 0, 0, 0b1000_0000);
            var model = new BuiltTodayDeserializer().Deserialize(chunk);

            Assert.That(this.serializer.Serialize(chunk, model), Is.SameAs(chunk));
        }

        [Test]
        public void ACastleThatBuiltSomethingHasItsBitSetAndNothingElseMoves()
        {
            var chunk = BuiltToday();

            var written = this.serializer.Serialize(chunk, new BuiltTodayModel(0, Numbers.Of(9, 71)));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (1, (byte)0, (byte)0b0000_0010), (8, (byte)0, (byte)0b1000_0000) }));
        }

        [Test]
        public void ACastleThatIsNoLongerInTheModelHasItsBitClearedAndTheOthersStay()
        {
            var chunk = BuiltToday(0b0000_0111);

            var written = this.serializer.Serialize(chunk, new BuiltTodayModel(0, Numbers.Of(0, 2)));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (0, (byte)0b0000_0111, (byte)0b0000_0101) }));
        }

        [Test]
        public void AModelWithNoCastleClearsEveryBit()
        {
            var chunk = BuiltToday(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);

            var written = this.serializer.Serialize(chunk, new BuiltTodayModel(0, Numbers.Of()));

            Assert.That(written.ToArray(), Is.All.Zero);
        }
    }
}
