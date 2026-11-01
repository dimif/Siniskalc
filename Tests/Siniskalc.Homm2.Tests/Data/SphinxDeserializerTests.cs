namespace Siniskalc.Homm2.Tests.Data
{
    using System.Text;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the bytes of a sphinx of the map read as, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SphinxDeserializerTests
    {
        /// <summary>A sphinx whose riddle has room for 99 characters, with <paramref name="set"/> applied to its bytes.</summary>
        internal static MapObjectChunk Sphinx(Action<byte[]>? set = null)
        {
            var bytes = new byte[MapObjectLayout.SphinxRiddle + 100];
            set?.Invoke(bytes);
            return new MapObjectChunk("Sphinx", bytes, 300, new MapPosition(5, 6), 4);
        }

        private static void Text(byte[] bytes, int offset, string text) => Encoding.Latin1.GetBytes(text).CopyTo(bytes, offset);

        [Test]
        public void ASphinxIsItsTileItsPlaceInTheTableAndTheRoomItsRiddleHas()
        {
            var sphinx = new SphinxDeserializer(TestCatalog.Standard).Deserialize(Sphinx());

            Assert.That((sphinx.Index, sphinx.Position, sphinx.RiddleCapacity), Is.EqualTo((4, new MapPosition(5, 6), 99)));
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(7, true)]
        public void TheRiddleIsAnsweredWhenTheFirstByteIsZeroAndNotAnsweredForAnyOtherByte(int flag, bool active)
        {
            var sphinx = new SphinxDeserializer(TestCatalog.Standard).Deserialize(Sphinx(b => b[MapObjectLayout.SphinxActive] = (byte)flag));

            Assert.That(sphinx.IsActive, Is.EqualTo(active));
        }

        [Test]
        public void TheSevenRewardsAreFourByteNumbersAndTheArtifactIsTwoBytes()
        {
            var sphinx = new SphinxDeserializer(TestCatalog.Standard).Deserialize(Sphinx(b =>
            {
                for (var i = 0; i < 7; i++)
                {
                    BitConverter.GetBytes((uint)(100 * (i + 1))).CopyTo(b, MapObjectLayout.SphinxReward + (i * 4));
                }

                BitConverter.GetBytes((ushort)96).CopyTo(b, MapObjectLayout.SphinxArtifact);
            }));

            Assert.That(sphinx.Reward, Is.EqualTo(new[] { 100, 200, 300, 400, 500, 600, 700 }));
            Assert.That(sphinx.ArtifactCode, Is.EqualTo(96));
        }

        [Test]
        public void ARewardTooBigForTheModelReadsAsTheNumberTheCastGivesAndIsNotAnError()
        {
            var sphinx = new SphinxDeserializer(TestCatalog.Standard).Deserialize(Sphinx(b => BitConverter.GetBytes(0xFFFF_FFF0u).CopyTo(b, MapObjectLayout.SphinxReward)));

            Assert.That(sphinx.Reward[0], Is.EqualTo(-16));
        }

        [Test]
        public void TheAnswersAreAsManyTextsAsTheCountSaysEachUpToItsFirstZero()
        {
            var sphinx = new SphinxDeserializer(TestCatalog.Standard).Deserialize(Sphinx(b =>
            {
                b[MapObjectLayout.SphinxAnswerCount] = 2;
                Text(b, MapObjectLayout.SphinxAnswers, "darkness");
                Text(b, MapObjectLayout.SphinxAnswers + MapObjectLayout.SphinxAnswerSize, "shadow");
                Text(b, MapObjectLayout.SphinxAnswers + (2 * MapObjectLayout.SphinxAnswerSize), "not an answer");
            }));

            Assert.That(sphinx.Answers, Is.EqualTo(new[] { "darkness", "shadow" }));
        }

        [Test]
        public void ARiddleIsReadUpToItsFirstZeroByte()
        {
            var sphinx = new SphinxDeserializer(TestCatalog.Standard).Deserialize(Sphinx(b => Text(b, MapObjectLayout.SphinxRiddle, "I Only Creep.\0leftover")));

            Assert.That(sphinx.Riddle, Is.EqualTo("I Only Creep."));
        }
    }
}
