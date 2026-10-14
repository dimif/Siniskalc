namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the parts of a savegame an edit can belong to.
    /// </summary>
    [TestFixture]
    internal sealed class ChangeAreasTests
    {
        [Test]
        public void EveryChangeAreaHasItsOwnName()
        {
            Assert.That(ChangeAreas.All.Select(a => a.DisplayName), Is.Unique.And.None.Empty);
        }
    }
}
