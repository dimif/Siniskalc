using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests how the list of seven slots of a merchant is read, on a list made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MerchantStockDeserializerTests
    {
        private static RecordChunk List(params uint[] codes)
        {
            var bytes = new byte[MerchantLayout.ListSize];
            for (var slot = 0; slot < MerchantLayout.SlotCount; slot++)
            {
                BitConverter.GetBytes(slot < codes.Length ? codes[slot] : MerchantLayout.Empty).CopyTo(bytes, MerchantLayout.SlotAt(slot));
            }

            return new RecordChunk("Merchant 3", bytes, 100, 3);
        }

        [Test]
        public void TheIndexAndTheNameAreThoseOfTheRecord()
        {
            var model = new MerchantStockDeserializer(TestCatalog.For(GameVersion.HornOfTheAbyss)).Deserialize(List());

            Assert.That((model.Index, model.Name), Is.EqualTo((3, "Merchant 3")));
        }

        [Test]
        public void EachSlotIsReadFromItsFourBytesAndFFFFFFFFIsEmpty()
        {
            var model = new MerchantStockDeserializer(TestCatalog.For(GameVersion.HornOfTheAbyss)).Deserialize(List(0x13, MerchantLayout.Empty, 0x7E));

            Assert.That(model.Slots, Has.Count.EqualTo(7));
            Assert.That(model.Slots[0], Is.SameAs(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HelmOfTheAlabasterUnicorn")));
            Assert.That(model.Slots[1], Is.Null);
            Assert.That(model.Slots[2], Is.SameAs(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("OrbOfInhibition")));
            Assert.That(model.Slots.Skip(3), Is.All.Null);
        }

        [Test]
        public void ACodeThatIsNoArtifactReadsAsTheCodeZeroArtifactAndIsNotEmpty()
        {
            var unknown = (uint)Enumerable.Range(0, 256).First(c => !TestCatalog.For(GameVersion.HornOfTheAbyss).WearableArtifacts.ToDictionary(a => a.Code).ContainsKey((byte)c));
            var first = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.Find(0);

            var model = new MerchantStockDeserializer(TestCatalog.For(GameVersion.HornOfTheAbyss)).Deserialize(List(unknown, 0x100, 0x17E, 0xFFFFFFFE));

            Assert.That(model.Slots.Take(4), Is.All.SameAs(first));
        }
    }
}
