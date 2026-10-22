namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One place an artifact can go, such as the helm or a backpack slot: it shows the artifact in it, and a popup with a finder
    /// that lists only the artifacts that fit.
    /// </summary>
    public sealed class ArtifactSlotViewModel : ArtifactPickerViewModel
    {
        /// <param name="label">The name of the slot, such as "Helm" or "Slot 3".</param>
        /// <param name="createFinder">
        /// Makes the finder for the popup, given what to do when the user picks a row. The owner of the slot decides which artifacts
        /// the finder offers; the slot only asks for it when the popup is first shown.
        /// </param>
        /// <param name="onPicked">Called with the name of the artifact the user picked, or null when the user clears the slot.</param>
        public ArtifactSlotViewModel(string label, Func<Action<ArtifactChoiceViewModel>, ArtifactFinderViewModel> createFinder, Action<string?> onPicked)
            : base(label, onPick => createFinder(choice => onPick(choice.Name)), onPicked)
        {
        }

        /// <summary>
        /// Gets the finder in the popup. It is built when first needed, so a page with many slots opens quickly.
        /// </summary>
        public new ArtifactFinderViewModel Finder => (ArtifactFinderViewModel)base.Finder;
    }
}
