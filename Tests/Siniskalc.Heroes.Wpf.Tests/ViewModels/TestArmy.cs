namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>A finder that holds nothing and counts how often it was refreshed.</summary>
    internal sealed class TestFinder : AbstractFinderViewModel
    {
        public TestFinder(Action<string>? onPicked = null)
            : base("Creatures", "creatures", [], new ListCollectionView(new List<object>()))
        {
            this.OnPicked = onPicked;
        }

        public Action<string>? OnPicked { get; }

        protected override int TotalCount => 0;

        public override void ChooseRow(object? row)
        {
        }

        public override void ChooseRowSecondary(object? row)
        {
        }

        protected override bool CanChooseRow(object row) => false;

        protected override bool CanChooseRowSecondary(object row) => false;

        protected override bool MatchesRow(object row) => true;
    }

    /// <summary>
    /// An army of four slots over a list of stacks made by the test, that notes everything the base class asks it to do.
    /// </summary>
    internal sealed class TestArmy : ArmyViewModelBase<StackSlotViewModel>
    {
        private readonly TestFinder finder = new();

        public TestArmy(Func<bool>? isReadOnly = null)
            : base(["Peasant", "Archer"], isReadOnly)
        {
            this.CreateSlots(4, (index, changed, move, clear) => new StackSlotViewModel(index, changed, move, clear));
        }

        public StackContent?[] Stacks { get; } = new StackContent?[4];

        public List<string> Calls { get; } = [];

        public List<int> SlotsRead { get; } = [];

        public string SummaryText { get; set; } = "summary";

        /// <summary>What the game does to a slot right after it was read, as a game that edits a slot while the page is refreshed would.</summary>
        public Action<StackSlotViewModel>? OnRead { get; set; }

        public override AbstractFinderViewModel Finder => this.finder;

        protected override AbstractFinderViewModel CreatePickerFinder(Action<string> onPicked) => new TestFinder(onPicked);

        protected override StackContent? ReadStack(int index) => this.Stacks[index];

        protected override void PutStack(int index, string? creatureName, int amount)
        {
            this.Calls.Add($"put {index} {creatureName} {amount}");
            this.Stacks[index] = string.IsNullOrEmpty(creatureName) ? null : new StackContent(creatureName, amount);
        }

        protected override void ClearStack(int index)
        {
            this.Calls.Add($"clear {index}");
            this.Stacks[index] = null;
        }

        protected override void SwapStacks(int first, int second)
        {
            this.Calls.Add($"swap {first} {second}");
            (this.Stacks[first], this.Stacks[second]) = (this.Stacks[second], this.Stacks[first]);
        }

        protected override string Summarize() => this.SummaryText;

        protected override void AfterSlotRead(StackSlotViewModel slot, StackContent? stack)
        {
            this.SlotsRead.Add(slot.SlotNumber);
            this.OnRead?.Invoke(slot);
        }
    }
}
