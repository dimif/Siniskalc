using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Homm3.Tests.Integration.Models;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// Tests what the artifacts a hero wears add up to on its pages.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class ArtifactEffectsViewModelTests
    {
        private Game game;

        private HeroViewModel hero;

        [SetUp]
        public void SetUp()
        {
            this.game = HeroEditingTests.LoadPrepared();
            this.hero = TestServices.CreateHeroViewModel(this.game.History.CreateHeroSession(this.game.History.Current.Heroes().FindByName("Christian")));
        }

        [TearDown]
        public void TearDown()
        {
            this.hero.Dispose();
        }

        [Test]
        public void GeneralPageNotesWhatArtifactsAddToSkillsAndMovement()
        {
            var general = this.hero.GeneralInfoViewModel;
            var baseAttack = general.PrimarySkills.Attack.Base;
            Assert.That(general.PrimarySkills.Attack.ArtifactNote, Is.Empty);

            this.hero.ArtifactsViewModel.SelectedHelm = Artifact.HelmOfHeavenlyEnlightenment.Name;

            Assert.That(general.PrimarySkills.Attack.ArtifactNote, Is.EqualTo("+6 from artifacts"));
            Assert.That(general.PrimarySkills.Attack.Total, Is.EqualTo(baseAttack + 6));
            Assert.That(general.LevelAndClass.NextLevelNote, Does.Contain("XP to level"));
            Assert.That(general.PrimarySkills.Knowledge.ArtifactNote, Does.Contain("from artifacts"));
            Assert.That(general.MovementAndSpellPoints.MovementNote, Is.Empty);
            Assert.That(general.MovementAndSpellPoints.MaxMovementText, Does.StartWith("Maximum "));

            this.game.History.Undo();

            Assert.That(general.PrimarySkills.Attack.ArtifactNote, Is.Empty);
        }

        [Test]
        public void EffectLinesListOnlyEffectsThatAreNotZero()
        {
            var vm = new EquippedArtifactsViewModel(TestServices.EffectLines, new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
            Assert.That(vm.TotalEffectLines, Is.Empty);
            Assert.That(vm.HasNoEffects, Is.True);

            vm.SelectedHelm = Artifact.ThunderHelmet.Name;

            Assert.That(vm.HasNoEffects, Is.False);
            Assert.That(vm.TotalEffectLines, Is.EqualTo(new[] { new EffectLine("Spell power", "-2"), new EffectLine("Knowledge", "+10") }));
        }

        [Test]
        public void ClearSlotCommandTakesTheArtifactOffAndMirroredSlotsGoToo()
        {
            var vm = new EquippedArtifactsViewModel(TestServices.EffectLines, new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
            vm.SelectedHelm = Artifact.AdmiralsHat.Name;

            Assert.That(vm.ClearSlotCommand.CanExecute(ArtifactSlots.Necklace), Is.True);
            Assert.That(vm.ClearSlotCommand.CanExecute(ArtifactSlots.Feet), Is.False);

            vm.ClearSlotCommand.Execute(ArtifactSlots.Necklace);

            Assert.That(vm.SelectedHelm, Is.Null);
            Assert.That(vm.SelectedNeck, Is.Null);
        }

        [Test]
        public void HoveredArtifactInfoComesAndGoes()
        {
            var vm = new EquippedArtifactsViewModel(TestServices.EffectLines, new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
            Assert.That(vm.HasSelectedInfo, Is.False);

            vm.ShowArtifactInfoByName(Artifact.PendantOfCourage.Name);
            Assert.That(vm.HasSelectedInfo, Is.True);
            Assert.That(vm.SelectedEffectLines, Is.EqualTo(new[] { new EffectLine("Morale", "+3"), new EffectLine("Luck", "+3") }));

            vm.ClearArtifactInfo();
            Assert.That(vm.HasSelectedInfo, Is.False);
        }
    }
}
