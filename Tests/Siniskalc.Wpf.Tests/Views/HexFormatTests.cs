namespace Siniskalc.Wpf.Tests.Views
{
    using Siniskalc.Wpf.Views;

    [TestFixture]
    internal sealed class HexFormatTests
    {
        [TestCase((byte)'A', 'A')]
        [TestCase((byte)' ', ' ')]
        [TestCase((byte)'~', '~')]
        [TestCase((byte)31, '.')]
        [TestCase((byte)127, '.')]
        [TestCase((byte)255, '.')]
        public void TheAsciiPreviewShowsPrintableBytesAndADotForTheRest(byte value, char expected)
        {
            Assert.That(HexFormat.ToAsciiPreview(value), Is.EqualTo(expected));
        }

        [TestCase((byte)0, "\\0")]
        [TestCase((byte)9, "\\t")]
        [TestCase((byte)10, "\\n")]
        [TestCase((byte)13, "\\r")]
        [TestCase((byte)'z', "z")]
        [TestCase((byte)200, ".")]
        public void ACharacterIsDescribedByItsEscapeWhenItHasOne(byte value, string expected)
        {
            Assert.That(HexFormat.DescribeChar(value), Is.EqualTo(expected));
        }
    }
}
