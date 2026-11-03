namespace Siniskalc.Heroes.Tests
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Tests that what the games of the series share knows no game, and no UI.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroesLayeringTests
    {
        [Test]
        public void TheSharedLibraryReferencesNoGameAndNoUi()
        {
            var references = typeof(Resource).Assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();

            Assert.That(references.Where(r => r.StartsWith("Siniskalc.Homm") || r.StartsWith("Siniskalc.Wpf") || r.StartsWith("Siniskalc.Fheroes")), Is.Empty);
            Assert.That(references, Does.Not.Contain("PresentationFramework"));
            Assert.That(references, Does.Not.Contain("WindowsBase"));
        }
    }
}
