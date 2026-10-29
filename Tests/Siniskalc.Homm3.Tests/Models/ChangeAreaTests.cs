using Siniskalc.Heroes.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    [TestFixture]
    internal sealed class ChangeAreaTests
    {
        [Test]
        public void EveryChangeAreaHasAName()
        {
            foreach (var area in ChangeAreas.All)
            {
                Assert.That(area.DisplayName, Is.Not.Empty, "an area has no name");
            }
        }
    }
}
