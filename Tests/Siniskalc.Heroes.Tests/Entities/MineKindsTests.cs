namespace Siniskalc.Heroes.Tests.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the names of the kinds of mines and the resource each one gives.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineKindsTests
    {
        [TestCase(MineKind.Sawmill, "Sawmill")]
        [TestCase(MineKind.AlchemistLab, "Alchemist's lab")]
        [TestCase(MineKind.OreMine, "Ore mine")]
        [TestCase(MineKind.SulfurMine, "Sulfur mine")]
        [TestCase(MineKind.CrystalMine, "Crystal mine")]
        [TestCase(MineKind.GemMine, "Gem mine")]
        [TestCase(MineKind.GoldMine, "Gold mine")]
        public void EveryKindHasItsName(MineKind kind, string name)
        {
            Assert.That(MineKinds.NameOf(kind), Is.EqualTo(name));
        }

        [TestCase(MineKind.Sawmill, Resource.Wood)]
        [TestCase(MineKind.AlchemistLab, Resource.Mercury)]
        [TestCase(MineKind.OreMine, Resource.Ore)]
        [TestCase(MineKind.SulfurMine, Resource.Sulfur)]
        [TestCase(MineKind.CrystalMine, Resource.Crystal)]
        [TestCase(MineKind.GemMine, Resource.Gems)]
        [TestCase(MineKind.GoldMine, Resource.Gold)]
        public void EveryKindGivesItsResource(MineKind kind, Resource resource)
        {
            Assert.That(MineKinds.ResourceOf(kind), Is.EqualTo(resource));
        }

        [Test]
        public void ACodeThatIsNoKindIsReadAsAGoldMine()
        {
            var unknown = (MineKind)99;

            Assert.That(MineKinds.NameOf(unknown), Is.EqualTo("Gold mine"));
            Assert.That(MineKinds.ResourceOf(unknown), Is.EqualTo(Resource.Gold));
        }

        [Test]
        public void TheNameAtAPlaceHasTheCoordinatesAndTheLevelOnlyForAMapWithLevels()
        {
            var place = new MapPosition(5, 6, 1);

            Assert.That(MineKinds.NameAt(MineKind.OreMine, place, true), Is.EqualTo("Ore mine at 5, 6, Underground"));
            Assert.That(MineKinds.NameAt(MineKind.OreMine, place, false), Is.EqualTo("Ore mine at 5, 6"));
        }
    }
}
