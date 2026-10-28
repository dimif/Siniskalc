using Siniskalc;
using Siniskalc.Core.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Heroes.ViewModels;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.Tests.Integration.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroGeneralTabTests
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
        public void MaxGivesTheHeroTheMostMovementAndSpellPoints()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.MovementAndSpellPoints.MaxMovementCommand.Execute(null);
            general.MovementAndSpellPoints.MaxSpellPointsCommand.Execute(null);

            Assert.That(this.hero.HeroModel.MovementPoints.Current, Is.EqualTo((uint)Constants.Hero.MaxMovementPoints));
            Assert.That(this.hero.HeroModel.SpellPoints, Is.EqualTo(Constants.Hero.MaxSpellPoints));
            Assert.That(general.MovementAndSpellPoints.MaxMovementCommand.CanExecute(null), Is.False);
            Assert.That(general.MovementAndSpellPoints.MaxSpellPointsCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void ResetStillFillsTheSpellPointsToTheMostTheHeroCanHave()
        {
            var general = this.hero.GeneralInfoViewModel;
            general.MovementAndSpellPoints.MaxSpellPointsCommand.Execute(null);

            general.MovementAndSpellPoints.ResetSpellPointsCommand.Execute(null);

            Assert.That(this.hero.HeroModel.SpellPoints, Is.EqualTo(TestServices.SpellPoints(this.hero.GameVersion).GetMaximum(this.hero.HeroModel.PrimarySkills, this.hero.HeroModel.SecondarySkills)));
        }

        [Test]
        public void TheGeneralTabEditsTheMovedFlagAndTheActiveSpells()
        {
            var general = this.hero.GeneralInfoViewModel;

            var fly = general.ActiveSpells.Spells.Single(t => t.Name == "Fly");
            var visions = general.ActiveSpells.Spells.Single(t => t.Name == "Visions");

            general.MovedToday = true;
            fly.IsChecked = true;
            visions.IsChecked = true;
            fly.IsChecked = false;

            Assert.That(this.hero.HeroModel.MovedToday, Is.True);
            Assert.That(this.hero.HeroModel.ActiveSpells, Is.EqualTo(ActiveAdventureSpells.Visions));
            Assert.That(visions.IsChecked, Is.True);
            Assert.That(fly.IsChecked, Is.False);
        }

        [Test]
        public void ResetPutsTheWholeHeroBackAndTheHeaderNamesWhereItIs()
        {
            var original = this.hero.HeroModel;
            Assert.That(this.hero.PositionText, Does.Match(@"^X: \d+, Y: \d+, (Surface \(0\)|Underground \(1\))$"));
            Assert.That(this.hero.ResetCommand.CanExecute(null), Is.False);

            this.hero.GeneralInfoViewModel.MovementAndSpellPoints.MaxMovementCommand.Execute(null);
            this.hero.GeneralInfoViewModel.MovementAndSpellPoints.MaxSpellPointsCommand.Execute(null);
            Assert.That(this.hero.ResetCommand.CanExecute(null), Is.True);

            this.hero.ResetCommand.Execute(null);

            Assert.That(this.hero.HasPendingChanges, Is.False);
            Assert.That(this.hero.HeroModel.MovementPoints, Is.EqualTo(original.MovementPoints));
            Assert.That(this.hero.HeroModel.SpellPoints, Is.EqualTo(original.SpellPoints));
        }

        [Test]
        public void AStackShowsItsAiAndFightValue()
        {
            var slot = this.hero.ArmyViewModel.CreatureSlots[0];
            var info = Creature.Pikeman.GetInfo(this.hero.GameVersion);

            Assert.That(slot.TotalAiValue, Is.EqualTo((info.AiValue * slot.CreatureAmount).ToString("N0")));
            Assert.That(slot.TotalFightValue, Is.EqualTo((info.FightValue * slot.CreatureAmount).ToString("N0")));
        }

        [Test]
        public void TheArmySummaryAddsUpTheStacks()
        {
            var creatures = this.hero.ArmyViewModel;
            var version = this.hero.GameVersion;
            var stacks = creatures.CreatureSlots.Where(s => s.HasCreature)
                .Select(s => (Info: (AbstractEnumeration.FromName<Creature>(s.SelectedCreature ?? string.Empty) ?? throw new AssertionException("Unknown creature.")).GetInfo(version), s.CreatureAmount)).ToList();

            var gold = stacks.Sum(s => (long)s.Info.Gold * s.CreatureAmount);
            var health = stacks.Sum(s => (long)s.Info.Health * s.CreatureAmount);

            Assert.That(creatures.ArmySummary, Does.StartWith($"HP Total {health:N0}"));
            Assert.That(creatures.ArmySummary, Does.Contain($"Cost {gold:N0}"));
            Assert.That(creatures.ArmySummary, Does.Contain("AI ").And.Contain("Fight "));
        }

        [Test]
        public void TheCreatureFinderShowsCostResourcesAndSpecialButNotGrowth()
        {
            var finder = this.hero.ArmyViewModel.Finder;

            Assert.That(finder.Columns.Select(c => c.Header), Does.Not.Contain("Growth").And.Not.Contain("Gold"));
            Assert.That(finder.Columns.Select(c => c.Header), Is.SupersetOf(new[] { "Cost", "Resources", "Special" }));
        }

        [Test]
        public void ACreatureChoiceTellsItsResourcesAndSpecial()
        {
            var troll = new CreatureChoiceViewModel(Creature.Troll, Creature.Troll.GetInfo(this.hero.GameVersion));

            Assert.That(troll.Special, Does.Contain("Regeneration"));
            Assert.That(troll.Resources, Is.Empty);

            // A creature that costs a resource besides gold lists it, and the finder's tooltip still lists it with the gold.
            var version = this.hero.GameVersion;
            var pricey = Creature.All.Where(c => c.IsAvailableIn(version)).Select(c => new CreatureChoiceViewModel(c, c.GetInfo(version))).First(c => c.Resources.Length > 0);
            Assert.That(pricey.Details, Does.Contain(pricey.Resources));
        }

        [Test]
        public void ValuesBeyondTheLimitsAreClampedByTheGeneralTab()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.PrimarySkills.Attack.Base = byte.MaxValue;
            general.Position.X = short.MaxValue;
            general.Destination.Y = short.MinValue;
            general.MovementAndSpellPoints.CurrentSpellPoints = ushort.MaxValue;
            general.MovementAndSpellPoints.CurrentMovement = uint.MaxValue;

            Assert.That(this.hero.HeroModel.PrimarySkills.Attack, Is.EqualTo(Constants.Hero.MaxPrimarySkill));
            Assert.That(this.hero.HeroModel.Position.X, Is.EqualTo(Constants.Map.MaxCoordinate));
            Assert.That(this.hero.HeroModel.Destination.Y, Is.EqualTo(Constants.Map.MinCoordinate));
            Assert.That(this.hero.HeroModel.SpellPoints, Is.EqualTo(Constants.Hero.MaxSpellPoints));
            Assert.That(this.hero.HeroModel.MovementPoints.Current, Is.EqualTo((uint)Constants.Hero.MaxMovementPoints));
        }
    }
}
