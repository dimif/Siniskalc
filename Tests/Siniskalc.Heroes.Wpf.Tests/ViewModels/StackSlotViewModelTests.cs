namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests one slot of an army on a page: the creature and amount it holds and tells about, the buttons that move, clear and upgrade it,
    /// and the picker that chooses its creature.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class StackSlotViewModelTests
    {
        private List<(int Slot, string? Creature, int Amount)> selections = null!;
        private List<(int Slot, int Direction)> moves = null!;
        private List<int> clears = null!;

        [SetUp]
        public void StartWithNothingDone()
        {
            this.selections = [];
            this.moves = [];
            this.clears = [];
        }

        private StackSlotViewModel Slot(int number = 2) =>
            new(number, (slot, creature, amount) => this.selections.Add((slot, creature, amount)), (slot, direction) => this.moves.Add((slot, direction)), this.clears.Add);

        [Test]
        public void ANewSlotIsEmptyAndShownFromOne()
        {
            var slot = this.Slot(2);

            Assert.That((slot.SlotNumber, slot.DisplayNumber, slot.HasCreature, slot.CreatureButtonText, slot.CreatureAmount), Is.EqualTo((2, 3, false, "(empty)", 0)));
            Assert.That(slot.Facts, Is.Empty);
        }

        [TestCase("1", 13)]
        [TestCase("-1", 11)]
        [TestCase("5", 17)]
        [TestCase("-5", 7)]
        [TestCase("-50", 0)]
        public void TheAdjustAmountCommandAddsItsStepToTheAmountOfTheStackButNeverGoesBelowZero(string step, int expected)
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 12, false, false);

            slot.AdjustAmountCommand.Execute(step);

            Assert.That(this.selections, Is.EqualTo(new[] { (2, (string?)"Archer", expected) }));
        }

        [Test]
        public void TheAdjustAmountCommandCannotRunOnAnEmptySlot()
        {
            var slot = this.Slot();
            slot.InitializeSlot(null, 0, false, false);

            Assert.That(slot.AdjustAmountCommand.CanExecute("1"), Is.False);
        }

        [Test]
        public void PickingACreatureTellsTheArmyWithTheAmountItHas()
        {
            var slot = this.Slot();
            slot.InitializeSlot(null, 5, false, false);

            slot.SelectedCreature = "Archer";

            Assert.That(this.selections, Is.EqualTo(new[] { (2, (string?)"Archer", 5) }));
            Assert.That((slot.HasCreature, slot.CreatureButtonText), Is.EqualTo((true, "Archer")));
        }

        [Test]
        public void ChangingTheAmountTellsTheArmyWithTheCreatureItHolds()
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 5, false, false);

            slot.CreatureAmount = 12;

            Assert.That(this.selections, Is.EqualTo(new[] { (2, (string?)"Archer", 12) }));
        }

        [Test]
        public void TheSameCreatureOrAmountTellsNothing()
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 5, false, false);

            slot.SelectedCreature = "Archer";
            slot.CreatureAmount = 5;

            Assert.That(this.selections, Is.Empty);
        }

        [Test]
        public void InitializingASlotShowsWhatThePageHoldsAndTellsNobody()
        {
            var slot = this.Slot();

            slot.InitializeSlot("Archer", 5, true, true);

            Assert.That((slot.SelectedCreature, slot.CreatureAmount, slot.HasCreature), Is.EqualTo(("Archer", 5, true)));
            Assert.That(this.selections, Is.Empty);
        }

        [Test]
        public void InitializingTellsEveryPropertyThatShowsTheStack()
        {
            var slot = this.Slot();
            var raised = new List<string?>();
            slot.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            slot.InitializeSlot("Archer", 5, true, true);

            Assert.That(raised, Is.SupersetOf(new[]
            {
                nameof(StackSlotViewModel.SelectedCreature), nameof(StackSlotViewModel.CreatureAmount), nameof(StackSlotViewModel.HasCreature),
                nameof(StackSlotViewModel.CreatureButtonText), nameof(StackSlotViewModel.Facts), nameof(StackSlotViewModel.Upgrades),
            }));
        }

        [Test]
        public void TheMoveButtonsAreOfferedOnlyWhereTheSlotCanMoveThatWay()
        {
            var slot = this.Slot();

            slot.InitializeSlot("Archer", 5, false, true);
            Assert.That((slot.MoveUpCommand.CanExecute(null), slot.MoveDownCommand.CanExecute(null)), Is.EqualTo((false, true)));

            slot.InitializeSlot("Archer", 5, true, false);
            Assert.That((slot.MoveUpCommand.CanExecute(null), slot.MoveDownCommand.CanExecute(null)), Is.EqualTo((true, false)));
        }

        [Test]
        public void TheMoveButtonsTellTheArmyTheSlotAndTheDirection()
        {
            var slot = this.Slot(3);
            slot.InitializeSlot("Archer", 5, true, true);

            slot.MoveUpCommand.Execute(null);
            slot.MoveDownCommand.Execute(null);

            Assert.That(this.moves, Is.EqualTo(new[] { (3, -1), (3, 1) }));
        }

        [Test]
        public void ClearIsOfferedOnlyWhenTheSlotHoldsACreatureAndTellsTheArmyTheSlot()
        {
            var slot = this.Slot(3);
            Assert.That(slot.ClearCommand.CanExecute(null), Is.False);

            slot.InitializeSlot("Archer", 5, false, false);
            Assert.That(slot.ClearCommand.CanExecute(null), Is.True);

            slot.ClearCommand.Execute(null);

            Assert.That(this.clears, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void FactsAreAskedForTheAmountOfTheSlotAndOnlyWhenItHoldsACreature()
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 5, false, false, amount => [new StackFact("Amount", amount.ToString())]);

            Assert.That(slot.Facts.Select(f => f.Value), Is.EqualTo(new[] { "5" }));

            slot.InitializeSlot(null, 5, false, false, amount => [new StackFact("Amount", amount.ToString())]);

            Assert.That(slot.Facts, Is.Empty);
        }

        [Test]
        public void ChangingTheAmountRefreshesTheFacts()
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 5, false, false, amount => [new StackFact("Amount", amount.ToString())]);
            var raised = new List<string?>();
            slot.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            slot.CreatureAmount = 8;

            Assert.That(raised, Does.Contain(nameof(StackSlotViewModel.Facts)));
            Assert.That(slot.Facts.Single().Value, Is.EqualTo("8"));
        }

        [Test]
        public void ASlotWithNoUpgradeCannotBeUpgradedAndSaysSo()
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 5, false, false);

            Assert.That((slot.HasUpgrade, slot.HasSeveralUpgrades, slot.UpgradeCommand.CanExecute(null), slot.UpgradeText), Is.EqualTo((false, false, false, "Upgrade")));
            Assert.That(slot.UpgradeToolTip, Is.EqualTo("This creature cannot be upgraded further"));
        }

        [Test]
        public void ASlotWithOneUpgradeChangesTheStackIntoItKeepingTheAmount()
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 5, false, false, upgrades: ["Marksman"]);
            this.selections.Clear();

            slot.UpgradeCommand.Execute(null);

            Assert.That(slot.UpgradeToolTip, Is.EqualTo("Change the stack into Marksman, keeping its size"));
            Assert.That(this.selections, Is.EqualTo(new[] { (2, (string?)"Marksman", 5) }));
        }

        [Test]
        public void ASlotWithSeveralUpgradesOffersAChoiceAndUpgradesToTheOneNamed()
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 5, false, false, upgrades: ["Marksman", "Sharpshooter"]);
            this.selections.Clear();

            slot.UpgradeCommand.Execute("Sharpshooter");

            Assert.That((slot.HasSeveralUpgrades, slot.UpgradeText), Is.EqualTo((true, "Upgrade ▾")));
            Assert.That(slot.UpgradeToolTip, Does.StartWith("Choose which creature"));
            Assert.That(this.selections.Single().Creature, Is.EqualTo("Sharpshooter"));
        }

        [Test]
        public void AnUpgradeThatIsNotOneOfTheSlotsIsRefused()
        {
            var slot = this.Slot();
            slot.InitializeSlot("Archer", 5, false, false, upgrades: ["Marksman"]);
            this.selections.Clear();

            slot.UpgradeCommand.Execute("Titan");

            Assert.That(this.selections, Is.Empty);
        }

        [Test]
        public void AnEmptySlotTellsToPickACreatureBeforeUpgrading()
        {
            var slot = this.Slot();
            slot.InitializeSlot(null, 0, false, false, upgrades: ["Marksman"]);

            Assert.That(slot.HasUpgrade, Is.False);
            Assert.That(slot.UpgradeToolTip, Is.EqualTo("Pick a creature first"));
        }

        [Test]
        public void ThePickerOpensAndClosesWithItsButtonAndTellsWhenItChanges()
        {
            var slot = this.Slot();
            var raised = new List<string?>();
            slot.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            slot.OpenPickerCommand.Execute(null);
            Assert.That(slot.IsPickerOpen, Is.True);

            slot.OpenPickerCommand.Execute(null);
            Assert.That(slot.IsPickerOpen, Is.False);
            Assert.That(raised.Count(r => r == nameof(StackSlotViewModel.IsPickerOpen)), Is.EqualTo(2));
        }

        [Test]
        public void SettingThePickerToTheStateItIsInTellsNothing()
        {
            var slot = this.Slot();
            var raised = new List<string?>();
            slot.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            slot.IsPickerOpen = false;

            Assert.That(raised, Is.Empty);
        }

        [Test]
        public void ASlotWithNoPickerAttachedHasNoFinder()
        {
            Assert.That(this.Slot().PickerFinder, Is.Null);
        }

        [Test]
        public void ThePickerIsMadeOnlyWhenFirstAskedForAndPickingFromItClosesItAndSetsTheCreature()
        {
            var slot = this.Slot();
            Action<string>? picked = null;
            var made = 0;
            slot.AttachPicker(onPicked =>
            {
                made++;
                picked = onPicked;
                return null!;
            });
            slot.IsPickerOpen = true;
            Assert.That(made, Is.Zero);

            _ = slot.PickerFinder;
            _ = slot.PickerFinder;
            picked!("Angel");

            Assert.That(made, Is.EqualTo(1));
            Assert.That((slot.IsPickerOpen, slot.SelectedCreature), Is.EqualTo((false, "Angel")));
        }
    }
}
