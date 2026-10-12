using Siniskalc.Core.Data;
using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Edits of the spells of a Mage Guild on the Bulwark savefile in which "Jotunngard" holds, on level 1, Protection from Water,
    /// Magic Arrow, View Air, Bless and Summon Boat, and on level 2 Ice Bolt, Disguise, Remove Obstacle and Lightning Bolt.
    /// </summary>
    [TestFixture]
    internal sealed class MageGuildSpellEditingTests
    {
        private SaveDocumentHistory history = null!;
        private CastleEditSession session = null!;
        private byte[] original = null!;

        [SetUp]
        public void Load()
        {
            var path = CastleSamples.Folder("Bulwark").Single(p => p.EndsWith("bulwark_33_mage_guild_5__nothing.GM1"));
            this.history = CastleSamples.Load(path).History;
            this.original = this.history.Current.ToBytes();
            this.session = this.history.CreateCastleSession(this.history.Current.Castles().FindByName("Jotunngard"));
        }

        private CastleModel Reloaded()
        {
            var game = TestServices.GameService.LoadGameFromBytes(this.history.Current.ToBytes(), "edited.GM1");
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName("Jotunngard")]);
        }

        private static string[] Names(CastleModel castle, int level) => [.. castle.MageGuildSpells[level - 1].Select(s => s.Name)];

        [Test]
        public void ASpellOfTheGuildCanBeReplacedAndTheChangeIsPendingRevertsAndSurvivesSaving()
        {
            this.session.Update(c => CastleBuilder.SetMageGuildSpell(c, 1, 0, Spell.Haste));

            var change = this.history.GetPendingChanges().Single();
            Assert.That((change.Path, change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Jotunngard › Spells", "Mage Guild level 1, spell 1", "Protection from Water", "Haste")));
            Assert.That(Names(this.Reloaded(), 1), Is.EqualTo(new[] { "Haste", "Magic Arrow", "View Air", "Bless", "Summon Boat" }));

            change.Revert(this.history);
            Assert.That(this.history.GetPendingChanges(), Is.Empty);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void ChoosingASpellTheGuildHoldsSwapsTheTwoSlotsAndEachChangeRevertsOnItsOwn()
        {
            this.session.Update(c => CastleBuilder.SetMageGuildSpell(c, 1, 0, Spell.Bless));

            Assert.That(Names(this.Reloaded(), 1), Is.EqualTo(new[] { "Bless", "Magic Arrow", "View Air", "Protection from Water", "Summon Boat" }));

            var changes = this.history.GetPendingChanges();
            Assert.That(changes, Has.Count.EqualTo(2));

            changes[0].Revert(this.history);
            changes[1].Revert(this.history);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void OnlyTheVisibleSlotsAreWrittenAndTheOnesAfterThemAreLeftAlone()
        {
            this.session.Update(c => CastleBuilder.SetMageGuildSpell(c, 2, 3, Spell.Precision));

            var before = CastleSamples.Load(CastleSamples.Folder("Bulwark").Single(p => p.EndsWith("bulwark_33_mage_guild_5__nothing.GM1"))).History.Current.ToBytes();
            var after = this.history.Current.ToBytes();
            var changed = Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).ToList();

            Assert.That(changed, Has.Count.GreaterThan(0).And.Count.LessThanOrEqualTo(4));
            Assert.That(Names(this.Reloaded(), 2), Is.EqualTo(new[] { "Ice Bolt", "Disguise", "Remove Obstacle", "Precision" }));
        }

        [Test]
        public void ASpellOfAnotherLevelIsRefused()
        {
            var castle = this.session.Current;

            Assert.That(() => CastleBuilder.SetMageGuildSpell(castle, 1, 0, Spell.Fireball), Throws.TypeOf<GameRuleException>());
        }

        [Test]
        public void TitansLightningBoltCannotBeInAGuild()
        {
            var castle = this.session.Current;

            Assert.That(castle.MageGuildSpells[4], Is.Empty, "Bulwark has no level 5 guild, so the level 5 test below uses another castle");
            var still = this.history.CreateCastleSession(this.history.Current.Castles().FindByName("Still Water")).Current;
            Assert.That(() => CastleBuilder.SetMageGuildSpell(still, 5, 0, Spell.TitansLightningBolt), Throws.TypeOf<GameRuleException>());
        }

        [TestCase(0, 0)]
        [TestCase(6, 0)]
        [TestCase(1, 5)]
        [TestCase(1, -1)]
        public void ASlotThatDoesNotExistIsRefused(int level, int slot)
        {
            var castle = this.session.Current;

            Assert.That(() => CastleBuilder.SetMageGuildSpell(castle, level, slot, Spell.MagicArrow), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ReplacingTheSameSpellChangesNothing()
        {
            this.session.Update(c => CastleBuilder.SetMageGuildSpell(c, 1, 1, Spell.MagicArrow));

            Assert.That(this.history.GetPendingChanges(), Is.Empty);
        }
    }
}
