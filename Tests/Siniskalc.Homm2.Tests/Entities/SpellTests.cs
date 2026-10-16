namespace Siniskalc.Homm2.Tests.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;

    [TestFixture]
    internal sealed class SpellTests
    {
        [Test]
        public void TheFirstFortyEightSpellsAreCombatSpellsAndTheRestAreAdventureSpells()
        {
            foreach (var spell in Spell.All)
            {
                var expected = spell.Code <= 48 ? SpellType.Combat : SpellType.Adventure;

                Assert.That(spell.Type, Is.EqualTo(expected), spell.Name);
            }
        }

        [TestCase("Earthquake", SpellType.Combat)]
        [TestCase("Teleport", SpellType.Combat)]
        [TestCase("View Mines", SpellType.Adventure)]
        [TestCase("Dimension Door", SpellType.Adventure)]
        [TestCase("Set Water Guardian", SpellType.Adventure)]
        public void ASpellKnownFromTheGameIsCastWhereTheGameSaysItIs(string name, SpellType type)
        {
            Assert.That(Spell.All.Single(s => s.Name == name).Type, Is.EqualTo(type));
        }
    }
}
