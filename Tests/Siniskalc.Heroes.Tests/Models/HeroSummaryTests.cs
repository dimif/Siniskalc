namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Tests.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests what a list of heroes shows under the name of a hero.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroSummaryTests
    {
        [Test]
        public void TheDescriptionIsTheKindAndTheLevel()
        {
            var summary = new HeroSummary<TestColor>(3, 17, "Gem", TestColor.Neutral, "Wizard", 12);

            Assert.That(summary.Description, Is.EqualTo("Wizard, Level 12"));
        }
    }
}
