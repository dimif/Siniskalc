namespace Siniskalc.Homm2.Tests.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;

    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SpellTests
    {
        [Test]
        public void TheFirstFortyEightSpellsAreCombatSpellsAndTheRestAreAdventureSpells()
        {
            foreach (var spell in TestCatalog.Standard.Spells)
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
            Assert.That(TestCatalog.Standard.Spells.Single(s => s.Name == name).Type, Is.EqualTo(type));
        }

        [Test]
        public void TheMindSpellsAndTheElementalSpellsAreTheOnesTheRulesNameAndNoOthers()
        {
            Assert.That(TestCatalog.Standard.Spells.Where(s => s.Group == TestCatalog.Standard.SpellTypes["Mind"]), Is.EquivalentTo(new[] { TestCatalog.Standard.Spells["Blind"], TestCatalog.Standard.Spells["Paralyze"], TestCatalog.Standard.Spells["Berserker"], TestCatalog.Standard.Spells["Hypnotize"] }));
            Assert.That(
                TestCatalog.Standard.Spells.Where(s => s.Group == TestCatalog.Standard.SpellTypes["Elemental"]),
                Is.EquivalentTo(new[] { TestCatalog.Standard.Spells["LightningBolt"], TestCatalog.Standard.Spells["ChainLightning"], TestCatalog.Standard.Spells["ColdRay"], TestCatalog.Standard.Spells["ColdRing"], TestCatalog.Standard.Spells["Fireball"], TestCatalog.Standard.Spells["Fireblast"], TestCatalog.Standard.Spells["ElementalStorm"] }));
        }

        [Test]
        public void TheElementalAndHasteSpellsAreOnTheLevelsOfTheGame()
        {
            Assert.That(
                new[] { TestCatalog.Standard.Spells["Fireball"], TestCatalog.Standard.Spells["LightningBolt"], TestCatalog.Standard.Spells["ChainLightning"], TestCatalog.Standard.Spells["MassHaste"] }.Select(s => s.Level),
                Is.EqualTo(new[] { 3, 2, 4, 3 }));
        }

        [Test]
        public void EverySpellIsOnALevelFromOneToFive()
        {
            Assert.That(TestCatalog.Standard.Spells.Select(s => s.Level), Has.All.InRange(1, 5));
        }
    }
}
