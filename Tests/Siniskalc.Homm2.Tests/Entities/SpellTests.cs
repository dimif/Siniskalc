namespace Siniskalc.Homm2.Tests.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;
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

        [Test]
        public void EveryMageGuildLevelAndShrineCircleOfTheTestSavesHoldsSpellsOfItsLevel()
        {
            var wrong = new HashSet<string>();
            // The saves of TestData/Homm2/out were written by the editor to try its edits in the game (E18GUILD put Fireball, Lightning Bolt
            // and Chain Lightning in the guild of Xabran, T08SHRIN gave first circle shrines Fireball and Mass Haste), so they hold any spell
            // anywhere. Only the saves the game wrote say which level a spell is on.
            foreach (var path in Saves.All().Where(p => !p.Replace('\\', '/').Contains("/Homm2/out/", StringComparison.OrdinalIgnoreCase)))
            {
                var document = TestServices.Load(path).History.Current;
                foreach (var castle in document.Castles().GetModels().Where(c => c.IsUsed))
                {
                    for (var level = 1; level <= Constants.Castle.MageGuildLevels; level++)
                    {
                        for (var slot = 0; slot < Constants.Castle.MageGuildSlotsPerLevel; slot++)
                        {
                            if (castle.MageGuildSpell(level, slot) is { } spell && spell.Level != level)
                            {
                                wrong.Add($"guild level {level}: {spell.Name} (level {spell.Level})");
                            }
                        }
                    }
                }

                foreach (var shrine in document.Shrines().GetModels())
                {
                    if (shrine.Spell is { } spell && spell.Level != shrine.Circle)
                    {
                        wrong.Add($"shrine circle {shrine.Circle}: {spell.Name} (level {spell.Level})");
                    }
                }
            }

            Assert.That(wrong, Is.Empty);
        }
    }
}
