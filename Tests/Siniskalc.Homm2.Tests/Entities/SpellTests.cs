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

        [Test]
        public void TheMindSpellsAndTheElementalSpellsAreTheOnesTheRulesNameAndNoOthers()
        {
            Assert.That(Spell.All.Where(s => s.Group == SpellGroup.Mind), Is.EquivalentTo(new[] { Spell.Blind, Spell.Paralyze, Spell.Berserker, Spell.Hypnotize }));
            Assert.That(
                Spell.All.Where(s => s.Group == SpellGroup.Elemental),
                Is.EquivalentTo(new[] { Spell.LightningBolt, Spell.ChainLightning, Spell.ColdRay, Spell.ColdRing, Spell.Fireball, Spell.Fireblast, Spell.ElementalStorm }));
        }

        [Test]
        public void EverySpellIsOnALevelFromOneToFive()
        {
            Assert.That(Spell.All.Select(s => s.Level), Has.All.InRange(1, 5));
        }
    }
}
