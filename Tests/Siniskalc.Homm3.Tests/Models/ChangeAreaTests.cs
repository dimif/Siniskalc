using Siniskalc;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

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
