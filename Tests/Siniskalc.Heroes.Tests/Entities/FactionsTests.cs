namespace Siniskalc.Heroes.Tests.Entities
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Tests the factions of a game whose factions are an enum.
    /// </summary>
    [TestFixture]
    internal sealed class FactionsTests
    {
        private enum Town : byte
        {
            Castle = 0,
            Swamp = 4,
        }

        [Test]
        public void EachMemberOfTheEnumIsAFactionWithItsNameAndCode()
        {
            Assert.That(Factions<Town>.All.Select(f => (f.Name, f.Code)), Is.EqualTo(new[] { ("Castle", (byte)0), ("Swamp", (byte)4) }));
            Assert.That(Factions<Town>.Of(Town.Swamp)!.Name, Is.EqualTo("Swamp"));
        }

        [Test]
        public void NoFactionIsNoFaction()
        {
            Assert.That(Factions<Town>.Of(null), Is.Null);
        }
    }
}
