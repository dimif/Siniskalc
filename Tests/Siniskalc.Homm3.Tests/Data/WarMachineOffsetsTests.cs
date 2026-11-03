using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests where the record of a hero keeps each of its war machines.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class WarMachineOffsetsTests
    {
        [Test]
        public void EachWarMachineHasItsPlaceInTheHero()
        {
            Assert.That(WarMachineOffsets.For(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Ballista")), Is.EqualTo(HeroOffsetName.Ballista));
            Assert.That(WarMachineOffsets.For(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Cannon")), Is.EqualTo(HeroOffsetName.Cannon));
            Assert.That(WarMachineOffsets.For(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("AmmoCart")), Is.EqualTo(HeroOffsetName.AmmoCart));
            Assert.That(WarMachineOffsets.For(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("FirstAidTent")), Is.EqualTo(HeroOffsetName.FirstAidTent));
        }

        [Test]
        public void EveryWarMachineOfTheGameHasAPlace()
        {
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.All, Is.Not.Empty);
            foreach (var machine in TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.All)
            {
                Assert.DoesNotThrow(() => WarMachineOffsets.For(machine), machine.Name);
            }
        }
    }
}
