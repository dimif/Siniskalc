namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Discovery and preservation of the controlled mine guardian saves.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MineGuardianTests
    {
        [TestCase("mine_guardians_00_baseline", 0, 0xFF, 0)]
        [TestCase("mine_guardians_01_earth", 0, 62, 20)]
        [TestCase("mine_guardians_02_air", 0, 63, 20)]
        [TestCase("mine_guardians_03_fire", 0, 64, 20)]
        [TestCase("mine_guardians_04_water", 0, 65, 20)]
        [TestCase("mine_guardians_05_earth_spell_power_10", 0, 62, 40)]
        [TestCase("mine_guardians_06_before_orange_capture", 0, 62, 20)]
        [TestCase("mine_guardians_07_orange_captures_mine", 4, 0xFF, 0)]
        public void TheGoldMineIsFoundWithItsOwnerAndItsGuardianBytesArePreserved(string save, int owner, int spell, int count)
        {
            var mines = TestServices.LoadNamed("MineGuardians", save).History.Current.Mines();
            var chunk = mines.Single(m => m.Position == new MapPosition(43, 23));
            var index = mines.ToList().IndexOf(chunk);
            var model = mines.GetModel(index);
            var original = new byte[] { (byte)owner, 6, (byte)spell, (byte)count, 43, 23, 0xFF };
            var serializer = new SafeMineSerializer();

            Assert.That(model.Owner.Code, Is.EqualTo(owner));
            Assert.That(chunk.StartIndex, Is.EqualTo(0x568A));
            Assert.That(chunk.ToArray(), Is.EqualTo(original));
            Assert.That(serializer.Serialize(chunk, model).ToArray(), Is.EqualTo(original));

            var changed = serializer.Serialize(chunk, model with { Owner = PlayerColor.Green });
            original[0] = PlayerColor.Green.Code;
            Assert.That(changed.ToArray(), Is.EqualTo(original));
        }
    }
}
