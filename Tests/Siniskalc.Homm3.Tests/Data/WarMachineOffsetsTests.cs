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
            Assert.That(WarMachineOffsets.For(WarMachine.Ballista), Is.EqualTo(HeroOffsetName.Ballista));
            Assert.That(WarMachineOffsets.For(WarMachine.Cannon), Is.EqualTo(HeroOffsetName.Cannon));
            Assert.That(WarMachineOffsets.For(WarMachine.AmmoCart), Is.EqualTo(HeroOffsetName.AmmoCart));
            Assert.That(WarMachineOffsets.For(WarMachine.FirstAidTent), Is.EqualTo(HeroOffsetName.FirstAidTent));
        }

        [Test]
        public void EveryWarMachineOfTheGameHasAPlace()
        {
            Assert.That(WarMachine.All, Is.Not.Empty);
            foreach (var machine in WarMachine.All)
            {
                Assert.DoesNotThrow(() => WarMachineOffsets.For(machine), machine.Name);
            }
        }
    }
}
