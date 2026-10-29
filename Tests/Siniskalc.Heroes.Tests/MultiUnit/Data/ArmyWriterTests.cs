namespace Siniskalc.Heroes.Tests.MultiUnit.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using static Siniskalc.Heroes.Tests.Data.ArmySamples;

    /// <summary>
    /// Tests how the army of a record is written: only what the model changes, and nothing the game could not hold.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class ArmyWriterTests
    {
        private static IArmy<Creature> Empty(int slots) => new Army<Creature>(slots);

        [Test]
        public void AStackIsWrittenIntoItsSlotAndTheOthersStayEmpty()
        {
            var bytes = Enumerable.Repeat((byte)255, 15).ToArray();

            ArmyWriter.Write(bytes, Narrow, Empty(5).WithSlot(1, Archer, 300), Numbering, "army");

            Assert.That(bytes[..5], Is.EqualTo(new byte[] { 255, 1, 255, 255, 255 }));
            Assert.That(BitConverter.ToUInt16(bytes, 7), Is.EqualTo(300));
        }

        [Test]
        public void AStackIsWrittenInFourByteTablesToo()
        {
            var bytes = Enumerable.Repeat((byte)255, 24).ToArray();

            ArmyWriter.Write(bytes, Wide, Empty(3).WithSlot(2, Archer, 70_000), Numbering, "army");

            Assert.That(BitConverter.ToUInt32(bytes, 8), Is.EqualTo(1u));
            Assert.That(BitConverter.ToUInt32(bytes, 20), Is.EqualTo(70_000u));
            Assert.That(BitConverter.ToUInt32(bytes, 0), Is.EqualTo(uint.MaxValue));
        }

        [Test]
        public void AnEmptiedSlotGetsTheEmptyTypeAndACountOfNothing()
        {
            var bytes = new byte[] { 1, 255, 255, 255, 255, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            ArmyWriter.Write(bytes, Narrow, Empty(5), Numbering, "army");

            Assert.That(bytes[0], Is.EqualTo(255));
            Assert.That(BitConverter.ToUInt16(bytes, 5), Is.EqualTo(0));
        }

        [Test]
        public void ACreatureTheGameDoesNotKnowKeepsItsNumberWhileTheModelStillHoldsItsStandIn()
        {
            var bytes = new byte[] { 77, 255, 255, 255, 255, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var army = Empty(5).WithSlot(0, Peasant, 5);

            ArmyWriter.Write(bytes, Narrow, army, Numbering, "army");

            Assert.That(bytes[0], Is.EqualTo(77));

            ArmyWriter.Write(bytes, Narrow, Empty(5).WithSlot(0, Archer, 5), Numbering, "army");

            Assert.That(bytes[0], Is.EqualTo(1));
        }

        [Test]
        public void ACountTheGameCannotHoldIsRefusedAndNothingIsCut()
        {
            var bytes = Enumerable.Repeat((byte)255, 15).ToArray();

            Assert.That(() => ArmyWriter.Write(bytes, Narrow, Empty(5).WithSlot(0, Archer, 1001), Numbering, "army"), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(bytes, Is.All.EqualTo(255));
        }

        [Test]
        public void AnArmyThatMatchesTheBytesLeavesEveryByteAlone()
        {
            var original = new byte[] { 77, 1, 255, 255, 255, 5, 0, 8, 0, 0, 0, 0, 0, 0, 0 };
            var bytes = (byte[])original.Clone();

            ArmyWriter.Write(bytes, Narrow, Empty(5).WithSlot(0, Peasant, 5).WithSlot(1, Archer, 8), Numbering, "army");

            Assert.That(bytes, Is.EqualTo(original));
        }

        [Test]
        public void ChangingOnlyTheCountOfAnUnknownCreatureKeepsItsNumber()
        {
            var bytes = new byte[] { 77, 255, 255, 255, 255, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            ArmyWriter.Write(bytes, Narrow, Empty(5).WithSlot(0, Peasant, 6), Numbering, "army");

            Assert.That(bytes, Is.EqualTo(new byte[] { 77, 255, 255, 255, 255, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
        }

        [Test]
        public void WhatTheReaderReadsWritesBackAsTheSameBytes()
        {
            var original = new byte[] { 77, 1, 255, 255, 255, 5, 0, 8, 0, 0, 0, 0, 0, 0, 0 };
            var bytes = (byte[])original.Clone();
            var army = ArmyReader.Read(new Siniskalc.Core.Data.RawBytesChunk("hero", bytes, 0), Narrow, Numbering);

            ArmyWriter.Write(bytes, Narrow, army, Numbering, "army");

            Assert.That(bytes, Is.EqualTo(original));
        }
    }
}
