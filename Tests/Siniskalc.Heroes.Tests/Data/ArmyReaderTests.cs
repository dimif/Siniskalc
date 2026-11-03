namespace Siniskalc.Heroes.Tests.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using static Siniskalc.Heroes.Tests.Data.ArmySamples;

    /// <summary>
    /// Tests how the army of a record is read, in the width each game keeps its tables.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ArmyReaderTests
    {
        [Test]
        public void StacksAreReadFromTablesOfOneByteTypesAndTwoByteCounts()
        {
            var chunk = new RawBytesChunk("hero", [1, 0, 255, 255, 255, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0], 0);

            var army = ArmyReader.Read(chunk, Narrow, Numbering);

            Assert.That(army.Slots, Is.EqualTo(new[] { new ArmySlot<Creature>(Archer, 7), new ArmySlot<Creature>(Peasant, 0), null, null, null }));
        }

        [Test]
        public void StacksAreReadFromTablesOfFourByteTypesAndCounts()
        {
            var bytes = new byte[24];
            bytes[0] = 1;
            Array.Fill(bytes, (byte)255, 4, 8);
            bytes[12] = 9;
            var army = ArmyReader.Read(new RawBytesChunk("castle", bytes, 0), Wide, Numbering);

            Assert.That(army.Slots, Is.EqualTo(new[] { new ArmySlot<Creature>(Archer, 9), null, null }));
        }

        [Test]
        public void ACreatureTheGameDoesNotKnowReadsAsTheFirstKnownOneAndKeepsItsCount()
        {
            var chunk = new RawBytesChunk("hero", [77, 255, 255, 255, 255, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0], 0);

            var army = ArmyReader.Read(chunk, Narrow, Numbering);

            Assert.That(army.Slots[0], Is.EqualTo(new ArmySlot<Creature>(Peasant, 5)));
        }

        [Test]
        public void AnEmptyTypeIsEveryBitSetWhateverTheWidth()
        {
            Assert.That(Narrow.EmptyType, Is.EqualTo(0xFFu));
            Assert.That(Wide.EmptyType, Is.EqualTo(uint.MaxValue));
        }
    }
}
