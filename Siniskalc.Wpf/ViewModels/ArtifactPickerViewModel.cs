namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// One place an artifact can go, such as the helm of a hero or a slot of its backpack: it shows the artifact in it, and a popup with a finder
    /// that lists only the artifacts that fit.
    /// </summary>
    public class ArtifactPickerViewModel : AbstractViewModelBase
    {
        private readonly Lazy<AbstractFinderViewModel> finder;
        private string? artifactName;
        private bool isOpen;

        /// <param name="label">The name of the slot, such as "Helm" or "Slot 3".</param>
        /// <param name="createFinder">
        /// Makes the finder for the popup, given what to do with the name of the artifact when the user picks a row. The owner of the slot
        /// decides which artifacts the finder offers; the slot only asks for it when the popup is first shown.
        /// </param>
        /// <param name="onPicked">Called with the name of the artifact the user picked, or null when the user clears the slot.</param>
        public ArtifactPickerViewModel(string label, Func<Action<string>, AbstractFinderViewModel> createFinder, Action<string?> onPicked)
        {
            this.Label = label;
            this.finder = new Lazy<AbstractFinderViewModel>(() => createFinder(name => this.Pick(name, onPicked)));
            this.OpenCommand = new DelegateCommand(_ => this.IsOpen = !this.IsOpen);
            this.ClearCommand = new DelegateCommand(_ => this.Pick(null, onPicked), _ => this.HasArtifact);
        }

        public string Label { get; }

        /// <summary>
        /// Gets the name of the artifact in the slot, or null when it is empty. Set it with <see cref="Show"/>.
        /// </summary>
        public string? ArtifactName => this.artifactName;

        public bool HasArtifact => !string.IsNullOrEmpty(this.artifactName);

        /// <summary>
        /// Gets the text of the button: the artifact, or a hint that the slot is empty.
        /// </summary>
        public string DisplayText => this.HasArtifact ? this.artifactName! : "(empty)";

        /// <summary>
        /// Gets or sets whether the picker popup is open.
        /// </summary>
        public bool IsOpen
        {
            get => this.isOpen;
            set
            {
                if (this.isOpen != value)
                {
                    this.isOpen = value;
                    this.RaisePropertyChanged(nameof(this.IsOpen));
                }
            }
        }

        /// <summary>
        /// Gets the finder in the popup. It is built when first needed, so a page with many slots opens quickly.
        /// </summary>
        public AbstractFinderViewModel Finder => this.finder.Value;

        public ICommand OpenCommand { get; }

        public ICommand ClearCommand { get; }

        /// <summary>
        /// Sets the artifact the slot holds to <paramref name="name"/>, as the model says it is. Changes <see cref="ArtifactName"/>,
        /// <see cref="HasArtifact"/> and <see cref="DisplayText"/>, but does not report it as a pick by the user.
        /// </summary>
        public void UpdateFromModel(string? name)
        {
            if (this.artifactName == name)
            {
                return;
            }

            this.artifactName = name;
            this.RaisePropertyChanged(nameof(this.ArtifactName));
            this.RaisePropertyChanged(nameof(this.HasArtifact));
            this.RaisePropertyChanged(nameof(this.DisplayText));
        }

        private void Pick(string? name, Action<string?> onPicked)
        {
            this.IsOpen = false;
            onPicked(name);
        }
    }
}
