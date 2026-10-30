using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Tests the regions the hex tab shows for the record of a castle.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleRegionProviderTests
    {
        [Test]
        public void EveryCastleByteBeforeTheNameIsAccountedFor()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "castles_00_baseline.GM1"));
            var castle = game.History.Current.Castles()[0];
            var regions = TestServices.CastleRegions.GetRegions(castle);

            var prefix = regions.Where(r => r.Start >= CastleLayout.NamePrefix && r.End <= CastleLayout.NameLength).Select(r => (r.Name, r.Start)).ToList();
            Assert.That(prefix, Is.EqualTo(new[]
            {
                ("Garrisoned hero ID", 65), ("Visiting hero ID", 66), ("Mage guild level", 67), ("Unused, always 0", 68),
            }));
            Assert.That(regions.Single(r => r.Name == "Creatures available").Elements[0].Name, Is.EqualTo("Level 1"));
        }
    }
}
