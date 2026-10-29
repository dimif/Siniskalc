using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Where the hex editor is told each field of the record of a dwelling lives.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class DwellingRegionProviderTests
    {
        private readonly DwellingRegionProvider provider = new();

        private static RecordChunk Record(int length) => new("Dwelling", new byte[length], 500, 0);

        [Test]
        public void TheRecordOfADwellingIsDescribedFieldByFieldInTheOrderOfItsBytes()
        {
            var regions = this.provider.GetRegions(Record(150));

            Assert.That(regions.Select(r => (r.Name, r.Start, r.Length)), Is.EqualTo(new[]
            {
                ("Owner", 5, 1),
                ("Object type", 6, 1),
                ("Creature", 8, 4),
                ("Creatures to hire", 12, 4),
                ("Pos X", 20, 1),
                ("Pos Y", 21, 1),
                ("Pos Lvl", 22, 1),
            }));
        }

        [Test]
        public void TheCreatureAndTheStockAreCreatureFieldsAndThePlaceIsPosition()
        {
            var regions = this.provider.GetRegions(Record(150));

            Assert.That(regions.Where(r => r.Kind == ByteRegionKind.Creatures).Select(r => r.Name), Is.EqualTo(new[] { "Creature", "Creatures to hire" }));
            Assert.That(regions.Where(r => r.Kind == ByteRegionKind.Position).Select(r => r.Name), Is.EqualTo(new[] { "Pos X", "Pos Y", "Pos Lvl" }));
        }

        [Test]
        public void ARecordCutShortLeavesOutTheFieldsThatWouldRunPastItsEnd()
        {
            var regions = this.provider.GetRegions(Record(15));

            Assert.That(regions.Select(r => r.Name), Is.EqualTo(new[] { "Owner", "Object type", "Creature" }));
        }

        [Test]
        public void ARecordTooShortForAnyFieldHasNoRegions()
        {
            Assert.That(this.provider.GetRegions(Record(5)), Is.Empty);
        }
    }
}
