namespace Siniskalc.Core.Tests.Data
{
    using Siniskalc.Core.Data;

    [TestFixture]
    internal sealed class ByteEnumTests
    {
        private enum Level : byte
        {
            Basic,
            Advanced,
            Expert,
        }

        [Test]
        public void ByteEnumReadsDefinedValuesAndRefusesTheRest()
        {
            Assert.That(ByteEnum.TryRead<Level>((byte)Level.Expert, out var level), Is.True);
            Assert.That(level, Is.EqualTo(Level.Expert));
            Assert.That(ByteEnum.TryRead<Level>(200, out _), Is.False);
            Assert.That(ByteEnum.Write(Level.Advanced), Is.EqualTo((byte)Level.Advanced));
            Assert.Throws<ArgumentOutOfRangeException>(() => ByteEnum.Write((Level)99));
        }
    }
}
