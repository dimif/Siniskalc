namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;

    /// <summary>
    /// A finder for creatures: picking one adds a new stack of <see cref="AddAmount"/> to the army, and the right-click menu sends it to a
    /// slot. The game gives the rows, the columns and what matches a search.
    /// </summary>
    /// <typeparam name="TRow">The row of a creature.</typeparam>
    public abstract class CreatureFinderViewModelBase<TRow> : FinderViewModel<TRow>
        where TRow : class
    {
        private readonly Func<bool> hasFreeSlot;
        private readonly Action<TRow, int, int>? sendToSlot;
        private readonly Func<int, string>? describeSlot;
        private readonly int slotCount;
        private readonly int minAmount;
        private readonly int maxAmount;
        private int addAmount = 1;

        /// <param name="rows">The creatures, in the order they are listed.</param>
        /// <param name="groupPath">The property the list is grouped by, such as the town; none when null.</param>
        /// <param name="columns">The columns of the list.</param>
        /// <param name="slotCount">How many slots the army has.</param>
        /// <param name="minAmount">The least a stack can have.</param>
        /// <param name="maxAmount">The most a stack can have.</param>
        /// <param name="hasFreeSlot">Tells whether the army has an empty slot for a creature.</param>
        /// <param name="add">Called with the creature and the size of the stack when the user picks one.</param>
        /// <param name="sendToSlot">Called with the creature, a slot (from 1) and a count when the user sends some of it there; the right-click menu is left out when null.</param>
        /// <param name="describeSlot">Says what is in a slot (from 1), for its menu entry.</param>
        protected CreatureFinderViewModelBase(
            IReadOnlyList<TRow> rows,
            string? groupPath,
            IReadOnlyList<FinderColumn> columns,
            int slotCount,
            int minAmount,
            int maxAmount,
            Func<bool> hasFreeSlot,
            Action<TRow, int> add,
            Action<TRow, int, int>? sendToSlot = null,
            Func<int, string>? describeSlot = null)
            : base(rows, groupPath, string.Empty, "creatures", columns)
        {
            this.slotCount = slotCount;
            this.minAmount = minAmount;
            this.maxAmount = maxAmount;
            this.hasFreeSlot = hasFreeSlot;
            this.sendToSlot = sendToSlot;
            this.describeSlot = describeSlot;

            this.SetAction("Add", choice => add(choice, this.addAmount), _ => this.hasFreeSlot());
            this.StartFiltering();
        }

        /// <summary>
        /// Gets or sets how many creatures a new stack from the finder starts with.
        /// </summary>
        public int AddAmount
        {
            get => this.addAmount;
            set
            {
                value = RangeGuard.Clamp(value, this.minAmount, this.maxAmount);
                if (this.addAmount != value)
                {
                    this.addAmount = value;
                    this.RaisePropertyChanged(nameof(this.AddAmount));
                }
            }
        }

        public override string Summary =>
            this.hasFreeSlot() ? base.Summary : $"{base.Summary}. All {this.slotCount} slots are in use; empty one to add more";

        /// <summary>
        /// Offers "Send 1 to slot" with one entry per slot; a slot that is taken gets its stack replaced by one creature.
        /// </summary>
        public override IReadOnlyList<FinderMenuItem> GetRowMenu(object? row)
        {
            if (row is not TRow choice || this.sendToSlot == null)
            {
                return [];
            }

            return [this.SendMenu(choice, 1), this.SendMenu(choice, 100)];
        }

        private FinderMenuItem SendMenu(TRow choice, int count)
        {
            var send = this.sendToSlot!;
            var slots = Enumerable.Range(1, this.slotCount)
                .Select(slot => new FinderMenuItem(
                    this.describeSlot == null ? $"Slot {slot}" : $"Slot {slot}: {this.describeSlot(slot)}",
                    () => send(choice, slot, count)))
                .ToList();
            return new FinderMenuItem($"Send {count} to slot", Children: slots);
        }
    }
}
