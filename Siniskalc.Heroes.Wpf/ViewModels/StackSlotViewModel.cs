namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A single creature slot in an army: the creature and the size of the stack, the commands to move or clear the slot, and what the
    /// whole stack is worth (<see cref="Facts"/>, which the game gives). It knows no game: creatures are names.
    /// </summary>
    public class StackSlotViewModel : AbstractViewModelBase
    {
        private static readonly IReadOnlyList<StackFact> NoFacts = [];
        private static readonly IReadOnlyList<string> NoUpgrades = [];

        private readonly Action<int, string?, int> onSelectionChanged;
        private Func<int, IReadOnlyList<StackFact>>? factsOf;
        private string? selectedCreature;
        private int creatureAmount;
        private bool canMoveUp;
        private bool canMoveDown;
        private IReadOnlyList<string> upgrades = NoUpgrades;
        private Lazy<AbstractFinderViewModel>? pickerFinder;
        private ICommand? openPickerCommand;
        private bool isPickerOpen;

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
            this.MoveUpCommand = new DelegateCommand(_ => onMove(slotNumber, -1), _ => this.canMoveUp);
            this.MoveDownCommand = new DelegateCommand(_ => onMove(slotNumber, 1), _ => this.canMoveDown);
            this.ClearCommand = new DelegateCommand(_ => onClear(slotNumber), _ => this.HasCreature);
            this.UpgradeCommand = new DelegateCommand(this.Upgrade, _ => this.HasUpgrade);
        }

        public int SlotNumber { get; }

        /// <summary>
        /// Gets the command that opens or closes the popup with the finder of creatures to put in the slot.
        /// </summary>
        public ICommand OpenPickerCommand => this.openPickerCommand ??= new DelegateCommand(_ => this.IsPickerOpen = !this.IsPickerOpen);

        /// <summary>
        /// Gets or sets whether the popup with the creature finder is open.
        /// </summary>
        public bool IsPickerOpen
        {
            get => this.isPickerOpen;
            set
            {
                if (this.isPickerOpen != value)
                {
                    this.isPickerOpen = value;
                    this.RaisePropertyChanged(nameof(this.IsPickerOpen));
                }
            }
        }

        /// <summary>
        /// Gets the finder in the popup, which puts the creature the user picks in the slot. It is built when first needed, so an army with
        /// many slots opens quickly. <c>null</c> until <see cref="AttachPicker"/> was called.
        /// </summary>
        public AbstractFinderViewModel? PickerFinder => this.pickerFinder?.Value;

        /// <summary>
        /// Text of the creature button: the creature, or a hint that the slot is empty.
        /// </summary>
        public string CreatureButtonText => this.HasCreature ? this.selectedCreature! : "(empty)";

        /// <summary>
        /// Lets the slot offer a finder of creatures in a popup.
        /// </summary>
        /// <param name="createFinder">Makes the finder, given what to do with the name of the creature when the user picks a row.</param>
        public void AttachPicker(Func<Action<string>, AbstractFinderViewModel> createFinder)
        {
            this.pickerFinder = new Lazy<AbstractFinderViewModel>(() => createFinder(name =>
            {
                this.IsPickerOpen = false;
                this.SelectedCreature = name;
            }));
        }

        /// <summary>
        /// Gets the slot as the player sees it, from 1.
        /// </summary>
        public int DisplayNumber => this.SlotNumber + 1;

        public ICommand MoveUpCommand { get; }

        public ICommand MoveDownCommand { get; }

        public ICommand ClearCommand { get; }

        /// <summary>
        /// Gets the command that upgrades the creature of the slot, keeping the size of the stack. Its parameter is the name of the creature to
        /// upgrade to; without one it upgrades to the first of <see cref="Upgrades"/>.
        /// </summary>
        public ICommand UpgradeCommand { get; }

        /// <summary>
        /// Gets the creatures the one in the slot can be upgraded to: none when it is fully upgraded, one for most, several for a few.
        /// </summary>
        public IReadOnlyList<string> Upgrades => this.upgrades;

        public bool HasUpgrade => this.HasCreature && this.upgrades.Count > 0;

        public bool HasSeveralUpgrades => this.upgrades.Count > 1;

        /// <summary>
        /// Gets the text of the upgrade button; an arrow tells that it opens a list to choose from.
        /// </summary>
        public string UpgradeText => this.HasSeveralUpgrades ? "Upgrade ▾" : "Upgrade";

        public string UpgradeToolTip => !this.HasCreature ? "Pick a creature first"
            : this.upgrades.Count == 0 ? "This creature cannot be upgraded further"
            : this.upgrades.Count == 1 ? $"Change the stack into {this.upgrades[0]}, keeping its size"
            : "Choose which creature to change the stack into, keeping its size";

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
                this.RaisePropertyChanged(nameof(this.CreatureButtonText));
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
        /// <param name="canMoveUp">Whether there is a slot above to swap with.</param>
        /// <param name="canMoveDown">Whether there is a slot below to swap with.</param>
        /// <param name="factsOf">Gives the facts of a stack of a size, or <c>null</c> when the game has none.</param>
        /// <param name="upgrades">The creatures the slot's creature can be upgraded to; none when omitted.</param>
        public void InitializeSlot(string? creatureName, int amount, bool canMoveUp, bool canMoveDown, Func<int, IReadOnlyList<StackFact>>? factsOf = null, IReadOnlyList<string>? upgrades = null)
        {
            this.selectedCreature = creatureName;
            this.creatureAmount = amount;
            this.canMoveUp = canMoveUp;
            this.canMoveDown = canMoveDown;
            this.factsOf = factsOf;
            this.upgrades = upgrades ?? NoUpgrades;

            this.RaisePropertyChanged(nameof(this.SelectedCreature));
            this.RaisePropertyChanged(nameof(this.CreatureButtonText));
            this.RaisePropertyChanged(nameof(this.CreatureAmount));
            this.RaisePropertyChanged(nameof(this.HasCreature));
            this.RaiseFactsChanged();
            this.RaisePropertyChanged(nameof(this.Upgrades));
            this.RaisePropertyChanged(nameof(this.HasUpgrade));
            this.RaisePropertyChanged(nameof(this.HasSeveralUpgrades));
            this.RaisePropertyChanged(nameof(this.UpgradeText));
            this.RaisePropertyChanged(nameof(this.UpgradeToolTip));
            ((DelegateCommand)this.UpgradeCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.MoveUpCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.MoveDownCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.ClearCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Tells the screen that what the stack is worth changed. A game that shows it in other properties raises them here too.
        /// </summary>
        protected virtual void RaiseFactsChanged() => this.RaisePropertyChanged(nameof(this.Facts));

        private void Upgrade(object? parameter)
        {
            var name = parameter as string ?? this.upgrades.FirstOrDefault();
            if (name != null && this.upgrades.Contains(name))
            {
                this.SelectedCreature = name;
            }
        }
    }
}
