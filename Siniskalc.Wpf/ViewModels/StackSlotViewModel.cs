namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;

    /// <summary>
    /// A single creature slot in an army: the creature and the size of the stack, the commands to move or clear the slot, and what the
    /// whole stack is worth (<see cref="Facts"/>, which the game gives). It knows no game: creatures are names.
    /// </summary>
    public class StackSlotViewModel : AbstractViewModelBase
    {
        private static readonly IReadOnlyList<StackFact> NoFacts = [];

        private readonly Action<int, string?, int> onSelectionChanged;
        private Func<int, IReadOnlyList<StackFact>>? factsOf;
        private string? selectedCreature;
        private int creatureAmount;
        private bool canMoveLeft;
        private bool canMoveRight;

        /// <param name="slotNumber">The slot, from 0.</param>
        /// <param name="onSelectionChanged">Called with the slot, creature name and amount when the user changes the slot.</param>
        /// <param name="onMove">Called with the slot and direction (-1 or 1) when the user moves the slot.</param>
        /// <param name="onClear">Called with the slot when the user clears it.</param>
        public StackSlotViewModel(
            int slotNumber,
            Action<int, string?, int> onSelectionChanged,
            Action<int, int> onMove,
            Action<int> onClear)
        {
            this.SlotNumber = slotNumber;
            this.onSelectionChanged = onSelectionChanged;
            this.MoveLeftCommand = new DelegateCommand(_ => onMove(slotNumber, -1), _ => this.canMoveLeft);
            this.MoveRightCommand = new DelegateCommand(_ => onMove(slotNumber, 1), _ => this.canMoveRight);
            this.ClearCommand = new DelegateCommand(_ => onClear(slotNumber), _ => this.HasCreature);
        }

        public int SlotNumber { get; }

        /// <summary>
        /// Gets the slot as the player sees it, from 1.
        /// </summary>
        public int DisplayNumber => this.SlotNumber + 1;

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public ICommand ClearCommand { get; }

        public bool HasCreature => !string.IsNullOrEmpty(this.selectedCreature);

        public string? SelectedCreature
        {
            get => this.selectedCreature;
            set
            {
                if (this.selectedCreature == value)
                {
                    return;
                }

                this.selectedCreature = value;
                this.RaisePropertyChanged(nameof(this.SelectedCreature));
                this.onSelectionChanged(this.SlotNumber, value, this.creatureAmount);
            }
        }

        public int CreatureAmount
        {
            get => this.creatureAmount;
            set
            {
                if (this.creatureAmount == value)
                {
                    return;
                }

                this.creatureAmount = value;
                this.RaisePropertyChanged(nameof(this.CreatureAmount));
                this.RaiseFactsChanged();
                this.onSelectionChanged(this.SlotNumber, this.selectedCreature, value);
            }
        }

        /// <summary>
        /// Gets what the whole stack is worth, line by line; none for an empty slot.
        /// </summary>
        public IReadOnlyList<StackFact> Facts => this.HasCreature && this.factsOf != null ? this.factsOf(this.creatureAmount) : NoFacts;

        /// <summary>
        /// Shows the slot's creature and amount without reporting a change.
        /// </summary>
        /// <param name="creatureName">The creature, or nothing for an empty slot.</param>
        /// <param name="amount">The size of the stack.</param>
        /// <param name="canMoveLeft">Whether there is a slot on the left to swap with.</param>
        /// <param name="canMoveRight">Whether there is a slot on the right to swap with.</param>
        /// <param name="factsOf">Gives the facts of a stack of a size, or <c>null</c> when the game has none.</param>
        public void InitializeSlot(string? creatureName, int amount, bool canMoveLeft, bool canMoveRight, Func<int, IReadOnlyList<StackFact>>? factsOf = null)
        {
            this.selectedCreature = creatureName;
            this.creatureAmount = amount;
            this.canMoveLeft = canMoveLeft;
            this.canMoveRight = canMoveRight;
            this.factsOf = factsOf;

            this.RaisePropertyChanged(nameof(this.SelectedCreature));
            this.RaisePropertyChanged(nameof(this.CreatureAmount));
            this.RaisePropertyChanged(nameof(this.HasCreature));
            this.RaiseFactsChanged();
            ((DelegateCommand)this.MoveLeftCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.MoveRightCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.ClearCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Tells the screen that what the stack is worth changed. A game that shows it in other properties raises them here too.
        /// </summary>
        protected virtual void RaiseFactsChanged() => this.RaisePropertyChanged(nameof(this.Facts));
    }
}
