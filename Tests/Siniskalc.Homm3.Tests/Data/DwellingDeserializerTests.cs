using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests how the record of a dwelling is read, on a record made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class DwellingDeserializerTests
    {
        private readonly DwellingDeserializer deserializer = new();

        private static RecordChunk Record(int index = 0, params (int Offset, byte Value)[] bytes)
        {
            var record = new byte[DwellingLayout.RecordSize];
            foreach (var (offset, value) in bytes)
            {
                record[offset] = value;
            }

            return new RecordChunk($"Dwelling {index}", record, 1000, index);
        }

        [Test]
        public void TheIndexIsThatOfTheRecord()
        {
            Assert.That(this.deserializer.Deserialize(Record(index: 4)).Index, Is.EqualTo(4));
        }

        [Test]
        public void TheCreatureIsReadFromItsCodeAndTheCodeIsKept()
        {
            var model = this.deserializer.Deserialize(Record(0, (DwellingLayout.Creature, Creature.Centaur.Code)));

            Assert.That(model.CreatureCode, Is.EqualTo((int)Creature.Centaur.Code));
            Assert.That(model.Creature, Is.SameAs(Creature.Centaur));
        }

        [Test]
        public void ACodeThatIsNoCreatureGivesNoCreatureButKeepsTheCode()
        {
            var unknown = (byte)Enumerable.Range(0, 256).First(c => Creature.FromByte((byte)c) is null);

            var model = this.deserializer.Deserialize(Record(0, (DwellingLayout.Creature, unknown)));

            Assert.That(model.Creature, Is.Null);
            Assert.That(model.CreatureCode, Is.EqualTo((int)unknown));
        }

        [Test]
        public void TheOwnerIsTheColourOfItsCodeAndFFIsNeutral()
        {
            Assert.That(this.deserializer.Deserialize(Record(0, (DwellingLayout.Owner, PlayerColor.Blue.Code))).Owner, Is.SameAs(PlayerColor.Blue));
            Assert.That(this.deserializer.Deserialize(Record(0, (DwellingLayout.Owner, DwellingLayout.NoOwner))).Owner, Is.SameAs(PlayerColor.Neutral));
        }

        [Test]
        public void AnOwnerByteThatIsNoColourReadsAsNeutral()
        {
            var unknown = (byte)Enumerable.Range(0, 256).First(c => c != DwellingLayout.NoOwner && !PlayerColor.All.Any(p => p.Code == c));

            Assert.That(this.deserializer.Deserialize(Record(0, (DwellingLayout.Owner, unknown))).Owner, Is.SameAs(PlayerColor.Neutral));
        }

        [Test]
        public void ThePositionIsReadFromThreeSeparateBytes()
        {
            var model = this.deserializer.Deserialize(Record(0, (DwellingLayout.X, 31), (DwellingLayout.Y, 56), (DwellingLayout.Level, 1)));

            Assert.That(model.Position, Is.EqualTo(new MapPosition(31, 56, 1)));
        }

        [Test]
        public void TheStockIsAFourByteNumberWithTheLeastSignificantByteFirst()
        {
            var model = this.deserializer.Deserialize(Record(
                0,
                (DwellingLayout.Stock, 0x01),
                (DwellingLayout.Stock + 1, 0x02),
                (DwellingLayout.Stock + 2, 0x03),
                (DwellingLayout.Stock + 3, 0x80)));

            Assert.That(model.Stock, Is.EqualTo(0x80030201L));
        }
    }
}
