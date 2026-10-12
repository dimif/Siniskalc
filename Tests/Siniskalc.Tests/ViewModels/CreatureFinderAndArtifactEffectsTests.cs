using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;
using Siniskalc.Tests.Models;

namespace Siniskalc.Tests.ViewModels
{
    [TestFixture]
    internal sealed class CreatureFinderAndArtifactEffectsTests
    {
        private Game game;
        private HeroViewModel hero;

        private ArmyViewModel Creatures => this.hero.ArmyViewModel;

        private CreatureFinderViewModel Finder => this.hero.ArmyViewModel.Finder;

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

        private static string[] Names(CreatureFinderViewModel vm) =>
            vm.Items.Cast<CreatureChoiceViewModel>().Select(c => c.Name).ToArray();

        [Test]
        public void FinderListsEveryCreatureUntilNarrowed()
        {
            Assert.That(this.Finder.Items.Count, Is.EqualTo(Creature.All.Count(c => c.IsAvailableIn(this.game.GameVersion))));
            Assert.That(this.Finder.Summary, Does.Contain("creatures"));
        }

        [Test]
        public void FinderNarrowsByEveryWordOfNameTownLevelAndForm()
        {
            this.Finder.Filter = "dragon";
            Assert.That(Names(this.Finder), Does.Contain("Green Dragon").And.Contain("Azure Dragon").And.Not.Contain("Pikeman"));

            this.Finder.Filter = "dragon rampart";
            Assert.That(Names(this.Finder), Is.EquivalentTo(new[] { "Green Dragon", "Gold Dragon" }));

            this.Finder.Filter = "level 1";
            Assert.That(Names(this.Finder), Does.Contain("Pikeman").And.Contain("Halberdier").And.Not.Contain("Angel"));
            Assert.That(this.Finder.Summary, Does.Contain("showing"));

            this.Finder.Filter = "castle upgraded level 7";
            Assert.That(Names(this.Finder), Is.EqualTo(new[] { "Archangel" }));
        }

        [Test]
        public void FinderRowsShowTheStatsOfTheCreatureForTheGameVersion()
        {
            var angel = this.Finder.Items.Cast<CreatureChoiceViewModel>().Single(c => c.Name == "Angel");
            var info = Creature.Angel.GetInfo(this.game.GameVersion);

            Assert.That(angel.Attack, Is.EqualTo(info.Atk));
            Assert.That(angel.Health, Is.EqualTo(info.Health));
            Assert.That(angel.Cost, Is.EqualTo(info.Gold));
            Assert.That(angel.DamageRange, Is.EqualTo(info.MinDamage == info.MaxDamage ? $"{info.MinDamage}" : $"{info.MinDamage}-{info.MaxDamage}"));
            Assert.That(angel.Details, Does.Contain("Angel").And.Contain("gold"));
            Assert.That(this.Finder.Columns.Select(c => c.Header), Does.Contain("HP").And.Not.Contain("Form"));
        }

        [Test]
        public void SlotsShowTheTotalsOfTheWholeStackAndTheyFollowTheAmount()
        {
            var slot = this.Creatures.CreatureSlots[0];
            var info = Creature.Pikeman.GetInfo(this.game.GameVersion);
            Assert.That(slot.SelectedCreature, Is.EqualTo("Pikeman"));

            slot.CreatureAmount = 10;
            Assert.That(slot.TotalHealth, Is.EqualTo((info.Health * 10).ToString("N0")));
            Assert.That(slot.TotalCost, Is.EqualTo((info.Gold * 10).ToString("N0")));
            Assert.That(slot.TotalDamage, Does.Contain((info.MinDamage * 10).ToString("N0")));

            slot.CreatureAmount = 25;
            Assert.That(slot.TotalHealth, Is.EqualTo((info.Health * 25).ToString("N0")));
            Assert.That(slot.TotalOtherCost, Is.Empty, "pikemen cost only gold");

            var empty = this.Creatures.CreatureSlots.First(s => !s.HasCreature);
            Assert.That(empty.TotalHealth, Is.Empty);
            Assert.That(empty.TotalCost, Is.Empty);
        }

        [Test]
        public void FinderFilterMatchesTownNamesToo()
        {
            this.Finder.Filter = "necropolis";

            Assert.That(Names(this.Finder), Does.Contain("Lich").And.Not.Contain("Pikeman"));
        }

        [Test]
        public void AddingACreatureFillsTheFirstEmptySlotAndUndoRemovesIt()
        {
            var slots = this.Creatures.CreatureSlots;
            var empty = slots.First(s => !s.HasCreature);
            var angel = this.Finder.Items.Cast<CreatureChoiceViewModel>().Single(c => c.Name == "Angel");
            this.Finder.AddAmount = 3;

            this.Finder.ChooseCommand.Execute(angel);

            Assert.That(empty.SelectedCreature, Is.EqualTo("Angel"));
            Assert.That(empty.CreatureAmount, Is.EqualTo(3));

            this.game.History.Undo();

            Assert.That(empty.HasCreature, Is.False);
        }

        [Test]
        public void AddingIsRefusedWhenEverySlotIsInUse()
        {
            var angel = this.Finder.Items.Cast<CreatureChoiceViewModel>().Single(c => c.Name == "Angel");
            while (this.Creatures.HasFreeSlot)
            {
                this.Finder.ChooseCommand.Execute(angel);
            }

            Assert.That(this.Creatures.CreatureSlots.All(s => s.HasCreature), Is.True);
            Assert.That(this.Finder.ChooseCommand.CanExecute(angel), Is.False);
            Assert.That(this.Finder.Summary, Does.Contain("in use"));

            this.Creatures.CreatureSlots[2].ClearCommand.Execute(null);

            Assert.That(this.Creatures.HasFreeSlot, Is.True);
            Assert.That(this.Finder.ChooseCommand.CanExecute(angel), Is.True);
        }

        [Test]
        public void GeneralPageNotesWhatArtifactsAddToSkillsAndMovement()
        {
            var general = this.hero.GeneralInfoViewModel;
            var baseAttack = general.Attack;
            Assert.That(general.AttackArtifactNote, Is.Empty);

            this.hero.ArtifactsViewModel.SelectedHelm = Artifact.HelmOfHeavenlyEnlightenment.Name;

            Assert.That(general.AttackArtifactNote, Is.EqualTo("+6 from artifacts"));
            Assert.That(general.AttackTotal, Is.EqualTo(baseAttack + 6));
            Assert.That(general.NextLevelNote, Does.Contain("XP to level"));
            Assert.That(general.KnowledgeArtifactNote, Does.Contain("from artifacts"));
            Assert.That(general.MovementArtifactNote, Is.Empty);
            Assert.That(general.MaxMovementText, Does.StartWith("Maximum "));

            this.game.History.Undo();

            Assert.That(general.AttackArtifactNote, Is.Empty);
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
