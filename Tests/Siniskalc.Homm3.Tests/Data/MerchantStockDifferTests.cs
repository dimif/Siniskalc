using Siniskalc.Core.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the differ of the artifact merchants of the map lists, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MerchantStockDifferTests
    {
        private static MerchantStockModel Merchant() => new(3, "Artifact merchant at X: 5, Y: 6", [TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SkullHelmet"), null, null, null, null, null, null]);

        [Test]
        public void TwoEqualStocksHaveNoChanges()
        {
            Assert.That(new MerchantStockDiffer(Homm3ChunkKinds.Merchants).Diff(0, Merchant(), Merchant()), Is.Empty);
        }

        [Test]
        public void ASlotThatChangedIsOneChangeNamedByItsPlaceInTheShopAndSoldIsWorded()
        {
            var original = Merchant();
            var current = original.WithSlot(0, null).WithSlot(2, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("ThunderHelmet"));

            var changes = new MerchantStockDiffer(Homm3ChunkKinds.Merchants).Diff(3, original, current);

            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[]
            {
                ("Slot 1", "Skull Helmet", "(sold)"),
                ("Slot 3", "(sold)", TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("ThunderHelmet").Name),
            }));
            Assert.That(changes.Select(c => (c.OwnerName, c.Kind, c.Index)).Distinct(), Is.EqualTo(new[] { ("Artifact merchant at X: 5, Y: 6", Homm3ChunkKinds.Merchants, 3) }));
        }
    }
}
