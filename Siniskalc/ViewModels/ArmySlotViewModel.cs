namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Input;
    using Siniskalc.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for a single creature slot in an army.
    /// Manages both the creature type and its army size, and the commands to move or clear the slot.
    /// </summary>
    public sealed class ArmySlotViewModel : AbstractViewModelBase
    {
        private readonly Action<int, string?, int> onSelectionChanged;
        private string? selectedCreature;
        private int creatureAmount;
        private bool canMoveLeft;
        private bool canMoveRight;
        private CreatureInfo? info;

        /// <param name="slotNumber">The slot, from 0.</param>
        /// <param name="onSelectionChanged">Called with the slot, creature name and amount when the user changes the slot.</param>
        /// <param name="onMove">Called with the slot and direction (-1 or 1) when the user moves the slot.</param>
        /// <param name="onClear">Called with the slot when the user clears it.</param>
        public ArmySlotViewModel(
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
                this.RaiseTotalsChanged();
                this.onSelectionChanged(this.SlotNumber, this.selectedCreature, value);
            }
        }

        /// <summary>
        /// Gets the hit points of the whole stack.
        /// </summary>
        public string TotalHealth => this.info is { } i ? this.Format((long)i.Health * this.creatureAmount) : string.Empty;

        /// <summary>
        /// Gets the damage the whole stack deals, as a range such as "100-150", or one number when it does not vary.
        /// </summary>
        public string TotalDamage
        {
            get
            {
                if (this.info is not { } i)
                {
                    return string.Empty;
                }

                var min = (long)i.MinDamage * this.creatureAmount;
                var max = (long)i.MaxDamage * this.creatureAmount;
                return min == max ? this.Format(min) : $"{this.Format(min)}-{this.Format(max)}";
            }
        }

        /// <summary>
        /// Gets the gold the whole stack costs.
        /// </summary>
        public string TotalCost => this.info is { } i ? this.Format((long)i.Gold * this.creatureAmount) : string.Empty;

        /// <summary>
        /// Gets the value the computer player gives the whole stack when it decides what to fight.
        /// </summary>
        public string TotalAiValue => this.info is { } i ? this.Format((long)i.AiValue * this.creatureAmount) : string.Empty;

        /// <summary>
        /// Gets the fight value of the whole stack: how much it counts for in the strength of an army.
        /// </summary>
        public string TotalFightValue => this.info is { } i ? this.Format((long)i.FightValue * this.creatureAmount) : string.Empty;

        /// <summary>
        /// Gets what the stack costs besides gold, such as "and 12 wood, 3 gems"; empty when it costs only gold.
        /// </summary>
        public string TotalOtherCost
        {
            get
            {
                if (this.info is not { } i)
                {
                    return string.Empty;
                }

                var parts = new List<string>();
                this.AddResource(parts, "wood", i.Wood);
                this.AddResource(parts, "ore", i.Ore);
                this.AddResource(parts, "mercury", i.Mercury);
                this.AddResource(parts, "sulfur", i.Sulfur);
                this.AddResource(parts, "crystal", i.Crystal);
                this.AddResource(parts, "gems", i.Gems);
                return parts.Count == 0 ? string.Empty : "and " + string.Join(", ", parts);
            }
        }

        /// <summary>
        /// Shows the slot's creature and amount without reporting a change.
        /// </summary>
        public void InitializeSlot(string? creatureName, int amount, bool canMoveLeft, bool canMoveRight, CreatureInfo? info = null)
        {
            this.selectedCreature = creatureName;
            this.creatureAmount = amount;
            this.canMoveLeft = canMoveLeft;
            this.canMoveRight = canMoveRight;
            this.info = info;

            this.RaisePropertyChanged(nameof(this.SelectedCreature));
            this.RaisePropertyChanged(nameof(this.CreatureAmount));
            this.RaisePropertyChanged(nameof(this.HasCreature));
            this.RaiseTotalsChanged();
            ((DelegateCommand)this.MoveLeftCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.MoveRightCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.ClearCommand).RaiseCanExecuteChanged();
        }

        private string Format(long number) => number.ToString("N0", CultureInfo.CurrentCulture);

        private void AddResource(List<string> parts, string name, short perCreature)
        {
            if (perCreature != 0)
            {
                parts.Add($"{this.Format((long)perCreature * this.creatureAmount)} {name}");
            }
        }

        private void RaiseTotalsChanged()
        {
            this.RaisePropertyChanged(nameof(this.TotalHealth));
            this.RaisePropertyChanged(nameof(this.TotalDamage));
            this.RaisePropertyChanged(nameof(this.TotalCost));
            this.RaisePropertyChanged(nameof(this.TotalOtherCost));
            this.RaisePropertyChanged(nameof(this.TotalAiValue));
            this.RaisePropertyChanged(nameof(this.TotalFightValue));
        }
    }
}
