namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the colour each resource is drawn in.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ResourcePaletteTests
    {
        [TestCase("Gold", "#B8902A")]
        [TestCase("Wood", "#6B4A2B")]
        [TestCase("Mercury", "#6D7A8C")]
        [TestCase("Ore", "#4E5560")]
        [TestCase("Sulfur", "#A59A22")]
        [TestCase("Crystal", "#A0356A")]
        [TestCase("Gems", "#2B8A78")]
        public void EveryResourceHasItsColour(string name, string hex)
        {
            Assert.That(ResourcePalette.Of(name), Is.EqualTo(hex));
        }

        [Test]
        public void TheNameIsMatchedWhateverTheCase()
        {
            Assert.That(ResourcePalette.Of("gold"), Is.EqualTo(ResourcePalette.Of("GOLD")));
            Assert.That(ResourcePalette.Of("gold"), Is.Not.EqualTo(ResourcePalette.Unknown));
        }

        [Test]
        public void ANameThatIsNoResourceGetsTheColourOfWhatIsUnknown()
        {
            Assert.That(ResourcePalette.Of("Mana"), Is.EqualTo("#8A93A6"));
            Assert.That(ResourcePalette.Of(string.Empty), Is.EqualTo(ResourcePalette.Unknown));
        }

        [Test]
        public void EveryResourceOfTheGameHasAColourOfItsOwn()
        {
            var colors = ResourceSet.All.Select(r => ResourcePalette.Of(r.ToString())).ToList();

            Assert.That(colors, Is.Unique);
            Assert.That(colors, Does.Not.Contain(ResourcePalette.Unknown));
        }
    }
}
