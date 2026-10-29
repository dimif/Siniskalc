namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests what every game's army editor does: shows the stacks in its slots, writes an edit of a slot back, moves and clears slots,
    /// and keeps its summary and finder up to date. The game's part is a fake that notes what it was asked.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ArmyViewModelBaseTests
    {
        [Test]
        public void AnArmyHasItsSlotsAndTheCreaturesToPickFromWithAnEmptyEntryFirst()
        {
            var army = new TestArmy();

            Assert.That(army.CreatureSlots, Has.Count.EqualTo(4));
            Assert.That(army.AvailableCreatures, Is.EqualTo(new[] { string.Empty, "Peasant", "Archer" }));
            Assert.That(army.CreatureSlots.Select(s => s.SlotNumber), Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void TheTitleIsArmyUntilAnotherIsSetAndTellsOnlyWhenItChanges()
        {
            var army = new TestArmy();
            var raised = new List<string?>();
            army.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            Assert.That(army.Title, Is.EqualTo("Army"));

            army.SetTitle("Garrison");
            army.SetTitle("Garrison");

            Assert.That(army.Title, Is.EqualTo("Garrison"));
            Assert.That(raised.Count(r => r == "Title"), Is.EqualTo(1));
        }

        [Test]
        public void TheArmyIsEditableUnlessItIsReadOnly()
        {
            Assert.That(new TestArmy().IsEditable, Is.True);
            Assert.That(new TestArmy(() => false).IsEditable, Is.True);
            Assert.That(new TestArmy(() => true).IsEditable, Is.False);
        }

        [Test]
        public void RefreshShowsTheStacksInTheirSlotsAndLeavesFreeSlotsEmpty()
        {
            var army = new TestArmy();
            army.Stacks[0] = new StackContent("Peasant", 10);
            army.Stacks[2] = new StackContent("Archer", 4);

            army.Refresh();

            Assert.That(army.CreatureSlots.Select(s => (s.SelectedCreature, s.CreatureAmount)), Is.EqualTo(new (string?, int)[] { ("Peasant", 10), (null, 0), ("Archer", 4), (null, 0) }));
        }

        [Test]
        public void AStackWithNoNameIsShownAsAnEmptySlot()
        {
            var army = new TestArmy();
            army.Stacks[1] = new StackContent(string.Empty, 5);

            army.Refresh();

            Assert.That(army.CreatureSlots[1].HasCreature, Is.False);
            Assert.That(army.CreatureSlots[1].SelectedCreature, Is.Null);
        }

        [Test]
        public void TheFirstSlotCannotMoveUpAndTheLastCannotMoveDown()
        {
            var army = new TestArmy();
            army.Stacks[0] = new StackContent("Peasant", 1);
            army.Stacks[3] = new StackContent("Archer", 1);

            army.Refresh();

            Assert.That(army.CreatureSlots.Select(s => s.MoveUpCommand.CanExecute(null)), Is.EqualTo(new[] { false, true, true, true }));
            Assert.That(army.CreatureSlots.Select(s => s.MoveDownCommand.CanExecute(null)), Is.EqualTo(new[] { true, true, true, false }));
        }

        [Test]
        public void RefreshingWritesNothingBackToTheGameAndLetsTheGameReadEachSlot()
        {
            var army = new TestArmy();
            army.Stacks[0] = new StackContent("Peasant", 10);

            army.Refresh();

            Assert.That(army.Calls, Is.Empty);
            Assert.That(army.SlotsRead, Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void RefreshingTellsTheSummaryTheFreeSlotAndTheEditableFlagAndRefreshesTheFinder()
        {
            var army = new TestArmy();
            army.SummaryText = "HP Total 100";
            var raised = new List<string?>();
            army.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            army.Refresh();

            Assert.That(army.ArmySummary, Is.EqualTo("HP Total 100"));
            Assert.That(raised, Is.SupersetOf(new[] { nameof(TestArmy.ArmySummary), nameof(TestArmy.HasFreeSlot), nameof(TestArmy.IsEditable) }));
        }

        [Test]
        public void ASlotEditedWhileThePageIsBeingRefreshedIsNotWrittenBackToTheGame()
        {
            var army = new TestArmy { OnRead = slot => slot.CreatureAmount += 1 };
            army.Stacks[0] = new StackContent("Peasant", 10);

            army.Refresh();

            Assert.That(army.Calls, Is.Empty);
        }

        [Test]
        public void AMoveOrAClearAskedWhileThePageIsBeingRefreshedIsIgnored()
        {
            var army = new TestArmy();
            army.OnRead = _ =>
            {
                army.MoveSlot(1, 1);
                army.ClearSlot(2);
            };

            army.Refresh();

            Assert.That(army.Calls, Is.Empty);
        }

        [Test]
        public void RefreshingTellsTheFinderSoItsSummaryAndButtonsFollowTheFreeSlots()
        {
            var army = new TestArmy();
            var raised = new List<string?>();
            army.Finder.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            army.Refresh();

            Assert.That(raised, Does.Contain("Summary"));
        }

        [Test]
        public void EditingASlotWritesTheStackBackToTheGame()
        {
            var army = new TestArmy();
            army.Refresh();

            army.CreatureSlots[1].SelectedCreature = "Archer";
            army.CreatureSlots[1].CreatureAmount = 7;

            Assert.That(army.Calls, Is.EqualTo(new[] { "put 1 Archer 0", "put 1 Archer 7" }));
        }

        [Test]
        public void MovingASlotSwapsItWithTheOneNextToItInThatDirection()
        {
            var army = new TestArmy();

            army.MoveSlot(1, 1);
            army.MoveSlot(2, -1);

            Assert.That(army.Calls, Is.EqualTo(new[] { "swap 1 2", "swap 2 1" }));
        }

        [TestCase(0, -1)]
        [TestCase(3, 1)]
        [TestCase(-1, 1)]
        [TestCase(4, -1)]
        public void MovingOffTheEndsOrFromOutsideTheArmyDoesNothing(int index, int direction)
        {
            var army = new TestArmy();

            army.MoveSlot(index, direction);

            Assert.That(army.Calls, Is.Empty);
        }

        [Test]
        public void ClearingASlotAsksTheGameToClearIt()
        {
            var army = new TestArmy();

            army.ClearSlot(2);

            Assert.That(army.Calls, Is.EqualTo(new[] { "clear 2" }));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void ClearingASlotOutsideTheArmyDoesNothing(int index)
        {
            var army = new TestArmy();

            army.ClearSlot(index);

            Assert.That(army.Calls, Is.Empty);
        }

        [Test]
        public void TheButtonsOfASlotMoveAndClearIt()
        {
            var army = new TestArmy();
            army.Stacks[1] = new StackContent("Peasant", 3);
            army.Refresh();

            army.CreatureSlots[1].MoveDownCommand.Execute(null);
            army.CreatureSlots[2].ClearCommand.Execute(null);

            Assert.That(army.Calls, Is.EqualTo(new[] { "swap 1 2", "clear 2" }));
        }

        [Test]
        public void ThereIsAFreeSlotWhileOneHoldsNoStackAndNoneWhenAllDo()
        {
            var army = new TestArmy();
            Assert.That(army.HasFreeSlot, Is.True);

            for (var i = 0; i < 4; i++)
            {
                army.Stacks[i] = new StackContent("Peasant", 1);
            }

            Assert.That(army.HasFreeSlot, Is.False);

            army.Stacks[2] = null;

            Assert.That(army.HasFreeSlot, Is.True);
        }

        [Test]
        public void EverySlotCanOpenAPickerFinderMadeByTheGameThatSetsItsCreature()
        {
            var army = new TestArmy();
            army.Refresh();

            var finder = (TestFinder)army.CreatureSlots[2].PickerFinder!;
            finder.OnPicked!("Archer");

            Assert.That(army.CreatureSlots[2].SelectedCreature, Is.EqualTo("Archer"));
            Assert.That(army.Calls, Is.EqualTo(new[] { "put 2 Archer 0" }));
        }
    }
}
