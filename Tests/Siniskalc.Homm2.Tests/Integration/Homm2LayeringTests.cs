namespace Siniskalc.Homm2.Tests.Integration
{
    /// <summary>
    /// Tests that the tests of a game need nothing of another game or of the application that hosts both.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class Homm2LayeringTests
    {
        [TestCase("Siniskalc.Homm3.dll")]
        [TestCase("Siniskalc.dll")]
        public void TheTestsOfTheGameDoNotLoad(string assembly) =>
            Assert.That(File.Exists(Path.Combine(AppContext.BaseDirectory, assembly)), Is.False, $"{assembly} is next to the tests of Homm2; they must reference only Siniskalc.Homm2.");
    }
}
