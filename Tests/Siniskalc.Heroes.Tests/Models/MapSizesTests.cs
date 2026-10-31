namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the names of the sizes a map can have.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapSizesTests
    {
        [TestCase(36, "Small (36 x 36)")]
        [TestCase(72, "Medium (72 x 72)")]
        [TestCase(108, "Large (108 x 108)")]
        [TestCase(144, "Extra large (144 x 144)")]
        [TestCase(180, "Huge (180 x 180)")]
        [TestCase(216, "Extra huge (216 x 216)")]
        [TestCase(252, "Giant (252 x 252)")]
        public void EveryStandardSizeHasItsName(int size, string name)
        {
            Assert.That(MapSizes.Describe(size), Is.EqualTo(name));
        }

        [TestCase(40, "40 x 40")]
        [TestCase(1, "1 x 1")]
        public void ASizeThatIsNotStandardIsTheNumberTwice(int size, string text)
        {
            Assert.That(MapSizes.Describe(size), Is.EqualTo(text));
        }

        [TestCase(0)]
        [TestCase(-36)]
        public void NoSizeIsNoText(int size)
        {
            Assert.That(MapSizes.Describe(size), Is.Empty);
        }
    }
}
