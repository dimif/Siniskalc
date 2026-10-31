using Siniskalc.Core.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The bytes a list of heroes and a selected hero write into the record of a player, on records made by hand, for both layouts: a
    /// change of the lists moves exactly the bytes of the list or of the selected hero, and what reads the same as the lists is left as it is.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafePlayerListsSerializerTests
    {
        private readonly SafePlayerListsSerializer serializer = new();
        private readonly PlayerListsDeserializer reader = new();

        private static IEnumerable<PlayerLayout> Layouts => PlayerRecords.Layouts;

        private static PlayerChunk Record(PlayerLayout layout, Action<byte[]>? set = null) => PlayerRecords.Blank(layout, PlayerColor.Blue, 2, set);

        private byte[] Written(PlayerChunk chunk, PlayerLists model) => this.serializer.Serialize(chunk, model).ToArray();

        private PlayerLists ListsOf(PlayerChunk chunk) => this.reader.Deserialize(chunk);

        [TestCaseSource(nameof(Layouts))]
        public void TheListsThatWereReadFromARecordLeaveEveryByteOfItAlone(PlayerLayout layout)
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

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, this.ListsOf(chunk))), Is.Empty);
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheListsLeaveTheResourcesTheTavernAndTheDaysLeftAloneWhenTheListChanges(PlayerLayout layout)
        {
            var chunk = Record(layout, b =>
            {
                BitConverter.GetBytes(1234).CopyTo(b, PlayerLayout.ResourcesOffset);
                new byte[] { 1, 2, 3, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.HeroIds);
                b[PlayerLayout.HeroCount] = 3;
                b[PlayerLayout.TavernHeroes] = 5;
                b[PlayerLayout.DaysLeft] = 4;
            });

            var written = this.Written(chunk, this.ListsOf(chunk) with { HeroIds = Numbers.Of(7), SelectedHero = 7 });

            var moved = ByteDiff.Of(chunk.ToArray(), written).Select(d => d.Item1).ToList();
            Assert.That(moved, Is.Not.Empty);
            Assert.That(
                moved.Where(o => o != PlayerLayout.HeroCount && o != PlayerLayout.SelectedHero && (o < PlayerLayout.HeroIds || o >= PlayerLayout.HeroIds + PlayerLayout.HeroIdPlaces)),
                Is.Empty);
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheListOfHeroesAndItsCountAreWrittenTogetherAndEmptyPlacesGetTheEmptyMark(PlayerLayout layout)
        {
            var chunk = Record(layout, b =>
            {
                new byte[] { 1, 2, 3, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }.CopyTo(b, PlayerLayout.HeroIds);
                b[PlayerLayout.HeroCount] = 3;
            });

            var written = this.Written(chunk, this.ListsOf(chunk) with { HeroIds = Numbers.Of(21, 22) });

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

            var written = this.Written(chunk, this.ListsOf(chunk) with { SelectedHero = 22 });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (PlayerLayout.SelectedHero, (byte)0xFF, (byte)22) }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void MoreHeroesThanTheListHoldsAnIdPastTheMostAHeroCanHaveAndSuchASelectedHeroAreRefused(PlayerLayout layout)
        {
            var chunk = Record(layout);
            var model = this.ListsOf(chunk);

            Assert.That(() => this.serializer.Serialize(chunk, model with { HeroIds = new Numbers(Enumerable.Range(0, 9)) }), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => this.serializer.Serialize(chunk, model with { HeroIds = Numbers.Of(255) }), Throws.TypeOf<ArgumentOutOfRangeException>(), "255 is the empty mark");
            Assert.That(() => this.serializer.Serialize(chunk, model with { SelectedHero = 255 }), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheSelectedHeroIsWrittenAsItsIdAndNoHeroGetsTheEmptyMark(PlayerLayout layout)
        {
            var chunk = Record(layout, b => b[PlayerLayout.SelectedHero] = 22);
            var model = this.ListsOf(chunk);

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, model with { SelectedHero = null })), Is.EqualTo(new[] { (PlayerLayout.SelectedHero, (byte)22, (byte)0xFF) }));
            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, model with { SelectedHero = 30 })), Is.EqualTo(new[] { (PlayerLayout.SelectedHero, (byte)22, (byte)30) }));
        }
    }
}
