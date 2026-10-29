namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for a single creature slot in an army of Heroes of Might and Magic III: the shared slot, and the totals of the whole stack
    /// (<see cref="StackTotals"/>), which are also in its <see cref="StackSlotViewModel.Facts"/>.
    /// </summary>
    public sealed class ArmySlotViewModel : StackSlotViewModel
    {
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
            : base(slotNumber, onSelectionChanged, onMove, onClear)
        {
        }

        public string TotalHealth => this.Totals?.Health ?? string.Empty;

        public string TotalDamage => this.Totals?.Damage ?? string.Empty;

        public string TotalCost => this.Totals?.Cost ?? string.Empty;

        public string TotalAiValue => this.Totals?.AiValue ?? string.Empty;

        public string TotalFightValue => this.Totals?.FightValue ?? string.Empty;

        public string TotalOtherCost => this.Totals?.OtherCost ?? string.Empty;

        private StackTotals? Totals => this.info is { } i ? new StackTotals(i, this.CreatureAmount) : null;

        /// <summary>
        /// Takes the stats of the slot's creature, which the totals are worked out from.
        /// </summary>
        public void SetInfo(CreatureInfo? creatureInfo)
        {
            this.info = creatureInfo;
            this.RaiseFactsChanged();
        }

        /// <inheritdoc/>
        protected override void RaiseFactsChanged()
        {
            base.RaiseFactsChanged();
            this.RaisePropertyChanged(nameof(this.TotalHealth));
            this.RaisePropertyChanged(nameof(this.TotalDamage));
            this.RaisePropertyChanged(nameof(this.TotalCost));
            this.RaisePropertyChanged(nameof(this.TotalOtherCost));
            this.RaisePropertyChanged(nameof(this.TotalAiValue));
            this.RaisePropertyChanged(nameof(this.TotalFightValue));
        }
    }
}
