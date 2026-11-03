using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// How edits of the army and of the general tab of a hero page reach the hero, over a hero session with no savefile. The hero has
    /// ten pikemen and five archers in the first two slots and four free slots after them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class HeroEditingPageTests
    {
        private FakeHeroSession session = null!;
        private HeroViewModel page = null!;

        private GeneralInfoPageViewModel General => this.page.GeneralInfoViewModel;

        [SetUp]
        public void StartWithPikemenAndArchers()
        {
            var christian = HeroModels.Christian();
            this.session = new FakeHeroSession(christian with { Army = christian.Army.WithSlot(1, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archer"), 5) });
            this.page = HeroPages.Create(this.session);
        }

        [TearDown]
        public void ClosePage()
        {
            this.page.Dispose();
        }

        [Test]
        public void CreatureSlotsShowTheArmyAndWhichMovesAreAllowed()
        {
            var slots = this.page.ArmyViewModel.CreatureSlots;

            Assert.That(slots[0].SelectedCreature, Is.EqualTo("Pikeman"));
            Assert.That(slots[0].CreatureAmount, Is.EqualTo(10));
            Assert.That(slots[3].SelectedCreature, Is.Null);
            Assert.That(slots[0].MoveUpCommand.CanExecute(null), Is.False);
            Assert.That(slots[0].MoveDownCommand.CanExecute(null), Is.True);
            Assert.That(slots[3].ClearCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void MovingACreatureSlotSwapsStacks()
        {
            var slots = this.page.ArmyViewModel.CreatureSlots;

            slots[0].MoveDownCommand.Execute(null);

            Assert.That(slots[0].SelectedCreature, Is.EqualTo("Archer"));
            Assert.That(slots[1].SelectedCreature, Is.EqualTo("Pikeman"));
            Assert.That(slots[1].CreatureAmount, Is.EqualTo(10));
            Assert.That(this.session.Current.Army.Slots[1]!.Creature, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman")));
        }

        [Test]
        public void ClearingACreatureSlotEmptiesItAndPuttingTheHeroBackBringsItBack()
        {
            var slots = this.page.ArmyViewModel.CreatureSlots;

            slots[1].ClearCommand.Execute(null);

            Assert.That(slots[1].SelectedCreature, Is.Null);
            Assert.That(slots[1].CreatureAmount, Is.EqualTo(0));
            Assert.That(this.session.Current.Army.Slots[1], Is.Null);

            this.session.Update(_ => this.session.Original);

            Assert.That(slots[1].SelectedCreature, Is.EqualTo("Archer"));
            Assert.That(slots[1].CreatureAmount, Is.EqualTo(5));
        }

        [Test]
        public void PickingACreatureStartsAtOneAndAnAmountWritesTheSlot()
        {
            var slots = this.page.ArmyViewModel.CreatureSlots;

            slots[5].SelectedCreature = "Angel";
            Assert.That(slots[5].CreatureAmount, Is.EqualTo(1));

            slots[5].CreatureAmount = 40;

            Assert.That(this.session.Current.Army.Slots[5], Is.EqualTo(new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Angel"), 40)));
        }

        [Test]
        public void PickingNoCreatureEmptiesTheSlot()
        {
            var slots = this.page.ArmyViewModel.CreatureSlots;
            slots[5].SelectedCreature = "Angel";

            slots[5].SelectedCreature = string.Empty;

            Assert.That(this.session.Current.Army.Slots[5], Is.Null);
        }

        [Test]
        public void LevelAndExperienceStayInStep()
        {
            this.General.LevelAndClass.Level = 10;
            Assert.That(this.General.LevelAndClass.Experience, Is.EqualTo(14700));

            this.General.LevelAndClass.Experience = 20600;
            Assert.That(this.General.LevelAndClass.Level, Is.EqualTo(12));
        }

        [Test]
        public void ASkillAndSpellPointsAndAWarMachineEditedOnThePageLandOnTheHero()
        {
            this.General.PrimarySkills.Attack.Base = 42;
            this.General.MovementAndSpellPoints.CurrentSpellPoints = 99;
            this.General.UpdateWarMachine("Ammo_Cart", true);

            Assert.That(this.session.Current.PrimarySkills.Attack, Is.EqualTo((byte)42));
            Assert.That(this.session.Current.SpellPoints, Is.EqualTo(99));
            Assert.That(this.session.Current.WarMachines, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("AmmoCart")));
            Assert.That(this.General.WarMachineChoices.Single(c => c.Name == "Ammo Cart").IsChecked, Is.True);
            Assert.That(this.session.IsDirty, Is.True);
        }

        [Test]
        public void BallistaAndCannonAreExclusive()
        {
            this.General.UpdateWarMachine("Ballista", true);
            this.General.UpdateWarMachine("Cannon", true);

            Assert.That(this.session.Current.WarMachines, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Cannon")));
            Assert.That(this.session.Current.WarMachines, Does.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Ballista")));
            Assert.That(this.General.WarMachineChoices.Single(c => c.Name == "Ballista").IsChecked, Is.False);
            Assert.That(this.General.WarMachineChoices.Single(c => c.Name == "Cannon").IsChecked, Is.True);
        }

        [Test]
        public void UntickingAWarMachineTakesItOffTheHero()
        {
            this.General.UpdateWarMachine("Ammo_Cart", true);

            this.General.UpdateWarMachine("Ammo_Cart", false);

            Assert.That(this.session.Current.WarMachines, Does.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("AmmoCart")));
            Assert.That(this.General.WarMachineChoices.Single(c => c.Name == "Ammo Cart").IsChecked, Is.False);
        }

        [Test]
        public void SelectingAllWarMachinesKeepsOneOfBallistaAndCannon()
        {
            this.General.SelectAllWarMachinesCommand.Execute(null);

            Assert.That(this.session.Current.WarMachines, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Ballista"), TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("AmmoCart"), TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("FirstAidTent") }));
        }

        [Test]
        public void RestoreMovementGivesBackTheMaximumAndIsOnlyOfferedWhenSomeIsUsed()
        {
            var points = this.General.MovementAndSpellPoints;

            points.CurrentMovement = 0;
            Assert.That(points.RestoreMovementCommand.CanExecute(null), Is.True);

            points.RestoreMovementCommand.Execute(null);

            Assert.That(points.CurrentMovement, Is.EqualTo(points.MaxMovement));
            Assert.That(points.RestoreMovementCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void ResettingSpellPointsFillsThemToTenForEachKnowledgePointAndTheBonusOfIntelligence()
        {
            var points = this.General.MovementAndSpellPoints;
            var knowledge = (int)this.session.Current.PrimarySkills.Knowledge;

            points.CurrentSpellPoints = 0;
            points.ResetSpellPointsCommand.Execute(null);

            Assert.That(points.MostSpellPoints, Is.EqualTo(knowledge * 10));
            Assert.That(points.CurrentSpellPoints, Is.EqualTo(points.MostSpellPoints));
        }

        [TestCase(GameVersion.ShadowOfDeath, 1, 25)]
        [TestCase(GameVersion.ShadowOfDeath, 2, 50)]
        [TestCase(GameVersion.ShadowOfDeath, 3, 100)]
        [TestCase(GameVersion.HornOfTheAbyss, 1, 20)]
        [TestCase(GameVersion.HornOfTheAbyss, 2, 35)]
        [TestCase(GameVersion.HornOfTheAbyss, 3, 50)]
        public void IntelligenceRaisesTheMostSpellPointsByAPercentThatDependsOnTheGame(GameVersion version, int level, int percent)
        {
            using var other = HeroPages.Create(this.session = new FakeHeroSession(HeroModels.Christian()) { GameVersion = version });
            var knowledge = (int)this.session.Current.PrimarySkills.Knowledge;

            this.session.Update(h => h with { SecondarySkills = h.SecondarySkills.WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Intelligence"), SkillLevel.Of((byte)level)) });

            Assert.That(other.GeneralInfoViewModel.MovementAndSpellPoints.MostSpellPoints, Is.EqualTo(knowledge * 10 * (100 + percent) / 100));
        }

        [Test]
        public void RotatingTurnsTheHeroAnEighthOfACircleAndWraps()
        {
            this.General.Position.Facing = FacingDirection.North;
            this.General.Position.RotateLeftCommand.Execute(null);
            Assert.That(this.General.Position.Facing, Is.EqualTo(FacingDirection.NorthWest));

            this.General.Position.RotateRightCommand.Execute(null);
            this.General.Position.RotateRightCommand.Execute(null);
            Assert.That(this.General.Position.Facing, Is.EqualTo(FacingDirection.NorthEast));
        }

        [Test]
        public void RemovingTheDestinationMarkerSetsItToMinusOneAndCannotBeDoneTwice()
        {
            this.General.Destination.X = 5;
            Assert.That(this.General.Destination.RemoveCommand.CanExecute(null), Is.True);

            this.General.Destination.RemoveCommand.Execute(null);

            Assert.That(this.session.Current.Destination, Is.EqualTo(new MapPosition(-1, -1, -1)));
            Assert.That(this.General.Destination.RemoveCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void ALevelChoiceOfAMapWithoutAnUndergroundHasOnlySurfaceAndOtherMapsOfferTheUndergroundToo()
        {
            var flat = new GeneralInfoPageViewModel(() => this.session.Current, edit => this.session.Update(edit), new SpellPointsCalculatorFactory().Create(this.session.GameVersion), this.session.GameVersion, hasUnderground: false);

            Assert.That(flat.Position.LevelChoices.Select(c => c.Label), Is.EqualTo(new[] { "Surface" }).Or.EqualTo(new[] { "Surface", "None" }));
            Assert.That(this.General.Position.LevelChoices.Select(c => c.Label), Does.Contain("Underground"));
        }
    }
}
