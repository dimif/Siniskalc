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
            this.page = new HeroViewModel(this.session, new EffectLineProvider(), new SpellPointsCalculatorFactory().Create(this.session.GameVersion), new Mock<IByteEditTargetFactory>().Object, MapPlaces.None, openCastle: _ => { });
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

            this.page.ArtifactsViewModel.SelectedItem1 = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic").Name;

            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")).IsAvailable, Is.True);
            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Armageddon")).IsAvailable, Is.True);
            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")).Source, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic").Name));
            Assert.That(this.page.SpellsViewModel.Finder.Summary, Is.Not.EqualTo(before));

            this.page.ArtifactsViewModel.SelectedItem1 = null;

            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")).IsAvailable, Is.False);
            Assert.That(this.page.SpellsViewModel.Finder.Summary, Is.EqualTo(before));
        }

        [Test]
        public void EveryTomeAlsoGivesMagicArrowAndVisionsWhichBelongToNoSchool()
        {
            this.page.ArtifactsViewModel.SelectedItem1 = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic").Name;

            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")).IsAvailable, Is.True);
            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions")).IsAvailable, Is.True);
        }

        [Test]
        public void ATomeDoesNotGiveASpellOfAnotherSchool()
        {
            this.page.ArtifactsViewModel.SelectedItem1 = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic").Name;

            var otherSchool = TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.All.First(sp => sp.School == TestCatalog.For(GameVersion.HornOfTheAbyss).SpellSchools.ById("water"));

            Assert.That(this.Row(otherSchool).IsAvailable, Is.False);
        }

        [Test]
        public void ASpellTheHeroLearnedItselfStaysWhenTheTomeThatGaveItIsTakenOff()
        {
            this.session.Update(h => h with { SpellBook = h.SpellBook.WithLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball"), true) });
            this.page.ArtifactsViewModel.SelectedItem1 = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic").Name;
            this.page.ArtifactsViewModel.SelectedItem1 = null;

            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")).IsLearned, Is.True);
        }

        [Test]
        public void SpellbindersHatGivesEveryLevelFiveSpellAndPuttingTheHeroBackTakesThemBack()
        {
            this.page.ArtifactsViewModel.SelectedHelm = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SpellbindersHat").Name;

            var levelFive = this.page.SpellsViewModel.Finder.Items.Cast<SpellRowViewModel>().Where(r => r.Level == 5 && r.Spell != TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("TitansLightningBolt") && !r.IsLearned).ToList();
            Assert.That(levelFive, Is.Not.Empty);
            Assert.That(levelFive.All(r => r.IsAvailable), Is.True);

            this.session.Update(_ => this.session.Original);

            Assert.That(levelFive.Any(r => r.IsAvailable), Is.False);
        }

        [Test]
        public void ASpellFromAnotherSourceSurvivesSwappingArtifactsAndIsSaidToHaveAnUnknownSource()
        {
            var scrollSpell = TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.All.First(sp => sp.School == TestCatalog.For(GameVersion.HornOfTheAbyss).SpellSchools.ById("wind") && sp.Level > 2);
            this.session.Update(h => h with { SpellBook = h.SpellBook.WithOtherAvailable([scrollSpell]) });
            Assert.That(this.Row(scrollSpell).IsAvailable, Is.True);
            Assert.That(this.Row(scrollSpell).Source, Is.EqualTo("Unknown source"));

            this.page.ArtifactsViewModel.SelectedItem1 = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfWaterMagic").Name;
            this.page.ArtifactsViewModel.SelectedItem1 = null;

            Assert.That(this.Row(scrollSpell).IsAvailable, Is.True);
        }

        [Test]
        public void SeaCaptainsHatGrantsTheBoatSpells()
        {
            this.page.ArtifactsViewModel.SelectedHelm = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SeaCaptainsHat").Name;

            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("SummonBoat")).IsAvailable, Is.True);
            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("ScuttleBoat")).IsAvailable, Is.True);
        }
    }
}
