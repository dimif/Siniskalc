namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the spells the castle page offers for each slot of the Mage Guild, on Xabran of Broken Alliance (save 24).
    /// </summary>
    [TestFixture]
    internal sealed class CastleMageGuildTests
    {
        [Test]
        public void ASlotOfAGuildLevelOffersNoSpellAndTheSpellsOfThatLevel()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var page = Xabran(game);

            for (var level = 1; level <= Constants.Castle.MageGuildLevels; level++)
            {
                var expected = Spell.All.Where(s => s.IsAvailableIn(game.GameVersion) && s.Level == level).Select(s => s.Name);
                foreach (var slot in page.GuildLevels[level - 1].Spells)
                {
                    Assert.That(slot.Choices[0], Is.EqualTo("(none)"));
                    Assert.That(slot.Choices.Skip(1), Is.SupersetOf(expected).And.Ordered);
                }
            }

            Assert.That(page.GuildLevels[0].Spells[0].Choices, Does.Contain("Magic Arrow").And.Not.Contain("Fireball"));
        }

        [Test]
        public void ASpellOfAnotherLevelThatTheSaveHasInASlotIsShownAndKept()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var index = game.History.Current.Castles().FindByName("Xabran");
            game.History.CreateCastleSession(index).Update(c => c.WithMageGuildSpell(1, 0, Spell.Armageddon));
            var loaded = TestServices.Parser.Parse(game.GetBytesToSave(), null);

            var slot = Xabran(loaded).GuildLevels[0].Spells[0];

            Assert.That((slot.Selected, slot.Choices.Contains("Armageddon")), Is.EqualTo(("Armageddon", true)));
            Assert.That(Xabran(loaded).GuildLevels[0].Spells[1].Choices, Does.Not.Contain("Armageddon"));
            Assert.That(loaded.History.GetPendingChanges(), Is.Empty);
        }

        private static CastleViewModel Xabran(Runtime.Game game) => new(game, game.History.Current.Castles().FindByName("Xabran"), _ => { });
    }
}
