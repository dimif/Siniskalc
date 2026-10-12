using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Models
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
