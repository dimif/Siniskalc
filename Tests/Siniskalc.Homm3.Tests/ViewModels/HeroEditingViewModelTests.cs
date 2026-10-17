using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Homm3.Tests.Models;

namespace Siniskalc.Homm3.Tests.ViewModels
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
            Assert.That(this.hero.HeroModel.Army.Slots[1]!.Creature, Is.EqualTo(Creature.Pikeman));
        }

        [Test]
        public void ClearingACreatureSlotEmptiesItAndUndoBringsItBack()
        {
            var slots = this.hero.ArmyViewModel.CreatureSlots;

            slots[1].ClearCommand.Execute(null);

            Assert.That(slots[1].SelectedCreature, Is.Null);
            Assert.That(slots[1].CreatureAmount, Is.EqualTo(0));
            Assert.That(this.hero.HeroModel.Army.Slots[1], Is.Null);

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
            Assert.That(this.hero.HeroModel.Army.Slots[5], Is.EqualTo(new ArmySlot<Creature>(Creature.Angel, 40)));

            slots[5].SelectedCreature = string.Empty;
            Assert.That(this.hero.HeroModel.Army.Slots[5], Is.Null);
        }

        [Test]
        public void LevelAndExperienceStayInStep()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.LevelAndClass.Level = 10;
            Assert.That(general.LevelAndClass.Experience, Is.EqualTo(14700));

            general.LevelAndClass.Experience = 20600;
            Assert.That(general.LevelAndClass.Level, Is.EqualTo(12));
        }

        [Test]
        public void EditsFromThePageLandInThePendingChanges()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.Attack = 42;
            general.MovementAndSpellPoints.CurrentSpellPoints = 99;
            general.UpdateWarMachine("Ammo_Cart", true);

            Assert.That(this.hero.HeroModel.PrimarySkills.Attack, Is.EqualTo((byte)42));
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

            general.MovementAndSpellPoints.CurrentMovement = 0;
            Assert.That(general.MovementAndSpellPoints.RestoreMovementCommand.CanExecute(null), Is.True);

            general.MovementAndSpellPoints.RestoreMovementCommand.Execute(null);

            Assert.That(general.MovementAndSpellPoints.CurrentMovement, Is.EqualTo(general.MovementAndSpellPoints.MaxMovement));
            Assert.That(general.MovementAndSpellPoints.RestoreMovementCommand.CanExecute(null), Is.False);
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
            var knowledge = (int)hero.PrimarySkills.Knowledge;   // the saved knowledge has the bonus of the artifacts in it

            general.MovementAndSpellPoints.CurrentSpellPoints = 0;
            general.MovementAndSpellPoints.ResetSpellPointsCommand.Execute(null);

            Assert.That(general.MovementAndSpellPoints.MostSpellPoints, Is.EqualTo(knowledge * 10 * (100 + percent) / 100));
            Assert.That(general.MovementAndSpellPoints.CurrentSpellPoints, Is.EqualTo(general.MovementAndSpellPoints.MostSpellPoints));
        }

        [Test]
        public void RotatingTurnsTheHeroAnEighthOfACircleAndWraps()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.Position.Facing = FacingDirection.North;
            general.Position.RotateLeftCommand.Execute(null);
            Assert.That(general.Position.Facing, Is.EqualTo(FacingDirection.NorthWest));

            general.Position.RotateRightCommand.Execute(null);
            general.Position.RotateRightCommand.Execute(null);
            Assert.That(general.Position.Facing, Is.EqualTo(FacingDirection.NorthEast));
        }

        [Test]
        public void RemovingTheMarkerSetsItToMinusOne()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.Destination.X = 5;
            general.Destination.RemoveCommand.Execute(null);

            Assert.That(this.hero.HeroModel.Destination, Is.EqualTo(new MapPosition(-1, -1, -1)));
            Assert.That(general.Destination.RemoveCommand.CanExecute(null), Is.False);
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
            Assert.That(this.hero.HeroModel.Destination, Is.EqualTo(new MapPosition(-1, -1, -1)));

            this.hero.SelectedOwner = "Blue";
            Assert.That(this.hero.HeroModel.Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void LevelChoicesOfAMapWithoutAnUndergroundHaveOnlySurface()
        {
            var general = new GeneralInfoPageViewModel(() => this.hero.HeroModel, edit => this.hero.Session.Update(edit), TestServices.SpellPoints, this.hero.GameVersion, hasUnderground: false);

            Assert.That(general.Position.LevelChoices.Select(c => c.Label), Is.EqualTo(new[] { "Surface (0)" }).Or.EqualTo(new[] { "Surface (0)", "None" }));
            Assert.That(this.hero.GeneralInfoViewModel.Position.LevelChoices.Select(c => c.Label), Does.Contain("Underground (1)"));
        }
    }
}
