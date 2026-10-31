namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The army of a hero or the garrison of a castle: its slots, and a finder to look up creatures to add. It knows no game: the game's
    /// view model says what is in a slot and how to change it, and the view (<c>ArmyEditorView</c>) only binds to this.
    /// Call <see cref="Refresh"/> after the army changed.
    /// </summary>
    /// <typeparam name="TSlot">The slot view model; a game can add what it shows of a stack.</typeparam>
    public abstract class ArmyViewModelBase<TSlot> : PageViewModelBase
        where TSlot : StackSlotViewModel
    {
        private readonly Func<bool>? isReadOnly;
        private bool isRefreshing;

        protected ArmyViewModelBase(IReadOnlyList<string> availableCreatures, Func<bool>? isReadOnly = null)
            : base("Creatures")
        {
            this.isReadOnly = isReadOnly;

            // An empty entry first, so a slot can be left empty.
            this.AvailableCreatures = [string.Empty, .. availableCreatures];
        }

        /// <summary>
        /// Gets the heading of the army card: "Army", or what the owner set with <see cref="SetTitle"/>, such as "Garrison of the sawmill at ...".
        /// </summary>
        public string Title { get; private set; } = "Army";

        /// <summary>
        /// Gets the finder that looks up creatures and adds them to the army.
        /// </summary>
        public abstract AbstractFinderViewModel Finder { get; }

        /// <summary>
        /// Gets the creature slots of the army, in order.
        /// </summary>
        public ObservableCollection<TSlot> CreatureSlots { get; } = [];

        /// <summary>
        /// Gets the creature names to pick from, with an empty entry first.
        /// </summary>
        public IReadOnlyList<string> AvailableCreatures { get; }

        /// <summary>
        /// Gets whether the army can be edited. False for the garrison of a castle while a hero is garrisoned in it.
        /// </summary>
        public bool IsEditable => !(this.isReadOnly?.Invoke() ?? false);

        /// <summary>
        /// Gets what the whole army is worth, such as "HP Total 1,200 · Cost 5,000". Empty for an empty army, or when the game has none.
        /// </summary>
        public string ArmySummary { get; private set; } = string.Empty;

        /// <summary>
        /// Gets whether the army has an empty slot for a creature from the finder.
        /// </summary>
        public bool HasFreeSlot => this.FirstFreeSlot() >= 0;

        /// <summary>
        /// Changes the heading of the army card.
        /// </summary>
        public void SetTitle(string title)
        {
            if (this.Title != title)
            {
                this.Title = title;
                this.RaisePropertyChanged(nameof(this.Title));
            }
        }

        /// <summary>
        /// Shows the current army again. Call after the army changed.
        /// </summary>
        public void Refresh()
        {
            this.isRefreshing = true;
            try
            {
                for (var i = 0; i < this.CreatureSlots.Count; i++)
                {
                    var stack = this.ReadStack(i);
                    this.CreatureSlots[i].InitializeSlot(string.IsNullOrEmpty(stack?.CreatureName) ? null : stack.CreatureName, stack?.Amount ?? 0, i > 0, i < this.CreatureSlots.Count - 1, stack?.FactsOf, stack?.Upgrades);
                    this.AfterSlotRead(this.CreatureSlots[i], stack);
                }
            }
            finally
            {
                this.isRefreshing = false;
            }

            this.ArmySummary = this.Summarize();
            this.RaisePropertyChanged(nameof(this.ArmySummary));

            // The finder's add buttons and summary depend on whether a slot is free.
            this.RaisePropertyChanged(nameof(this.HasFreeSlot));
            this.RaisePropertyChanged(nameof(this.IsEditable));
            this.Finder.Refresh();
        }

        /// <summary>
        /// Swaps the stack in a slot with the one next to it. <paramref name="direction"/> is -1 for left, 1 for right.
        /// An empty neighbour just receives the stack.
        /// </summary>
        public void MoveSlot(int index, int direction)
        {
            var target = index + direction;
            if (this.isRefreshing || index < 0 || index >= this.CreatureSlots.Count || target < 0 || target >= this.CreatureSlots.Count)
            {
                return;
            }

            this.SwapStacks(index, target);
        }

        /// <summary>
        /// Empties a slot.
        /// </summary>
        public void ClearSlot(int index)
        {
            if (this.isRefreshing || index < 0 || index >= this.CreatureSlots.Count)
            {
                return;
            }

            this.ClearStack(index);
        }

        /// <summary>
        /// Makes the slots. The game's view model calls this once, from its constructor, before <see cref="Refresh"/>.
        /// </summary>
        protected void CreateSlots(int count, Func<int, Action<int, string?, int>, Action<int, int>, Action<int>, TSlot> create)
        {
            foreach (var i in Enumerable.Range(0, count))
            {
                var slot = create(i, this.OnSlotChanged, this.MoveSlot, this.ClearSlot);
                slot.AttachPicker(this.CreatePickerFinder);
                this.CreatureSlots.Add(slot);
            }
        }

        /// <summary>
        /// Makes the finder in the popup of a slot, which calls <paramref name="onPicked"/> with the name of the creature the user picks.
        /// </summary>
        protected abstract AbstractFinderViewModel CreatePickerFinder(Action<string> onPicked);

        /// <summary>
        /// Reads what is in a slot (from 0) of the model, or <c>null</c> when it is empty.
        /// </summary>
        protected abstract StackContent? ReadStack(int index);

        /// <summary>
        /// Puts a stack of <paramref name="amount"/> of the creature called <paramref name="creatureName"/> in a slot (from 0), or empties it
        /// when there is no such creature.
        /// </summary>
        protected abstract void PutStack(int index, string? creatureName, int amount);

        protected abstract void ClearStack(int index);

        protected abstract void SwapStacks(int first, int second);

        /// <summary>
        /// Tells what the whole army is worth; nothing when the game has no such thing.
        /// </summary>
        protected virtual string Summarize() => string.Empty;

        /// <summary>
        /// Lets the game's slot take what it shows besides the facts, after the slot was read.
        /// </summary>
        protected virtual void AfterSlotRead(TSlot slot, StackContent? stack)
        {
        }

        /// <summary>
        /// Gets the first empty slot, from 0, or -1 when the army is full.
        /// </summary>
        protected int FirstFreeSlot()
        {
            for (var i = 0; i < this.CreatureSlots.Count; i++)
            {
                if (this.ReadStack(i) == null)
                {
                    return i;
                }
            }

            return -1;
        }

        private void OnSlotChanged(int index, string? creatureName, int amount)
        {
            if (!this.isRefreshing)
            {
                this.PutStack(index, creatureName, amount);
            }
        }
    }
}
