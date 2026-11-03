namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    public class EquippedArtifactsViewModel : ArtifactInfoViewModelBase
    {
        /// <summary>
        /// The slots the page shows, with the property of this view model that the slot's combo box binds to.
        /// </summary>
        private static readonly IReadOnlyDictionary<ArtifactSlots, string> SlotProperties = new Dictionary<ArtifactSlots, string>
        {
            [ArtifactSlots.Helmet] = nameof(SelectedHelm),
            [ArtifactSlots.Necklace] = nameof(SelectedNeck),
            [ArtifactSlots.Torso] = nameof(SelectedArmor),
            [ArtifactSlots.Cape] = nameof(SelectedCloak),
            [ArtifactSlots.Feet] = nameof(SelectedBoots),
            [ArtifactSlots.Weapon] = nameof(SelectedWeapon),
            [ArtifactSlots.Shield] = nameof(SelectedShield),
            [ArtifactSlots.Ring1] = nameof(SelectedLeftRing),
            [ArtifactSlots.Ring2] = nameof(SelectedRightRing),
            [ArtifactSlots.Misc1] = nameof(SelectedItem1),
            [ArtifactSlots.Misc2] = nameof(SelectedItem2),
            [ArtifactSlots.Misc3] = nameof(SelectedItem3),
            [ArtifactSlots.Misc4] = nameof(SelectedItem4),
            [ArtifactSlots.Misc5] = nameof(SelectedItem5),
        };

        private static readonly IReadOnlyDictionary<ArtifactSlots, string> SlotLabels = new Dictionary<ArtifactSlots, string>
        {
            [ArtifactSlots.Helmet] = "Helm",
            [ArtifactSlots.Necklace] = "Neck",
            [ArtifactSlots.Torso] = "Armor",
            [ArtifactSlots.Cape] = "Cloak",
            [ArtifactSlots.Feet] = "Boots",
            [ArtifactSlots.Weapon] = "Weapon",
            [ArtifactSlots.Shield] = "Shield",
            [ArtifactSlots.Ring1] = "Left ring",
            [ArtifactSlots.Ring2] = "Right ring",
            [ArtifactSlots.Misc1] = "Item 1",
            [ArtifactSlots.Misc2] = "Item 2",
            [ArtifactSlots.Misc3] = "Item 3",
            [ArtifactSlots.Misc4] = "Item 4",
            [ArtifactSlots.Misc5] = "Item 5",
        };

        private static readonly ArtifactSlots[] RingSlots = [ArtifactSlots.Ring1, ArtifactSlots.Ring2];

        private static readonly ArtifactSlots[] MiscSlots = [ArtifactSlots.Misc1, ArtifactSlots.Misc2, ArtifactSlots.Misc3, ArtifactSlots.Misc4, ArtifactSlots.Misc5];

        private static readonly ArtifactSlots[] WornOrder =
        [
            ArtifactSlots.Helmet, ArtifactSlots.Necklace, ArtifactSlots.Torso, ArtifactSlots.Cape, ArtifactSlots.Feet,
            ArtifactSlots.Weapon, ArtifactSlots.Shield, ArtifactSlots.Ring1, ArtifactSlots.Ring2,
        ];

        private readonly Action<EquippedArtifacts>? onChanged;
        private readonly Dictionary<ArtifactSlots, ArtifactSlotViewModel> slots;
        private EquippedArtifacts model;

        /// <param name="effectLines">Words what artifacts do.</param>
        /// <param name="model">The equipped artifacts to show.</param>
        /// <param name="onChanged">Called with the new equipped artifacts whenever the user changes a slot.</param>
        /// <param name="gameVersion">The game version; the finder leaves out artifacts from later versions.</param>
        /// <param name="sendToBackpack">Puts an artifact in the backpack; the finder gets a "Backpack" button when this is given.</param>
        /// <param name="canSendToBackpack">Tells whether the backpack has room.</param>
        public EquippedArtifactsViewModel(
            EffectLineProvider effectLines,
            EquippedArtifacts model,
            EntityCatalog catalog,
            Action<EquippedArtifacts>? onChanged = null,
            Action<Artifact>? sendToBackpack = null,
            Func<bool>? canSendToBackpack = null)
            : base("Artifacts", effectLines, catalog)
        {
            this.model = model;
            this.onChanged = onChanged;
            this.slots = SlotLabels.ToDictionary(
                p => p.Key,
                p => new ArtifactSlotViewModel(
                    p.Value,
                    onChoose => new ArtifactFinderViewModel(catalog, this.EffectLines, string.Empty, "Choose", onChoose, null, includeScrolls: true, rowFilter: a => a.BlockedSlots.HasFlag(p.Key)),
                    name => this.UpdateSelection(p.Key, name)));
            this.ClearSlotCommand = new DelegateCommand(
                p => this.UpdateSelection((ArtifactSlots)p!, null),
                p => p is ArtifactSlots slot && !string.IsNullOrEmpty(this.GetSlotValue(slot)));
            this.Finder = new ArtifactFinderViewModel(catalog, this.EffectLines, "Find artifacts", "Equip", this.Equip, c => this.FindFreeSlot(c.Artifact) != null, includeScrolls: true);
            if (sendToBackpack != null)
            {
                this.Finder.AddAction("Backpack", sendToBackpack, a => !a.IsScroll && (canSendToBackpack?.Invoke() ?? true));
            }

            this.Refresh(model);
        }

        /// <summary>
        /// Gets the finder that puts an artifact in the slot it belongs in.
        /// </summary>
        public ArtifactFinderViewModel Finder { get; }

        /// <summary>
        /// Gets the slot with the helm, neck, armor, cloak, boots, weapon, shield and rings, in the order the page shows them.
        /// </summary>
        public IReadOnlyList<ArtifactSlotViewModel> WornSlots => this.SlotsOf(WornOrder);

        /// <summary>
        /// Gets the five slots for carried items.
        /// </summary>
        public IReadOnlyList<ArtifactSlotViewModel> CarriedSlots => this.SlotsOf(MiscSlots);

        /// <summary>
        /// Gets what all the artifacts that are worn do together.
        /// </summary>
        public ArtifactEffectInfo TotalEffects => this.model.GetTotalEffects();

        /// <summary>
        /// Gets the numeric effects of everything worn, one line each; effects that add up to zero are left out.
        /// </summary>
        public IReadOnlyList<EffectLine> TotalEffectLines => this.EffectLines.Describe(this.TotalEffects);

        /// <summary>
        /// Gets whether the worn artifacts do nothing at all, so the page can say so.
        /// </summary>
        public bool HasNoEffects => this.TotalEffectLines.Count == 0 && string.IsNullOrEmpty(this.TotalEffects.Other);

        /// <summary>
        /// Gets the command that empties a slot. Its parameter is the <see cref="ArtifactSlots"/> value of the slot.
        /// </summary>
        public ICommand ClearSlotCommand { get; }

        /// <summary>
        /// Gets or sets the name of the artifact worn on the head, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedHelm
        {
            get => this.GetSlotValue(ArtifactSlots.Helmet);
            set => this.UpdateSelection(ArtifactSlots.Helmet, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact worn on the neck, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedNeck
        {
            get => this.GetSlotValue(ArtifactSlots.Necklace);
            set => this.UpdateSelection(ArtifactSlots.Necklace, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact worn as armor, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedArmor
        {
            get => this.GetSlotValue(ArtifactSlots.Torso);
            set => this.UpdateSelection(ArtifactSlots.Torso, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact worn as a cloak, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedCloak
        {
            get => this.GetSlotValue(ArtifactSlots.Cape);
            set => this.UpdateSelection(ArtifactSlots.Cape, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact worn on the feet, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedBoots
        {
            get => this.GetSlotValue(ArtifactSlots.Feet);
            set => this.UpdateSelection(ArtifactSlots.Feet, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact worn in the hand as a weapon, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedWeapon
        {
            get => this.GetSlotValue(ArtifactSlots.Weapon);
            set => this.UpdateSelection(ArtifactSlots.Weapon, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact worn as a shield, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedShield
        {
            get => this.GetSlotValue(ArtifactSlots.Shield);
            set => this.UpdateSelection(ArtifactSlots.Shield, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact worn on the left ring, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedLeftRing
        {
            get => this.GetSlotValue(ArtifactSlots.Ring1);
            set => this.UpdateSelection(ArtifactSlots.Ring1, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact worn on the right ring, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedRightRing
        {
            get => this.GetSlotValue(ArtifactSlots.Ring2);
            set => this.UpdateSelection(ArtifactSlots.Ring2, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact in the miscellaneous slot 1, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedItem1
        {
            get => this.GetSlotValue(ArtifactSlots.Misc1);
            set => this.UpdateSelection(ArtifactSlots.Misc1, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact in the miscellaneous slot 2, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedItem2
        {
            get => this.GetSlotValue(ArtifactSlots.Misc2);
            set => this.UpdateSelection(ArtifactSlots.Misc2, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact in the miscellaneous slot 3, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedItem3
        {
            get => this.GetSlotValue(ArtifactSlots.Misc3);
            set => this.UpdateSelection(ArtifactSlots.Misc3, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact in the miscellaneous slot 4, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedItem4
        {
            get => this.GetSlotValue(ArtifactSlots.Misc4);
            set => this.UpdateSelection(ArtifactSlots.Misc4, value);
        }

        /// <summary>
        /// Gets or sets the name of the artifact in the miscellaneous slot 5, or <see langword="null"/> for none.
        /// </summary>
        public string? SelectedItem5
        {
            get => this.GetSlotValue(ArtifactSlots.Misc5);
            set => this.UpdateSelection(ArtifactSlots.Misc5, value);
        }

        /// <summary>
        /// Gets the slot view model for <paramref name="slot"/>: what is in it, and the popup that picks something else.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <returns>The view model of the slot.</returns>
        public ArtifactSlotViewModel SlotFor(ArtifactSlots slot) => this.slots[slot];

        /// <summary>
        /// Wears <paramref name="artifact"/> in its own slot, replacing what is worn there. A ring or item goes in the first empty
        /// slot of its kind.
        /// </summary>
        /// <param name="artifact">The artifact.</param>
        /// <param name="choice">The choice from the finder.</param>
        public void Equip(Artifact artifact)
        {
            var slot = this.FindFreeSlot(artifact);
            if (slot != null)
            {
                this.UpdateSelection(slot.Value, artifact.Name);
            }
        }

        /// <summary>
        /// Wears the artifact of a finder row, with the spell when the row is a Spell Scroll.
        /// </summary>
        /// <param name="artifact">The artifact.</param>
        /// <param name="choice">The choice from the finder.</param>
        public void Equip(ArtifactChoiceViewModel choice)
        {
            var slot = this.FindFreeSlot(choice.Artifact);
            if (slot != null)
            {
                this.UpdateSelection(slot.Value, choice.Name);
            }
        }

        /// <summary>
        /// Shows <paramref name="model"/>, e.g. after an undo. A combination artifact is shown in every slot it blocks.
        /// </summary>
        /// <param name="model">The artifacts.</param>
        public void Refresh(EquippedArtifacts model)
        {
            this.model = model;

            foreach (var slot in SlotProperties.Keys)
            {
                this.SetSlotValue(slot, null);
            }

            foreach (var (slot, artifact) in model.GetEquippedArtifacts())
            {
                if (artifact.IsCombination)
                {
                    this.ApplySlotGroup(artifact.BlockedSlots, artifact.Name);
                }

                var scrollSpell = model.GetScrollSpell(slot);
                this.SetSlotValue(slot, scrollSpell != null ? SpellScroll.NameOf(scrollSpell) : artifact.Name);
            }

            this.RaisePropertyChanged(nameof(this.TotalEffects));
            this.RaisePropertyChanged(nameof(this.TotalEffectLines));
            this.RaisePropertyChanged(nameof(this.HasNoEffects));
            CommandManager.InvalidateRequerySuggested();
        }

        private ArtifactSlotViewModel[] SlotsOf(ArtifactSlots[] order) => order.Select(s => this.slots[s]).ToArray();

        /// <summary>
        /// Finds where <paramref name="artifact"/> would go: its slot, or for rings and items the first empty one. Null when the
        /// rings or items are all taken, or the artifact cannot be worn.
        /// </summary>
        private ArtifactSlots? FindFreeSlot(Artifact artifact)
        {
            if (EquippedArtifacts.IsSingleSlot(artifact.Slot))
            {
                return artifact.Slot;
            }

            ArtifactSlots[] group = artifact.Slot switch
            {
                ArtifactSlots.Rings => RingSlots,
                ArtifactSlots.Misc => MiscSlots,
                _ => [],
            };

            foreach (var slot in group)
            {
                if (string.IsNullOrEmpty(this.GetSlotValue(slot)))
                {
                    return slot;
                }
            }

            return null;
        }

        private void UpdateSelection(ArtifactSlots slot, string? name)
        {
            if (this.GetSlotValue(slot) == name)
            {
                return;
            }

            // The slot may show a combination artifact that is stored in another slot.
            var currentArtifact = this.GetArtifactForSlot(slot);
            var updated = currentArtifact is { IsCombination: true }
                ? this.model.WithUnequipped(this.FindSlotOf(currentArtifact) ?? slot)
                : this.model.WithUnequipped(slot);

            var artifact = string.IsNullOrWhiteSpace(name) ? null : this.Catalog.FindArtifact(name);
            if (artifact != null)
            {
                // Combination artifacts live in their own slot, unless that is a group (e.g. Misc), then in the chosen one.
                var targetSlot = artifact.IsCombination && EquippedArtifacts.IsSingleSlot(artifact.Slot) ? artifact.Slot : slot;
                SpellScroll.TryParse(this.Catalog, name, out var spell);
                updated = updated.WithEquipped(targetSlot, artifact, spell);
            }

            this.ShowArtifactInfo(artifact);
            this.Refresh(updated);
            this.onChanged?.Invoke(updated);
        }

        private ArtifactSlots? FindSlotOf(Artifact artifact)
        {
            foreach (var (slot, equipped) in this.model.GetEquippedArtifacts())
            {
                if (equipped == artifact)
                {
                    return slot;
                }
            }

            return null;
        }

        private void ApplySlotGroup(ArtifactSlots slots, string? value)
        {
            foreach (var slot in SlotProperties.Keys)
            {
                if (slots.HasFlag(slot))
                {
                    this.SetSlotValue(slot, value);
                }
            }
        }

        private Artifact? GetArtifactForSlot(ArtifactSlots slot)
        {
            var name = this.GetSlotValue(slot);
            return string.IsNullOrWhiteSpace(name) ? null : this.Catalog.FindArtifact(name);
        }

        private string? GetSlotValue(ArtifactSlots slot) => this.slots[slot].ArtifactName;

        private void SetSlotValue(ArtifactSlots slot, string? value)
        {
            this.slots[slot].UpdateFromModel(value);
            this.RaisePropertyChanged(SlotProperties[slot]);
        }
    }
}
