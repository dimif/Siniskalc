using Siniskalc.Core.Models;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The bytes a player model writes into the record of a player, one field at a time on records made by hand, for both layouts: a
    /// change of the model moves exactly the bytes of that field, and what the model reads the same way as the savefile is left as it is.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafePlayerSerializerTests
    {
        private readonly SafePlayerSerializer serializer = new();
        private readonly PlayerDeserializer reader = new();

        private static IEnumerable<PlayerLayout> Layouts => PlayerRecords.Layouts;

        private static PlayerChunk Record(PlayerLayout layout, Action<byte[]>? set = null) => PlayerRecords.Blank(layout, PlayerColor.Blue, 2, set);

        private byte[] Written(PlayerChunk chunk, PlayerModel model) => this.serializer.Serialize(chunk, model).ToArray();

        private PlayerModel ModelOf(PlayerChunk chunk) => this.reader.Deserialize(chunk);

        [TestCaseSource(nameof(Layouts))]
        public void TheModelThatWasReadFromARecordLeavesEveryByteOfItAlone(PlayerLayout layout)
        {
            var chunk = Record(layout, b =>
            {
                BitConverter.GetBytes(1234).CopyTo(b, PlayerLayout.ResourcesOffset);
                new byte[] { 21, 0xFF, 22, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.HeroIds);
                b[PlayerLayout.HeroCount] = 2;
                b[PlayerLayout.SelectedHero] = 22;
                b[PlayerLayout.TavernHeroes] = 5;
                b[PlayerLayout.DaysLeft] = 0x40;
            });

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, this.ModelOf(chunk))), Is.Empty);
        }

        [TestCaseSource(nameof(Layouts))]
        public void AnAmountOfAResourceIsWrittenInFourBytesLeastSignificantFirstAndNothingElseMoves(PlayerLayout layout)
        {
            var chunk = Record(layout);
            var model = this.ModelOf(chunk);

            var written = this.Written(chunk, model with { Resources = model.Resources.With(Resource.Gold, 0x0102_0304) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (PlayerLayout.OffsetOf(Resource.Gold), (byte)0, (byte)0x04),
                (PlayerLayout.OffsetOf(Resource.Gold) + 1, (byte)0, (byte)0x03),
                (PlayerLayout.OffsetOf(Resource.Gold) + 2, (byte)0, (byte)0x02),
                (PlayerLayout.OffsetOf(Resource.Gold) + 3, (byte)0, (byte)0x01),
            }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void AnAmountThatDoesNotFitFourBytesIsRefusedAndTheRecordIsNotChanged(PlayerLayout layout)
        {
            var chunk = Record(layout);
            var before = chunk.ToArray();
            var model = this.ModelOf(chunk);

            Assert.That(() => this.serializer.Serialize(chunk, model with { Resources = model.Resources.With(Resource.Gold, (long)int.MaxValue + 1) }), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(chunk.ToArray(), Is.EqualTo(before));
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheListOfHeroesAndItsCountAreWrittenTogetherAndEmptyPlacesGetTheEmptyMark(PlayerLayout layout)
        {
            var chunk = Record(layout, b =>
            {
                new byte[] { 1, 2, 3, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.HeroIds);
                b[PlayerLayout.HeroCount] = 3;
            });

            var written = this.Written(chunk, this.ModelOf(chunk) with { HeroIds = Numbers.Of(21, 22) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (PlayerLayout.HeroCount, (byte)3, (byte)2),
                (PlayerLayout.HeroIds, (byte)1, (byte)21),
                (PlayerLayout.HeroIds + 1, (byte)2, (byte)22),
                (PlayerLayout.HeroIds + 2, (byte)3, (byte)0xFF),
            }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void AListThatReadsAsTheModelSaysIsLeftWithItsCountEvenWhenTheRecordHasGapsInIt(PlayerLayout layout)
        {
            var chunk = Record(layout, b =>
            {
                new byte[] { 21, 0xFF, 22, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.HeroIds);
                b[PlayerLayout.HeroCount] = 9;
            });

            var written = this.Written(chunk, this.ModelOf(chunk) with { SelectedHero = 22 });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (PlayerLayout.SelectedHero, (byte)0xFF, (byte)22) }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void MoreHeroesThanTheListHoldsAnIdPastTheMostAHeroCanHaveAndSuchASelectedHeroAreRefused(PlayerLayout layout)
        {
            var chunk = Record(layout);
            var model = this.ModelOf(chunk);

            Assert.That(() => this.serializer.Serialize(chunk, model with { HeroIds = new Numbers(Enumerable.Range(0, 9)) }), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => this.serializer.Serialize(chunk, model with { HeroIds = Numbers.Of(255) }), Throws.TypeOf<ArgumentOutOfRangeException>(), "255 is the empty mark");
            Assert.That(() => this.serializer.Serialize(chunk, model with { SelectedHero = 255 }), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheSelectedHeroIsWrittenAsItsIdAndNoHeroGetsTheEmptyMark(PlayerLayout layout)
        {
            var chunk = Record(layout, b => b[PlayerLayout.SelectedHero] = 22);
            var model = this.ModelOf(chunk);

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, model with { SelectedHero = null })), Is.EqualTo(new[] { (PlayerLayout.SelectedHero, (byte)22, (byte)0xFF) }));
            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, model with { SelectedHero = 30 })), Is.EqualTo(new[] { (PlayerLayout.SelectedHero, (byte)22, (byte)30) }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheTavernHeroesAreWrittenOneByOneAndAnIdPastTheMostAHeroCanHaveIsRefused(PlayerLayout layout)
        {
            var chunk = Record(layout, b => b[PlayerLayout.TavernHeroes] = 5);
            var model = this.ModelOf(chunk);

            var written = this.Written(chunk, model with { Tavern = new TavernHeroes(null, 40) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (PlayerLayout.TavernHeroes, (byte)5, (byte)0xFF),
                (PlayerLayout.TavernHeroes + 1, (byte)0xFF, (byte)40),
            }));
            Assert.That(() => this.serializer.Serialize(chunk, model with { Tavern = new TavernHeroes(255, null) }), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCaseSource(nameof(Layouts))]
        public void ADaysLeftByteThatIsNoCountIsKeptWhileTheModelHasNoneAndReplacedWhenItHasADay(PlayerLayout layout)
        {
            var chunk = Record(layout, b => b[PlayerLayout.DaysLeft] = 0x40);
            var model = this.ModelOf(chunk);

            Assert.That(model.DaysLeft, Is.Null);
            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, model)), Is.Empty);
            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, model with { DaysLeft = 3 })), Is.EqualTo(new[] { (PlayerLayout.DaysLeft, (byte)0x40, (byte)3) }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void ADayCountTakenAwayGetsTheEmptyMarkAndOneBeyondTheMostAPlayerCanHaveIsRefused(PlayerLayout layout)
        {
            var chunk = Record(layout, b => b[PlayerLayout.DaysLeft] = 5);
            var model = this.ModelOf(chunk);

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, model with { DaysLeft = null })), Is.EqualTo(new[] { (PlayerLayout.DaysLeft, (byte)5, (byte)0xFF) }));
            Assert.That(() => this.serializer.Serialize(chunk, model with { DaysLeft = 8 }), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheTownsAndTheColourOfThePlayerAreNeverWrittenWhateverTheModelSays(PlayerLayout layout)
        {
            var chunk = Record(layout, b =>
            {
                b[PlayerLayout.TownCount] = 4;
                b[PlayerLayout.SelectedTown] = 2;
            });

            var written = this.Written(chunk, this.ModelOf(chunk) with { Color = PlayerColor.Pink });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.Empty);
        }
    }
}
