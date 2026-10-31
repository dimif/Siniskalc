using Moq;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The spells the artifacts a hero wears make available, as the spells page shows them, over a hero session with no savefile and
    /// the real resolver of the spells artifacts grant. Christian wears nothing and knows no spell.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class SpellsFromArtifactsViewModelTests
    {
        private FakeHeroSession session = null!;
        private HeroViewModel page = null!;

        [SetUp]
        public void StartWithChristianWearingNothing()
        {
            this.session = new FakeHeroSession(HeroModels.Christian(new GrantedSpellsResolver(GameVersion.HornOfTheAbyss)));
            this.page = new HeroViewModel(this.session, new EffectLineProvider(), new SpellPointsCalculatorFactory().Create(this.session.GameVersion), new Mock<IByteEditTargetFactory>().Object, openCastle: _ => { });
        }

        [TearDown]
        public void ClosePage()
        {
            this.page.Dispose();
        }

        private SpellRowViewModel Row(Spell spell) =>
            this.page.SpellsViewModel.Finder.Items.Cast<SpellRowViewModel>().Single(r => r.Spell == spell);

        [Test]
        public void ATomeMakesItsSchoolAvailableAndTakingItOffRemovesThem()
        {
            var before = this.page.SpellsViewModel.Finder.Summary;

            this.page.ArtifactsViewModel.SelectedItem1 = Artifact.TomeOfFireMagic.Name;

            Assert.That(this.Row(Spell.Fireball).IsAvailable, Is.True);
            Assert.That(this.Row(Spell.Armageddon).IsAvailable, Is.True);
            Assert.That(this.Row(Spell.Fireball).Source, Is.EqualTo(Artifact.TomeOfFireMagic.Name));
            Assert.That(this.page.SpellsViewModel.Finder.Summary, Is.Not.EqualTo(before));

            this.page.ArtifactsViewModel.SelectedItem1 = null;

            Assert.That(this.Row(Spell.Fireball).IsAvailable, Is.False);
            Assert.That(this.page.SpellsViewModel.Finder.Summary, Is.EqualTo(before));
        }

        [Test]
        public void EveryTomeAlsoGivesMagicArrowAndVisionsWhichBelongToNoSchool()
        {
            this.page.ArtifactsViewModel.SelectedItem1 = Artifact.TomeOfFireMagic.Name;

            Assert.That(this.Row(Spell.MagicArrow).IsAvailable, Is.True);
            Assert.That(this.Row(Spell.Visions).IsAvailable, Is.True);
        }

        [Test]
        public void ATomeDoesNotGiveASpellOfAnotherSchool()
        {
            this.page.ArtifactsViewModel.SelectedItem1 = Artifact.TomeOfFireMagic.Name;

            var otherSchool = Spell.All.First(sp => sp.School == SpellSchool.Water);

            Assert.That(this.Row(otherSchool).IsAvailable, Is.False);
        }

        [Test]
        public void ASpellTheHeroLearnedItselfStaysWhenTheTomeThatGaveItIsTakenOff()
        {
            this.session.Update(h => h with { SpellBook = h.SpellBook.WithLearned(Spell.Fireball, true) });
            this.page.ArtifactsViewModel.SelectedItem1 = Artifact.TomeOfFireMagic.Name;
            this.page.ArtifactsViewModel.SelectedItem1 = null;

            Assert.That(this.Row(Spell.Fireball).IsLearned, Is.True);
        }

        [Test]
        public void SpellbindersHatGivesEveryLevelFiveSpellAndPuttingTheHeroBackTakesThemBack()
        {
            this.page.ArtifactsViewModel.SelectedHelm = Artifact.SpellbindersHat.Name;

            var levelFive = this.page.SpellsViewModel.Finder.Items.Cast<SpellRowViewModel>().Where(r => r.Level == 5 && r.Spell != Spell.TitansLightningBolt && !r.IsLearned).ToList();
            Assert.That(levelFive, Is.Not.Empty);
            Assert.That(levelFive.All(r => r.IsAvailable), Is.True);

            this.session.Update(_ => this.session.Original);

            Assert.That(levelFive.Any(r => r.IsAvailable), Is.False);
        }

        [Test]
        public void ASpellFromAnotherSourceSurvivesSwappingArtifactsAndIsSaidToHaveAnUnknownSource()
        {
            var scrollSpell = Spell.All.First(sp => sp.School == SpellSchool.Wind && sp.Level > 2);
            this.session.Update(h => h with { SpellBook = h.SpellBook.WithOtherAvailable([scrollSpell]) });
            Assert.That(this.Row(scrollSpell).IsAvailable, Is.True);
            Assert.That(this.Row(scrollSpell).Source, Is.EqualTo("Unknown source"));

            this.page.ArtifactsViewModel.SelectedItem1 = Artifact.TomeOfWaterMagic.Name;
            this.page.ArtifactsViewModel.SelectedItem1 = null;

            Assert.That(this.Row(scrollSpell).IsAvailable, Is.True);
        }

        [Test]
        public void SeaCaptainsHatGrantsTheBoatSpells()
        {
            this.page.ArtifactsViewModel.SelectedHelm = Artifact.SeaCaptainsHat.Name;

            Assert.That(this.Row(Spell.SummonBoat).IsAvailable, Is.True);
            Assert.That(this.Row(Spell.ScuttleBoat).IsAvailable, Is.True);
        }
    }
}
