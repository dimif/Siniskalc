namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using Siniskalc.Wpf.ViewModels;

    public sealed class BackpackSlotViewModel : AbstractViewModelBase
    {
        private readonly Action<int, string?> onSelectionChanged;
        private string? selectedArtifact;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackpackSlotViewModel"/> class.
        /// </summary>
        /// <param name="slotNumber">The slot number.</param>
        /// <param name="onSelectionChanged">The on selection changed.</param>
        /// <param name="createFinder">The create finder.</param>
        public BackpackSlotViewModel(int slotNumber, Action<int, string?> onSelectionChanged, Func<Action<ArtifactChoiceViewModel>, ArtifactFinderViewModel> createFinder)
        {
            this.SlotNumber = slotNumber;
            this.onSelectionChanged = onSelectionChanged;
            this.Picker = new ArtifactSlotViewModel($"Slot {slotNumber}", createFinder, name => this.SelectedArtifact = name);
        }

        /// <summary>
        /// Gets the number of the slot, from 1.
        /// </summary>
        public int SlotNumber { get; }

        /// <summary>
        /// Gets what the slot shows, and the popup that picks something else for it.
        /// </summary>
        public ArtifactSlotViewModel Picker { get; }

        /// <summary>
        /// Gets the label of the slot.
        /// </summary>
        public string SlotLabel => $"Slot {this.SlotNumber}";

        /// <summary>
        /// Gets or sets the name of the artifact in the slot, or <see langword="null"/> when it is empty.
        /// </summary>
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
