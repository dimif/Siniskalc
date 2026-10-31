namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The bytes a player model writes into the record of a player, one field at a time on a record made by hand: a change of the model
    /// moves exactly the bytes of that field, and what the model reads the same way as the savegame is left as it is.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafePlayerSerializerTests
    {
        private readonly SafePlayerSerializer serializer = new();
        private readonly PlayerDeserializer reader = new();

        private static PlayerChunk Record(Action<byte[]>? set = null) => BlankRecords.Player("Blue", set);

        private byte[] Written(PlayerChunk chunk, PlayerModel model) => this.serializer.Serialize(chunk, model).ToArray();

        private PlayerModel ModelOf(PlayerChunk chunk) => this.reader.Deserialize(chunk);

        [Test]
        public void TheModelThatWasReadFromARecordLeavesEveryByteOfItAlone()
        {
            var chunk = Record(b =>
            {
                BitConverter.GetBytes(1234u).CopyTo(b, PlayerLayout.Resources);
                new byte[] { 7, 0xFF, 9, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.Heroes);
                b[PlayerLayout.HeroCount] = 2;
                b[PlayerLayout.Tavern] = 12;
                b[PlayerLayout.SelectedHero] = 9;
            });

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, this.ModelOf(chunk))), Is.Empty);
        }

        [Test]
        public void AnAmountOfAResourceIsWrittenInFourBytesLeastSignificantFirstAndNothingElseMoves()
        {
            var chunk = Record();
            var model = this.ModelOf(chunk);

            var written = this.Written(chunk, model with { Resources = model.Resources.With(Resource.Gold, 0x0102_0304) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (PlayerLayout.Resources + 24, (byte)0, (byte)0x04),
                (PlayerLayout.Resources + 25, (byte)0, (byte)0x03),
                (PlayerLayout.Resources + 26, (byte)0, (byte)0x02),
                (PlayerLayout.Resources + 27, (byte)0, (byte)0x01),
            }));
        }

        [Test]
        public void TheGoldIncomeAndTheIncomeOfAnotherResourceAreWrittenToTheirOwnPlaces()
        {
            var chunk = Record();
            var model = this.ModelOf(chunk);

            var written = this.Written(chunk, model with { Income = model.Income.With(Resource.Gold, 500).With(Resource.Ore, 2) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (PlayerLayout.Income + 8, (byte)0, (byte)2),
                (PlayerLayout.GoldIncome, (byte)0, (byte)0xF4),
                (PlayerLayout.GoldIncome + 1, (byte)0, (byte)0x01),
            }));
        }

        [Test]
        public void AStoredAmountBeyondTheLimitIsKeptWhileTheModelSaysItAndTheEditorDoesNotWriteSuchAnAmountElsewhere()
        {
            var big = Record(b => BitConverter.GetBytes(0xFFFF_FFF0u).CopyTo(b, PlayerLayout.Resources + 24));
            var model = this.ModelOf(big);

            Assert.That(ByteDiff.Of(big.ToArray(), this.Written(big, model)), Is.Empty, "the stored amount is read as it is and left alone");
            Assert.That(() => this.serializer.Serialize(Record(), model), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheListOfHeroesAndItsCountAreWrittenTogetherAndEmptyPlacesGetTheEmptyMark()
        {
            var chunk = Record(b =>
            {
                new byte[] { 1, 2, 3, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.Heroes);
                b[PlayerLayout.HeroCount] = 3;
            });

            var written = this.Written(chunk, this.ModelOf(chunk) with { HeroIds = Numbers.Of(7, 9) });

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

            var written = this.Written(chunk, this.ModelOf(chunk) with { SelectedCastle = 4 });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (PlayerLayout.SelectedCastle, (byte)0xFF, (byte)4) }));
        }

        [Test]
        public void AnIdTakenOutOfASelectionLeavesTheEmptyMarkAndTheTavernHeroesAreWrittenOneByOne()
        {
            var chunk = Record(b =>
            {
                b[PlayerLayout.SelectedHero] = 7;
                b[PlayerLayout.DaysLeft] = 5;
                b[PlayerLayout.Tavern] = 12;
            });

            var written = this.Written(chunk, this.ModelOf(chunk) with { SelectedHero = null, DaysLeft = null, Tavern = new TavernHeroes(12, 20) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (PlayerLayout.SelectedHero, (byte)7, (byte)0xFF),
                (PlayerLayout.Tavern + 1, (byte)0xFF, (byte)20),
                (PlayerLayout.DaysLeft, (byte)5, (byte)0xFF),
            }));
        }
    }
}
