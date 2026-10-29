using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests how the lists of the merchants are found in bytes made by hand: a 32 bit number of merchants and, right behind it, that many
    /// lists of seven 32 bit artifact codes, among the first bytes of the savefile.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MerchantBytesLoaderTests
    {
        private static readonly uint[] Four = [0x13, 0x14, 0x15, 0x7E];

        private readonly MerchantBytesLoader loader = new();

        /// <summary>The count, then <paramref name="count"/> lists whose first slots hold <paramref name="codes"/> and the others are empty, behind <paramref name="lead"/> zero bytes.</summary>
        private static byte[] Savegame(int lead, uint count, uint[] codes, int? lists = null, int trailing = 8)
        {
            var bytes = new List<byte>(new byte[lead]);
            bytes.AddRange(BitConverter.GetBytes(count));
            for (var slot = 0; slot < (lists ?? (int)count) * MerchantLayout.SlotCount; slot++)
            {
                bytes.AddRange(BitConverter.GetBytes(slot < codes.Length ? codes[slot] : MerchantLayout.Empty));
            }

            bytes.AddRange(new byte[trailing]);
            return [.. bytes];
        }

        [Test]
        public void OneListIsFoundBehindItsCountWithItsOffsetAndTheNameOfTheBlackMarket()
        {
            var chunks = this.loader.LoadMerchantChunks(Savegame(10, 1, Four));

            Assert.That(chunks, Has.Count.EqualTo(1));
            Assert.That((chunks[0].Name, chunks[0].StartIndex, chunks[0].Index, chunks[0].Length), Is.EqualTo(("Black market", 14, 0, 28)));
            Assert.That(chunks[0].GetUInt32At(12), Is.EqualTo(0x7Eu));
        }

        [Test]
        public void SeveralListsAreNamedByTheirNumberAndFollowEachOther()
        {
            var chunks = this.loader.LoadMerchantChunks(Savegame(0, 3, Four));

            Assert.That(chunks.Select(c => c.Name), Is.EqualTo(new[] { "Black market 1", "Black market 2", "Black market 3" }));
            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(new[] { 4, 32, 60 }));
            Assert.That(chunks.Select(c => c.Index), Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void ThirtyTwoListsAreTheMostAndThirtyThreeAreNoTable()
        {
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 32, Four)), Has.Count.EqualTo(32));
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 33, Four)), Is.Empty);
        }

        [Test]
        public void NoMerchantsIsNoTable()
        {
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 0, Four, lists: 1)), Is.Empty);
        }

        [Test]
        public void AtLeastFourArtifactsMustBeInStockInAllTheLists()
        {
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 1, Four.Take(3).ToArray())), Is.Empty);
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 2, Four.Take(3).Concat([MerchantLayout.Empty, MerchantLayout.Empty, MerchantLayout.Empty, MerchantLayout.Empty, 0x13u]).ToArray())), Has.Count.EqualTo(2));
        }

        [Test]
        public void ACodeThatIsNoArtifactOrDoesNotFitAByteIsNoTable()
        {
            var unknown = (uint)Enumerable.Range(0, 256).First(c => !Artifact.All.ContainsKey((byte)c));

            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 1, [.. Four, unknown])), Is.Empty);
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 1, [.. Four, 0x113u])), Is.Empty);
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 1, [.. Four, 0xFFFFFFFEu])), Is.Empty);
        }

        [Test]
        public void ListsThatDoNotFitInTheBytesAreNoTable()
        {
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 2, Four, lists: 1, trailing: 0)), Is.Empty);
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 2, Four, lists: 2, trailing: 0)), Has.Count.EqualTo(2));
        }

        [Test]
        public void ATableIsOnlySearchedInTheFirstBytesOfTheSavegame()
        {
            var last = Savegame(MerchantLayout.SearchLength - 4, 1, Four);
            var tooLate = Savegame(MerchantLayout.SearchLength - 3, 1, Four);

            Assert.That(this.loader.LoadMerchantChunks(last).Single().StartIndex, Is.EqualTo(MerchantLayout.SearchLength));
            Assert.That(this.loader.LoadMerchantChunks(tooLate), Is.Empty);
        }

        [Test]
        public void TheFirstTableThatLooksRightWinsAndBytesTooShortGiveNone()
        {
            var first = Savegame(3, 1, Four, trailing: 0);
            var both = first.Concat(Savegame(0, 2, Four)).ToArray();

            Assert.That(this.loader.LoadMerchantChunks(both).Single().StartIndex, Is.EqualTo(7));
            Assert.That(this.loader.LoadMerchantChunks([]), Is.Empty);
            Assert.That(this.loader.LoadMerchantChunks(new byte[31]), Is.Empty);
        }

        [Test]
        public void ATableRightAtTheStartOfTheSavegameAfterItsCountIsFound()
        {
            Assert.That(this.loader.LoadMerchantChunks(Savegame(0, 1, Four, trailing: 0)).Single().StartIndex, Is.EqualTo(4));
        }
    }
}
