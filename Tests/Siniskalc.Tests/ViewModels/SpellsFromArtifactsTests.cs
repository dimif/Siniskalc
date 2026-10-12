using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;
using Siniskalc.Tests.Models;

namespace Siniskalc.Tests.ViewModels
{
    [TestFixture]
    internal sealed class SpellsFromArtifactsTests
    {
        private Game game;
        private HeroViewModel hero;

        [SetUp]
        public void SetUp()
        {
            this.game = HeroEditingTests.LoadPrepared();
            this.hero = TestServices.CreateHeroViewModel(this.game.History.CreateHeroSession(this.game.History.Current.Heroes().FindByName("Christian")));

            // Start from a hero that wears nothing that grants spells.
            this.hero.ArtifactsViewModel.SelectedHelm = null;
            this.hero.ArtifactsViewModel.SelectedItem1 = null;

            // And knows no spells from artifacts, whatever the savegame had stored.
            this.hero.Session.Update(h => h with { SpellBook = h.SpellBook.WithOtherAvailable([]) });
        }

        [TearDown]
        public void TearDown()
        {
            this.hero.Dispose();
        }

        private SpellRowViewModel Row(Spell spell) =>
            this.hero.SpellsViewModel.Finder.Items.Cast<SpellRowViewModel>().Single(r => r.Spell == spell);

        [Test]
        public void ATomeMakesItsSchoolAvailableAndTakingItOffRemovesThem()
        {
            var before = this.hero.SpellsViewModel.Finder.Summary;

            this.hero.ArtifactsViewModel.SelectedItem1 = Artifact.TomeOfFireMagic.Name;

            Assert.That(this.Row(Spell.Fireball).IsAvailable, Is.True);
            Assert.That(this.Row(Spell.Armageddon).IsAvailable, Is.True);

            // Visions and Magic Arrow belong to no school, and every tome gives them (confirmed by the savefiles in TestData/Hota/Arts).
            Assert.That(this.Row(Spell.MagicArrow).IsAvailable, Is.True);
            Assert.That(this.Row(Spell.Visions).IsAvailable, Is.True);

            // A spell of another school is not given, unless the hero learned it.
            var otherSchool = Spell.All.First(sp => sp.School == SpellSchool.Water && !this.hero.HeroModel.SpellBook.IsLearned(sp));
            Assert.That(this.Row(otherSchool).IsAvailable, Is.False);
            Assert.That(this.hero.SpellsViewModel.Finder.Summary, Is.Not.EqualTo(before));

            this.hero.ArtifactsViewModel.SelectedItem1 = null;


            // What the tome gave is gone again, except the spells the hero learned itself.
            var notLearnedFire = Spell.All.First(sp => sp.School == SpellSchool.Fire && !this.hero.HeroModel.SpellBook.IsLearned(sp));
            Assert.That(this.Row(notLearnedFire).IsAvailable, Is.False);
            Assert.That(this.Row(Spell.MagicArrow).IsAvailable, Is.EqualTo(this.hero.HeroModel.SpellBook.IsLearned(Spell.MagicArrow)));
            Assert.That(this.hero.SpellsViewModel.Finder.Summary, Is.EqualTo(before));
        }

        [Test]
        public void SpellbindersHatGivesEveryLevelFiveSpellAndUndoTakesThemBack()
        {
            this.hero.ArtifactsViewModel.SelectedHelm = Artifact.SpellbindersHat.Name;

            var levelFive = this.hero.SpellsViewModel.Finder.Items.Cast<SpellRowViewModel>().Where(r => r.Level == 5 && r.Spell != Spell.TitansLightningBolt && !r.IsLearned).ToList();
            Assert.That(levelFive, Is.Not.Empty);
            Assert.That(levelFive.All(r => r.IsAvailable), Is.True);

            this.game.History.Undo();

            Assert.That(levelFive.Any(r => r.IsAvailable), Is.False);
        }

        [Test]
        public void SpellsFromOtherSourcesSurviveSwappingArtifacts()
        {
            // An available spell no worn artifact explains, such as one from a scroll. (Not Visions: every tome grants it, and the
            // savefile cannot tell a spell that is both granted and available for another reason from one that is only granted.)
            var scrollSpell = Spell.All.First(sp => sp.School == SpellSchool.Wind && sp.Level > 2 && !this.hero.HeroModel.SpellBook.IsLearned(sp));
            this.hero.Session.Update(h => h with { SpellBook = h.SpellBook.WithOtherAvailable([scrollSpell]) });
            Assert.That(this.Row(scrollSpell).IsAvailable, Is.True);
            Assert.That(this.Row(scrollSpell).Source, Is.EqualTo("Unknown source"));

            this.hero.ArtifactsViewModel.SelectedItem1 = Artifact.TomeOfWaterMagic.Name;
            this.hero.ArtifactsViewModel.SelectedItem1 = null;

            Assert.That(this.Row(scrollSpell).IsAvailable, Is.True);
        }

        [Test]
        public void SeaCaptainsHatGrantsTheBoatSpells()
        {
            this.hero.ArtifactsViewModel.SelectedHelm = Artifact.SeaCaptainsHat.Name;

            Assert.That(this.Row(Spell.SummonBoat).IsAvailable, Is.True);
            Assert.That(this.Row(Spell.ScuttleBoat).IsAvailable, Is.True);
        }
    }
}
