namespace Siniskalc.ViewModels
{
    using System;
    using Siniskalc.Wpf.ViewModels;

    public sealed class BackpackSlotViewModel : AbstractViewModelBase
    {
        private readonly Action<int, string?> onSelectionChanged;
        private string? selectedArtifact;

        public BackpackSlotViewModel(int slotNumber, Action<int, string?> onSelectionChanged, Func<Action<ArtifactChoiceViewModel>, ArtifactFinderViewModel> createFinder)
        {
            this.SlotNumber = slotNumber;
            this.onSelectionChanged = onSelectionChanged;
            this.Picker = new ArtifactSlotViewModel($"Slot {slotNumber}", createFinder, name => this.SelectedArtifact = name);
        }

        public int SlotNumber { get; }

        /// <summary>
        /// Gets what the slot shows, and the popup that picks something else for it.
        /// </summary>
        public ArtifactSlotViewModel Picker { get; }

        public string SlotLabel => $"Slot {this.SlotNumber}";

        public string? SelectedArtifact
        {
            get => this.selectedArtifact;
            set
            {
                if (this.selectedArtifact == value)
                {
                    return;
                }

                this.selectedArtifact = value;
                this.Picker.UpdateFromModel(value);
                this.RaisePropertyChanged(nameof(this.SelectedArtifact));
                this.onSelectionChanged(this.SlotNumber, value);
            }
        }
    }
}
