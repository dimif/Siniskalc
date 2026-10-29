using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Where the hex editor is told each field of the record of a castle lives: the fields in front of the name sit at fixed bytes,
    /// the ones after it are placed from the end of the name, so a longer name moves them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleRegionProviderTests
    {
        private const int NameLengthOffset = 69;
        private const int NameOffset = 71;
        private const int TailLength = 233;

        private readonly CastleRegionProvider provider = new();

        /// <summary>A castle record of zeros whose name is <paramref name="nameLength"/> bytes long, cut to <paramref name="length"/> when given.</summary>
        private static CastleChunk Castle(int nameLength, int? length = null)
        {
            var bytes = new byte[length ?? (NameOffset + nameLength + TailLength)];
            if (bytes.Length > NameLengthOffset + 1)
            {
                BitConverter.GetBytes((ushort)nameLength).CopyTo(bytes, NameLengthOffset);
            }

            return new CastleChunk("Castle", bytes, 2000);
        }

        [Test]
        public void TheFieldsInFrontOfTheNameSitAtFixedBytes()
        {
            var regions = this.provider.GetRegions(Castle(2));

            Assert.That(regions.Take(16).Select(r => (r.Name, r.Start, r.Length)), Is.EqualTo(new[]
            {
                ("Owner", 0, 1),
                ("Built this turn", 1, 1),
                ("Faction", 3, 1),
                ("Pos X", 4, 1),
                ("Pos Y", 5, 1),
                ("Pos Lvl", 6, 1),
                ("Shipyard X", 7, 1),
                ("Shipyard Y", 8, 1),
                ("Garrison creatures", 9, 28),
                ("Garrison count", 37, 28),
                ("Garrisoned hero ID", 65, 1),
                ("Visiting hero ID", 66, 1),
                ("Mage guild level", 67, 1),
                ("Unused, always 0", 68, 1),
                ("Name length", 69, 2),
                ("Name", 71, 2),
            }));
        }

        [Test]
        public void TheFieldsAfterTheNameAreCountedFromItsEnd()
        {
            var regions = this.provider.GetRegions(Castle(2));

            Assert.That(regions.Skip(16).Select(r => (r.Name, r.Start, r.Length)), Is.EqualTo(new[]
            {
                ("Creatures available", 73, 14),
                ("Upgraded creatures available", 87, 14),
                ("Mage guild spell counts", 157, 5),
                ("Buildings now", 162, 8),
                ("Buildings built", 170, 8),
                ("Buildings allowed", 178, 8),
                ("Mage guild spells", 186, 120),
            }));
        }

        [Test]
        public void ALongerNameMovesEveryFieldAfterItByTheDifference()
        {
            var short1 = this.provider.GetRegions(Castle(2)).Where(r => r.Start > NameOffset).ToList();
            var long1 = this.provider.GetRegions(Castle(12)).Where(r => r.Start > NameOffset).ToList();

            Assert.That(long1.Select(r => r.Name), Is.EqualTo(short1.Select(r => r.Name)));
            Assert.That(long1.Select(r => r.Start - 10), Is.EqualTo(short1.Select(r => r.Start)));
            Assert.That(this.provider.GetRegions(Castle(12)).Single(r => r.Name == "Name").Length, Is.EqualTo(12));
        }

        [Test]
        public void ACastleWithNoNameHasNoNameRegion()
        {
            var regions = this.provider.GetRegions(Castle(0));

            Assert.That(regions.Select(r => r.Name), Does.Not.Contain("Name"));
            Assert.That(regions.Single(r => r.Name == "Creatures available").Start, Is.EqualTo(NameOffset));
        }

        [Test]
        public void TheDwellingsAndTheSpellCountsAreOneSlotForEachLevel()
        {
            var regions = this.provider.GetRegions(Castle(2));

            var creatures = regions.Single(r => r.Name == "Creatures available");
            var counts = regions.Single(r => r.Name == "Mage guild spell counts");
            Assert.That(creatures.Parts!.Select(p => (p.Name, p.Start, p.Length)), Is.EqualTo(Enumerable.Range(0, 7).Select(i => ($"Level {i + 1}", 73 + (i * 2), 2))));
            Assert.That(counts.Parts!.Select(p => (p.Name, p.Start, p.Length)), Is.EqualTo(Enumerable.Range(0, 5).Select(i => ($"Level {i + 1}", 157 + i, 1))));
        }

        [Test]
        public void TheGarrisonIsSevenSlotsOfFourBytesForTheCreatureAndFourForTheCount()
        {
            var regions = this.provider.GetRegions(Castle(2));

            Assert.That(regions.Single(r => r.Name == "Garrison creatures").Parts!.Select(p => p.Start), Is.EqualTo(Enumerable.Range(0, 7).Select(i => 9 + (i * 4))));
            Assert.That(regions.Single(r => r.Name == "Garrison count").Parts!.Select(p => p.Start), Is.EqualTo(Enumerable.Range(0, 7).Select(i => 37 + (i * 4))));
        }

        [Test]
        public void ARecordCutShortLeavesOutTheFieldsThatWouldRunPastItsEnd()
        {
            var regions = this.provider.GetRegions(Castle(2, length: 165));

            Assert.That(regions.Select(r => r.Name), Does.Contain("Mage guild spell counts"));
            Assert.That(regions.Select(r => r.Name), Does.Not.Contain("Buildings now"));
            Assert.That(regions.Select(r => r.Name), Does.Not.Contain("Mage guild spells"));
        }

        [Test]
        public void NoByteBelongsToTwoFields()
        {
            var leaves = this.provider.GetRegions(Castle(8)).SelectMany(r => r.Leaves()).ToList();

            Assert.That(leaves.SelectMany(l => Enumerable.Range(l.Start, l.Length)), Is.Unique);
        }
    }
}
