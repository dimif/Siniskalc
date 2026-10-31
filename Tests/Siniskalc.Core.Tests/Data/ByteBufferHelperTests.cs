namespace Siniskalc.Core.Tests.Data
{
    using System;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Tests reading and writing numbers at an offset of a buffer, always least significant byte first.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ByteBufferHelperTests
    {
        [Test]
        public void NumbersAreReadLeastSignificantByteFirstFromAnOffset()
        {
            byte[] buffer = [0xAA, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];

            Assert.That(ByteBufferHelper.ReadUInt32(buffer, 1), Is.EqualTo(0x04030201u));
            Assert.That(ByteBufferHelper.ReadUInt64(buffer, 1), Is.EqualTo(0x0807060504030201UL));
        }

        [Test]
        public void NumbersAreWrittenLeastSignificantByteFirstAtAnOffsetAndNothingElseChanges()
        {
            var buffer = new byte[10];

            ByteBufferHelper.WriteUInt16(buffer, 1, 0x0201);
            ByteBufferHelper.WriteInt16(buffer, 3, -2);
            ByteBufferHelper.WriteUInt32(buffer, 5, 0x08070605);

            Assert.That(buffer, Is.EqualTo(new byte[] { 0, 0x01, 0x02, 0xFE, 0xFF, 0x05, 0x06, 0x07, 0x08, 0 }));
        }

        [Test]
        public void ASignedThirtyTwoBitNumberIsWrittenInTwosComplement()
        {
            var buffer = new byte[6];

            ByteBufferHelper.WriteInt32(buffer, 1, -2);

            Assert.That(buffer, Is.EqualTo(new byte[] { 0, 0xFE, 0xFF, 0xFF, 0xFF, 0 }));
        }

        [Test]
        public void ASixtyFourBitNumberIsWrittenInEightBytes()
        {
            var buffer = new byte[10];

            ByteBufferHelper.WriteUInt64(buffer, 1, 0x0807060504030201UL);

            Assert.That(buffer, Is.EqualTo(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 0 }));
        }

        [Test]
        public void WhatIsWrittenIsReadBack()
        {
            var buffer = new byte[12];

            ByteBufferHelper.WriteUInt32(buffer, 2, 0xDEADBEEF);
            ByteBufferHelper.WriteUInt64(buffer, 4, 0x1122334455667788UL);

            Assert.That(ByteBufferHelper.ReadUInt64(buffer, 4), Is.EqualTo(0x1122334455667788UL));
            Assert.That(ByteBufferHelper.ReadUInt32(buffer, 4), Is.EqualTo(0x55667788u));
        }

        [Test]
        public void ReadingOrWritingPastTheEndOfTheBufferIsRefused()
        {
            var buffer = new byte[4];

            Assert.That(() => ByteBufferHelper.ReadUInt32(buffer, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => ByteBufferHelper.ReadUInt64(buffer, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => ByteBufferHelper.WriteUInt16(buffer, 3, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => ByteBufferHelper.WriteUInt32(buffer, 1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }
}
