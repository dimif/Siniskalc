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
    /// What the general tab of the hero page says of the artifacts the hero wears, over a hero session with no savefile.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class GeneralTabArtifactNotesTests
    {
        private FakeHeroSession session = null!;
        private HeroViewModel page = null!;

        [SetUp]
        public void StartWithChristianWearingNothing()
        {
            this.session = new FakeHeroSession(HeroModels.Christian());
            this.page = new HeroViewModel(this.session, new EffectLineProvider(), new SpellPointsCalculatorFactory().Create(this.session.GameVersion), new Mock<IByteEditTargetFactory>().Object, MapPlaces.None, openCastle: _ => { });
        }

        [TearDown]
        public void ClosePage()
        {
            this.page.Dispose();
        }

        [Test]
        public void ANoteSaysWhatTheArtifactsAddToASkillAndTheTotalIncludesIt()
        {
            var attack = this.page.GeneralInfoViewModel.PrimarySkills.Attack;
            Assert.That(attack.ArtifactNote, Is.Empty);

            this.page.ArtifactsViewModel.SelectedHelm = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HelmOfHeavenlyEnlightenment").Name;

            Assert.That(attack.ArtifactNote, Is.EqualTo("+6 from artifacts"));
            Assert.That(attack.Total, Is.EqualTo(attack.Base + 6));
            Assert.That(this.page.GeneralInfoViewModel.PrimarySkills.Knowledge.ArtifactNote, Does.Contain("from artifacts"));
        }

        [Test]
        public void TheNoteGoesWhenTheHeroIsPutBack()
        {
            this.page.ArtifactsViewModel.SelectedHelm = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HelmOfHeavenlyEnlightenment").Name;

            this.session.Update(_ => this.session.Original);

            Assert.That(this.page.GeneralInfoViewModel.PrimarySkills.Attack.ArtifactNote, Is.Empty);
        }

        [Test]
        public void TheMovementNoteIsEmptyAndTheMaximumIsSaidWhenNoArtifactChangesMovement()
        {
            var points = this.page.GeneralInfoViewModel.MovementAndSpellPoints;

            Assert.That(points.MovementNote, Is.Empty);
            Assert.That(points.MaxMovementText, Does.StartWith("Maximum "));
        }

        [Test]
        public void TheLevelSaysHowManyExperiencePointsTheNextLevelNeeds()
        {
            Assert.That(this.page.GeneralInfoViewModel.LevelAndClass.NextLevelNote, Does.Contain("XP to level"));
        }
    }
}
