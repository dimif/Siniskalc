namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Siniskalc.Core.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for the artifacts of Heroes of Might and Magic II. The search text is matched against the name and what the artifact adds
    /// to the skills: "sword", "ultimate" and "knowledge" all work. Picking one runs the action the owner gave it: equip it, or put it in a slot.
    /// </summary>
    public sealed class ArtifactFinderViewModel : FinderViewModel<ArtifactChoiceViewModel>
    {
        /// <param name="gameVersion">The game version; artifacts from later versions are left out.</param>
        /// <param name="title">The heading of the finder.</param>
        /// <param name="actionLabel">The text of the button on every row, such as "Equip".</param>
        /// <param name="onChoose">Called with the artifact when the user picks one.</param>
        /// <param name="canChoose">Tells whether an artifact can be picked right now; the button is disabled when not. Always when omitted.</param>
        public ArtifactFinderViewModel(GameCatalog catalog, GameVersion gameVersion, string title, string actionLabel, Action<Artifact> onChoose, Func<bool>? canChoose = null)
            : this(catalog, gameVersion, title, showCodes: false)
        {
            this.SetAction(actionLabel, choice => onChoose(choice.Artifact), _ => canChoose?.Invoke() ?? true);
        }

        private ArtifactFinderViewModel(GameCatalog catalog, GameVersion gameVersion, string title, bool showCodes)
            : base(CreateRows(catalog, gameVersion), null, title, "artifacts", showCodes ? CatalogColumns : FinderColumns)
        {
            this.ShowsCodes = showCodes;
            this.StartFiltering();
        }

        /// <summary>
        /// Gets whether the finder shows the byte code of each artifact and finds artifacts by it.
        /// </summary>
        public bool ShowsCodes { get; }

        /// <inheritdoc/>
        public override string SearchPlaceholder => this.ShowsCodes ? "Search by name, code or effect..." : "Search by name or effect...";

        /// <inheritdoc/>
        public override string SearchHint => "Type part of a name, or what it adds: attack, defense, spell power, knowledge"
            + (this.ShowsCodes ? ", or a byte code (2A, 0x2A or 42)" : string.Empty);

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            new("Artifact", nameof(ArtifactChoiceViewModel.Name), new GridLength(2, GridUnitType.Star)),
            new("Adds to the skills", nameof(ArtifactChoiceViewModel.Bonuses), new GridLength(2, GridUnitType.Star), FinderColumnKind.MutedText),
        ];

        private static IReadOnlyList<FinderColumn> CatalogColumns { get; } =
        [
            new("Code", nameof(ArtifactChoiceViewModel.Code), new GridLength(56), FinderColumnKind.MutedText, "The byte that stands for the artifact in a savefile, in hex"),
            .. FinderColumns,
        ];

        /// <summary>
        /// Makes a finder that only looks artifacts up: it has no action, and shows the byte code of each, left of its name.
        /// </summary>
        /// <param name="catalog">The catalog of the game.</param>
        /// <param name="gameVersion">The game version; artifacts from later versions are left out.</param>
        /// <returns>The finder.</returns>
        public static ArtifactFinderViewModel ForCatalog(GameCatalog catalog, GameVersion gameVersion) =>
            new(catalog, gameVersion, string.Empty, showCodes: true);

        /// <inheritdoc/>
        protected override bool Matches(ArtifactChoiceViewModel row) =>
            this.ShowsCodes ? this.TextOrCodeMatches(row.CodeValue, row.Name, row.Bonuses) : this.TextMatches(row.Name, row.Bonuses);

        private static List<ArtifactChoiceViewModel> CreateRows(GameCatalog catalog, GameVersion gameVersion) => catalog.Artifacts
            .Where(a => a.IsAvailableIn(gameVersion))
            .OrderBy(a => a.Name)
            .Select(a => new ArtifactChoiceViewModel(a))
            .ToList();
    }
}
