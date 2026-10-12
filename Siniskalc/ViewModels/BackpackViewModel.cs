namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    public sealed class BackpackViewModel : ArtifactInfoViewModelBase
    {
        private readonly Action<Backpack>? onChanged;
        private readonly GameVersion gameVersion;
        private Backpack model;
        private bool isRefreshing;

        /// <param name="effectLines">Words what artifacts do.</param>
        /// <param name="model">The backpack to show.</param>
        /// <param name="onChanged">Called with the new backpack whenever the user changes a slot.</param>
        /// <param name="gameVersion">The game version; the finder leaves out artifacts from later versions.</param>
        public BackpackViewModel(EffectLineProvider effectLines, Backpack model, Action<Backpack>? onChanged = null, GameVersion gameVersion = GameVersion.HornOfTheAbyss)
            : base("Backpack", effectLines)
        {
            this.model = model;
            this.onChanged = onChanged;
            this.Slots1To32 = new ObservableCollection<BackpackSlotViewModel>(Enumerable.Range(1, Constants.Backpack.SlotsPerPage).Select(i => new BackpackSlotViewModel(i, this.UpdateBackpackSlot, this.CreateSlotFinder)));
            this.Slots33To64 = new ObservableCollection<BackpackSlotViewModel>(Enumerable.Range(Constants.Backpack.SlotsPerPage + 1, Constants.Backpack.SlotsPerPage).Select(i => new BackpackSlotViewModel(i, this.UpdateBackpackSlot, this.CreateSlotFinder)));
            this.Finder = new ArtifactFinderViewModel(gameVersion, this.EffectLines, "Find artifacts", "Add", this.AddToBackpack, _ => this.FirstEmptySlot() != null, includeScrolls: true);
            this.gameVersion = gameVersion;
            this.Refresh(model);
        }

        /// <summary>
        /// Gets the finder that puts an artifact in the first empty backpack slot.
        /// </summary>
        public ArtifactFinderViewModel Finder { get; }

        public ObservableCollection<BackpackSlotViewModel> Slots1To32 { get; }

        public ObservableCollection<BackpackSlotViewModel> Slots33To64 { get; }

        /// <summary>
        /// Gets whether the backpack has an empty slot.
        /// </summary>
        public bool HasFreeSlot => this.FirstEmptySlot() != null;

        /// <summary>
        /// Shows <paramref name="model"/>, e.g. after an undo, without reporting it as a user change.
        /// </summary>
        public void Refresh(Backpack model)
        {
            this.model = model;
            this.isRefreshing = true;
            try
            {
                foreach (var slot in this.Slots1To32.Concat(this.Slots33To64))
                {
                    var index = slot.SlotNumber - 1;
                    slot.SelectedArtifact = index < model.All.Count ? NameOf(model.All[index]) : null;
                }
            }
            finally
            {
                this.isRefreshing = false;
            }

            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Puts <paramref name="artifact"/> in the first empty slot of the backpack. Does nothing when it is full.
        /// </summary>
        public void Add(Artifact artifact)
        {
            var slot = this.FirstEmptySlot();
            if (slot != null)
            {
                slot.SelectedArtifact = artifact.Name;
            }
        }

        private static string? NameOf(BackpackSlot? slot) =>
            slot?.Spell != null ? SpellScroll.NameOf(slot.Spell) : slot?.Art?.Name;

        private ArtifactFinderViewModel CreateSlotFinder(Action<ArtifactChoiceViewModel> onChoose) =>
            new(this.gameVersion, this.EffectLines, string.Empty, "Choose", onChoose, null, includeScrolls: true);

        private BackpackSlotViewModel? FirstEmptySlot() =>
            this.Slots1To32.Concat(this.Slots33To64).FirstOrDefault(s => string.IsNullOrEmpty(s.SelectedArtifact));

        private void AddToBackpack(ArtifactChoiceViewModel choice)
        {
            var slot = this.FirstEmptySlot();
            if (slot != null)
            {
                slot.SelectedArtifact = choice.Name;
            }
        }

        private void UpdateBackpackSlot(int slotNumber, string? artifactName)
        {
            if (this.isRefreshing)
            {
                return;
            }

            var artifact = string.IsNullOrWhiteSpace(artifactName) ? null : Artifact.FindByName(artifactName);
            SpellScroll.TryParse(artifactName, out var spell);
            this.model = this.model.WithSlot(slotNumber - 1, artifact, spell);
            this.onChanged?.Invoke(this.model);
        }
    }
}
