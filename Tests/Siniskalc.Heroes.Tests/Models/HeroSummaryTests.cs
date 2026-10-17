namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests what a list of heroes shows under the name of a hero.
    /// </summary>
    [TestFixture]
    internal sealed class HeroSummaryTests
    {
        [Test]
        public void TheDescriptionIsTheKindAndTheLevel()
        {
            var summary = new HeroSummary<ConsoleColor>(3, 17, "Gem", null, "Wizard", 12);

            Assert.That(summary.Description, Is.EqualTo("Wizard, level 12"));
        }
    }
}
