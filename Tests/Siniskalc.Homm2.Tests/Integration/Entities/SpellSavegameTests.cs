namespace Siniskalc.Homm2.Tests.Integration.Entities
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SpellSavegameTests
    {
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
