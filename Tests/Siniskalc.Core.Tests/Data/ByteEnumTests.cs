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

        [Test]
        public void AByteThatIsNoValueReadsAsNothing()
        {
            Assert.That(ByteEnum.ReadOrNull<Level>(1), Is.EqualTo(Level.Advanced));
            Assert.That(ByteEnum.ReadOrNull<Level>(255), Is.Null);
        }

        [Test]
        public void ABytePutBackIsLeftAloneWhileItReadsAsTheValueAndWrittenWhenTheValueChanges()
        {
            Assert.That(ByteEnum.WriteKeeping<Level>(null, 200, 255), Is.EqualTo(200), "an unknown byte stays while the model has nothing");
            Assert.That(ByteEnum.WriteKeeping<Level>(Level.Expert, 200, 255), Is.EqualTo((byte)Level.Expert));
            Assert.That(ByteEnum.WriteKeeping<Level>(null, (byte)Level.Basic, 255), Is.EqualTo(255));
            Assert.That(ByteEnum.WriteKeeping<Level>(Level.Advanced, (byte)Level.Advanced, 255), Is.EqualTo((byte)Level.Advanced));
        }
    }
}
