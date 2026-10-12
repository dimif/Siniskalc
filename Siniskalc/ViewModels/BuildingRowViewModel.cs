namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A building of a castle, as a line of the page: whether it is built, and what it needs.
    /// </summary>
    public sealed class BuildingRowViewModel
    {
        private readonly Action<bool>? setBuilt;

        /// <param name="setBuilt">Called with whether the building should be built when the user ticks or unticks it; null for a row that cannot be edited.</param>
        /// <param name="blockedReason">Why the building cannot be built or removed now, or null when it can.</param>
        public BuildingRowViewModel(
            Building building,
            bool isBuilt,
            IReadOnlySet<Building> built,
            bool isEnabled = true,
            int? creaturesAvailable = null,
            bool isReplaced = false,
            Action<bool>? setBuilt = null,
            string? blockedReason = null,
            bool isChild = false,
            Action<bool>? setAllowed = null,
            string? allowBlockedReason = null)
        {
            this.Creatures = DwellingCreatures.Of(building)?.Name ?? string.Empty;
            this.CanSetAllowed = setAllowed != null && allowBlockedReason == null;
            this.AllowText = isEnabled ? "Forbid" : "Allow";
            this.AllowToolTip = isEnabled ? "Forbid building it in this town" : "Allow building it in this town";
            this.ToggleAllowedCommand = new DelegateCommand(_ => setAllowed?.Invoke(!isEnabled));
            this.setBuilt = setBuilt;
            this.IsChild = isChild;
            this.CanToggle = setBuilt != null && blockedReason == null;
            var unmet = building.Requires.Where(r => !built.Contains(r)).Select(r => r.Name).ToList();
            var unmetNote = unmet.Count == 0 ? string.Empty : " (not built yet: " + string.Join(", ", unmet) + ")";
            this.ToggleToolTip = blockedReason ?? ((isBuilt ? "Remove this building" : "Build this building") + unmetNote);
            this.Name = building.Name;
            this.IsBuilt = isBuilt;
            this.Requirements = building.Prerequisites.Count == 0 ? string.Empty : "Requires " + string.Join(", ", building.Prerequisites.Select(p => p.Name));

            var missing = building.Prerequisites.Where(p => !built.Contains(p)).Select(p => p.Name).ToList();
            this.Warning = isBuilt && missing.Count > 0 ? "Built without " + string.Join(", ", missing) : string.Empty;
            this.IsDisabled = !isBuilt && !isEnabled;
            this.IsReplaced = isBuilt && isReplaced;
            this.Note = this.IsDisabled ? "Not allowed by the map" : string.Empty;
            this.Available = creaturesAvailable == null ? string.Empty : $"{creaturesAvailable:N0} available";
        }

        /// <summary>
        /// Gets whether the building can be built or removed now.
        /// </summary>
        public bool CanToggle { get; }

        /// <summary>
        /// Gets whether the row is shown under the building before it, such as an upgraded dwelling under its dwelling.
        /// </summary>
        public bool IsChild { get; }

        /// <summary>
        /// Gets whether the map's permission to build it can be changed: only for a building that is not built.
        /// </summary>
        public bool CanSetAllowed { get; }

        /// <summary>
        /// Gets "Allow" for a building the map forbids, "Forbid" for one it allows.
        /// </summary>
        public string AllowText { get; }

        public string AllowToolTip { get; }

        public ICommand ToggleAllowedCommand { get; }

        /// <summary>
        /// Gets what the tick box does, or why it cannot be used.
        /// </summary>
        public string ToggleToolTip { get; }

        /// <summary>
        /// Gets or sets whether the building is built. Setting it builds or removes the building through the castle's edit session.
        /// </summary>
        public bool IsChecked
        {
            get => this.IsBuilt;
            set => this.setBuilt?.Invoke(value);
        }

        /// <summary>
        /// Gets a note such as "Not allowed by the map", or nothing.
        /// </summary>
        public string Note { get; }

        /// <summary>
        /// Gets whether the building is not built and the map does not allow building it.
        /// </summary>
        public bool IsDisabled { get; }

        /// <summary>
        /// Gets whether the building is built but an upgrade has taken its place.
        /// </summary>
        public bool IsReplaced { get; }

        /// <summary>
        /// Gets how many creatures wait in a dwelling that is built, such as "16 available", or nothing for other buildings.
        /// </summary>
        public string Available { get; }

        /// <summary>
        /// Gets the creature a dwelling serves, such as "Behemoth", or nothing for other buildings.
        /// </summary>
        public string Creatures { get; }

        public string Name { get; }

        public bool IsBuilt { get; }

        /// <summary>
        /// Gets what has to be built first, such as "Requires Tavern", or nothing when the building needs nothing.
        /// </summary>
        public string Requirements { get; }

        /// <summary>
        /// Gets a note for a building that is built although what it needs is not, which the map editor allows.
        /// </summary>
        public string Warning { get; }
    }
}
