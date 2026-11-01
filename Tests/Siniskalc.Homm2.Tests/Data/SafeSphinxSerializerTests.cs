namespace Siniskalc.Homm2.Tests.Data
{
    using System.Text;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The bytes a sphinx model writes into the record of a sphinx, one field at a time on a record made by hand: a change of the model
    /// moves exactly the bytes of that field and what the model reads the same way is left as it is.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeSphinxSerializerTests
    {
        private readonly SafeSphinxSerializer serializer = new();
        private readonly SphinxDeserializer reader = new(TestCatalog.Standard);

        private static MapObjectChunk Record() => SphinxDeserializerTests.Sphinx(b =>
        {
            b[MapObjectLayout.SphinxActive] = 1;
            BitConverter.GetBytes(1000u).CopyTo(b, MapObjectLayout.SphinxReward + 24);
            BitConverter.GetBytes((ushort)96).CopyTo(b, MapObjectLayout.SphinxArtifact);
            b[MapObjectLayout.SphinxAnswerCount] = 2;
            Encoding.Latin1.GetBytes("darkness").CopyTo(b, MapObjectLayout.SphinxAnswers);
            Encoding.Latin1.GetBytes("shadow").CopyTo(b, MapObjectLayout.SphinxAnswers + MapObjectLayout.SphinxAnswerSize);
            Encoding.Latin1.GetBytes("I Only Creep.").CopyTo(b, MapObjectLayout.SphinxRiddle);
        });

        private SphinxModel Model(MapObjectChunk chunk) => this.reader.Deserialize(chunk);

        private byte[] Written(MapObjectChunk chunk, SphinxModel model) => this.serializer.Serialize(chunk, model).ToArray();

        [Test]
        public void TheModelThatWasReadFromARecordLeavesEveryByteOfItAlone()
        {
            var chunk = Record();

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, this.Model(chunk))), Is.Empty);
        }

        [Test]
        public void AnActiveFlagThatIsAnyNonZeroByteIsKeptWhileTheModelSaysActive()
        {
            var chunk = SphinxDeserializerTests.Sphinx(b =>
            {
                b[MapObjectLayout.SphinxActive] = 7;
                b[MapObjectLayout.SphinxAnswerCount] = 1;
                b[MapObjectLayout.SphinxAnswers] = (byte)'x';
                b[MapObjectLayout.SphinxRiddle] = (byte)'r';
            });

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, this.Model(chunk))), Is.Empty);
            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, this.Model(chunk) with { IsActive = false })), Is.EqualTo(new[] { (MapObjectLayout.SphinxActive, (byte)7, (byte)0) }));
        }

        [Test]
        public void AnAnsweredSphinxBecomesActiveAgainWithTheByteOne()
        {
            var chunk = SphinxDeserializerTests.Sphinx(b =>
            {
                b[MapObjectLayout.SphinxAnswerCount] = 1;
                b[MapObjectLayout.SphinxAnswers] = (byte)'x';
                b[MapObjectLayout.SphinxRiddle] = (byte)'r';
            });

            var written = this.Written(chunk, this.Model(chunk) with { IsActive = true });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (MapObjectLayout.SphinxActive, (byte)0, (byte)1) }));
        }

        [Test]
        public void ARewardIsWrittenInFourBytesLeastSignificantFirstAndTheOthersStay()
        {
            var chunk = Record();
            var model = this.Model(chunk);

            var written = this.Written(chunk, model with { Reward = model.Reward.Select((v, i) => i == 2 ? 0x0102_0304 : v).ToList() });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (MapObjectLayout.SphinxReward + 8, (byte)0, (byte)0x04),
                (MapObjectLayout.SphinxReward + 9, (byte)0, (byte)0x03),
                (MapObjectLayout.SphinxReward + 10, (byte)0, (byte)0x02),
                (MapObjectLayout.SphinxReward + 11, (byte)0, (byte)0x01),
            }));
        }

        [Test]
        public void ARewardTooBigForTheModelIsKeptWhileTheModelSaysTheNumberItReadsAsAndANegativeRewardIsRefused()
        {
            var chunk = SphinxDeserializerTests.Sphinx(b =>
            {
                BitConverter.GetBytes(0xFFFF_FFF0u).CopyTo(b, MapObjectLayout.SphinxReward);
                b[MapObjectLayout.SphinxAnswerCount] = 1;
                b[MapObjectLayout.SphinxAnswers] = (byte)'x';
                b[MapObjectLayout.SphinxRiddle] = (byte)'r';
            });
            var model = this.Model(chunk);

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, model)), Is.Empty);
            Assert.That(() => this.serializer.Serialize(Record(), model), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheArtifactIsWrittenInTwoBytes()
        {
            var chunk = Record();

            var written = this.Written(chunk, this.Model(chunk) with { ArtifactCode = 0x0102 });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (MapObjectLayout.SphinxArtifact, (byte)96, (byte)0x02),
                (MapObjectLayout.SphinxArtifact + 1, (byte)0, (byte)0x01),
            }));
        }

        [Test]
        public void NewAnswersReplaceTheOldOnesWithTheirCountAndNothingOfTheOldTextRemains()
        {
            var chunk = Record();

            var written = this.Written(chunk, this.Model(chunk) with { Answers = ["one"] });

            Assert.That(written[MapObjectLayout.SphinxAnswerCount], Is.EqualTo(1));
            Assert.That(written.AsSpan(MapObjectLayout.SphinxAnswers, 3).ToArray(), Is.EqualTo(Encoding.Latin1.GetBytes("one")));
            Assert.That(written.AsSpan(MapObjectLayout.SphinxAnswers + 3, (Constants.Sphinx.MaxAnswers * MapObjectLayout.SphinxAnswerSize) - 3).ToArray(), Is.All.Zero);
        }

        [Test]
        public void ANewRiddleReplacesTheOldOneAndNothingOfTheOldTextRemains()
        {
            var chunk = Record();

            var written = this.Written(chunk, this.Model(chunk) with { Riddle = "What?" });

            Assert.That(written.AsSpan(MapObjectLayout.SphinxRiddle, 5).ToArray(), Is.EqualTo(Encoding.Latin1.GetBytes("What?")));
            Assert.That(written.AsSpan(MapObjectLayout.SphinxRiddle + 5).ToArray(), Is.All.Zero);
        }

        [Test]
        public void AnAnswerListARiddleOrAnAnswerThatDoesNotFitIsRefusedAndTheRecordIsNotChanged()
        {
            var chunk = Record();
            var before = chunk.ToArray();
            var model = this.Model(chunk);

            Assert.That(() => this.serializer.Serialize(chunk, model with { Answers = [] }), Throws.TypeOf<ArgumentOutOfRangeException>(), "a sphinx needs an answer");
            Assert.That(() => this.serializer.Serialize(chunk, model with { Answers = Enumerable.Repeat("a", 9).ToList() }), Throws.TypeOf<ArgumentOutOfRangeException>(), "at most eight");
            Assert.That(() => this.serializer.Serialize(chunk, model with { Answers = [new string('a', 13)] }), Throws.TypeOf<ArgumentOutOfRangeException>(), "an answer is at most twelve characters");
            Assert.That(() => this.serializer.Serialize(chunk, model with { Riddle = string.Empty }), Throws.TypeOf<ArgumentOutOfRangeException>(), "a sphinx needs a riddle");
            Assert.That(() => this.serializer.Serialize(chunk, model with { Riddle = new string('r', 100) }), Throws.TypeOf<ArgumentOutOfRangeException>(), "the riddle has room for 99");
            Assert.That(chunk.ToArray(), Is.EqualTo(before));
        }
    }
}
