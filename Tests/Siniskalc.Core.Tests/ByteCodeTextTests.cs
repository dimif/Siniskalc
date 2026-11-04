namespace Siniskalc.Core.Tests
{
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ByteCodeTextTests
    {
        [Test]
        public void ACodeIsWrittenAsTwoUpperCaseHexDigitsAfter0x()
        {
            Assert.That(ByteCodeText.Format(0x2A), Is.EqualTo("0x2A"));
            Assert.That(ByteCodeText.Format(0x05), Is.EqualTo("0x05"));
        }

        [TestCase("2a")]
        [TestCase("2A")]
        [TestCase("0x2a")]
        [TestCase("42")]
        public void AWordThatIsTheCodeInHexOrDecimalMatchesIt(string word)
        {
            Assert.That(ByteCodeText.Matches(0x2A, word), Is.True);
        }

        [TestCase("2")]
        [TestCase("A")]
        [TestCase("0x2")]
        [TestCase("helm")]
        [TestCase("")]
        [TestCase("300")]
        public void AWordThatIsOnlyPartOfTheCodeOrNoCodeDoesNotMatch(string word)
        {
            Assert.That(ByteCodeText.Matches(0x2A, word), Is.False);
        }
    }
}
