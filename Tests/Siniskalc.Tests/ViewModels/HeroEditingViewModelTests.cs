using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;
using Siniskalc.Tests.Models;

namespace Siniskalc.Tests.ViewModels
{
    [TestFixture]
    internal sealed class HeroEditingViewModelTests
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
        public void CreatureSlotsShowTheArmy()
        {
            var slots = this.hero.ArmyViewModel.CreatureSlots;

            Assert.That(slots[0].SelectedCreature, Is.EqualTo("Pikeman"));
            Assert.That(slots[0].CreatureAmount, Is.EqualTo(10));
            Assert.That(slots[3].SelectedCreature, Is.Null);
            Assert.That(slots[0].MoveLeftCommand.CanExecute(null), Is.False);
            Assert.That(slots[0].MoveRightCommand.CanExecute(null), Is.True);
            Assert.That(slots[3].ClearCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void MovingACreatureSlotSwapsStacks()
        {
            var slots = this.hero.ArmyViewModel.CreatureSlots;

            slots[0].MoveRightCommand.Execute(null);

            Assert.That(slots[0].SelectedCreature, Is.EqualTo("Archer"));
            Assert.That(slots[1].SelectedCreature, Is.EqualTo("Pikeman"));
            Assert.That(slots[1].CreatureAmount, Is.EqualTo(10));
            Assert.That(this.hero.HeroModel.Army.GetSlot(2).CreatureCode, Is.EqualTo(Creature.Pikeman.Code));
        }

        [Test]
        public void ClearingACreatureSlotEmptiesItAndUndoBringsItBack()
        {
            var slots = this.hero.ArmyViewModel.CreatureSlots;

            slots[1].ClearCommand.Execute(null);

            Assert.That(slots[1].SelectedCreature, Is.Null);
            Assert.That(slots[1].CreatureAmount, Is.EqualTo(0));
            Assert.That(this.hero.HeroModel.Army.FindSlot(2), Is.Null);

            this.game.History.Undo();

            Assert.That(slots[1].SelectedCreature, Is.EqualTo("Archer"));
            Assert.That(slots[1].CreatureAmount, Is.EqualTo(5));
        }

        [Test]
        public void PickingACreatureAndAmountWritesTheSlot()
        {
            var slots = this.hero.ArmyViewModel.CreatureSlots;

            slots[5].SelectedCreature = "Angel";
            Assert.That(slots[5].CreatureAmount, Is.EqualTo(1));

            slots[5].CreatureAmount = 40;
            Assert.That(this.hero.HeroModel.Army.GetSlot(6), Is.EqualTo(new ArmySlot(6, Creature.Angel.Code, 40)));

            slots[5].SelectedCreature = string.Empty;
            Assert.That(this.hero.HeroModel.Army.FindSlot(6), Is.Null);
        }

        [Test]
        public void LevelAndExperienceStayInStep()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.Level = 10;
            Assert.That(general.Experience, Is.EqualTo(14700));

            general.Experience = 20600;
            Assert.That(general.Level, Is.EqualTo((short)12));
        }

        [Test]
        public void EditsFromThePageLandInThePendingChanges()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.Attack = 42;
            general.CurrentSpellPoints = 99;
            general.UpdateWarMachine("Ammo_Cart", true);

            Assert.That(this.hero.HeroModel.PrimarySkills.Atk, Is.EqualTo((byte)42));
            Assert.That(general.WarMachinesChecked[3], Is.True);
            Assert.That(this.game.History.GetPendingChanges().Select(c => c.Change.Description), Is.SupersetOf(new[] { "Attack", "Spell points", "War machines" }));
        }

        [Test]
        public void BallistaAndCannonAreExclusive()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.UpdateWarMachine("Ballista", true);
            general.UpdateWarMachine("Cannon", true);

            Assert.That(this.hero.HeroModel.WarMachines, Does.Contain(WarMachine.Cannon));
            Assert.That(this.hero.HeroModel.WarMachines, Does.Not.Contain(WarMachine.Ballista));
            Assert.That(general.WarMachinesChecked[0], Is.False);
            Assert.That(general.WarMachinesChecked[2], Is.True);
        }

        [Test]
        public void RestoreMovementGivesBackTheMaximum()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.CurrentMovement = 0;
            Assert.That(general.RestoreMovementCommand.CanExecute(null), Is.True);

            general.RestoreMovementCommand.Execute(null);

            Assert.That(general.CurrentMovement, Is.EqualTo(general.MaxMovement));
            Assert.That(general.RestoreMovementCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void ResettingSpellPointsFillsThemToTheMaximumFromKnowledgeAndIntelligence()
        {
            var general = this.hero.GeneralInfoViewModel;
            var hero = this.hero.HeroModel;
            var percent = hero.SecondarySkills.GetSkillLevel(SecondarySkill.Intelligence) switch
            {
                SkillLevel.Beginner => 25,
                SkillLevel.Intermediate => 50,
                SkillLevel.Expert => 100,
                _ => 0,
            };
            var knowledge = Math.Max(0, hero.PrimarySkills.Kno + hero.EquippedArtifacts.GetTotalEffects().Kno);

            general.CurrentSpellPoints = 0;
            general.ResetSpellPointsCommand.Execute(null);

            Assert.That(general.MaxSpellPoints, Is.EqualTo(knowledge * 10 * (100 + percent) / 100));
            Assert.That(general.CurrentSpellPoints, Is.EqualTo(general.MaxSpellPoints));
        }

        [Test]
        public void RotatingTurnsTheHeroAnEighthOfACircleAndWraps()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.FacingDirection = FacingDirection.North;
            general.RotateLeftCommand.Execute(null);
            Assert.That(general.FacingDirection, Is.EqualTo(FacingDirection.NorthWest));

            general.RotateRightCommand.Execute(null);
            general.RotateRightCommand.Execute(null);
            Assert.That(general.FacingDirection, Is.EqualTo(FacingDirection.NorthEast));
        }

        [Test]
        public void RemovingTheMarkerSetsItToMinusOne()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.MarkerX = 5;
            general.RemoveMarkerCommand.Execute(null);

            Assert.That(this.hero.HeroModel.MarkerPosition, Is.EqualTo(new Position(-1, -1, -1)));
            Assert.That(general.RemoveMarkerCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void SelectingAllWarMachinesKeepsOneOfBallistaAndCannon()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.SelectAllWarMachinesCommand.Execute(null);

            Assert.That(this.hero.HeroModel.WarMachines, Is.EquivalentTo(new[] { WarMachine.Ballista, WarMachine.AmmoCart, WarMachine.FirstAidTent }));
        }

        [Test]
        public void MakingAHeroNeutralTakesItOffTheMap()
        {
            this.hero.SelectedOwner = "Neutral";

            Assert.That(this.hero.HeroModel.Owner, Is.Null);
            Assert.That(this.hero.HeroModel.Position, Is.EqualTo(new HeroPosition(-1, -1, -1, FacingDirection.East)));
            Assert.That(this.hero.HeroModel.MarkerPosition, Is.EqualTo(new Position(-1, -1, -1)));

            this.hero.SelectedOwner = "Blue";
            Assert.That(this.hero.HeroModel.Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void LevelChoicesOfAMapWithoutAnUndergroundHaveOnlySurface()
        {
            var general = new GeneralInfoPageViewModel(() => this.hero.HeroModel, edit => this.hero.Session.Update(edit), TestServices.SpellPoints, this.hero.GameVersion, hasUnderground: false);

            Assert.That(general.PositionLevelChoices.Select(c => c.Label), Is.EqualTo(new[] { "Surface (0)" }).Or.EqualTo(new[] { "Surface (0)", "None" }));
            Assert.That(this.hero.GeneralInfoViewModel.PositionLevelChoices.Select(c => c.Label), Does.Contain("Underground (1)"));
        }
    }
}
