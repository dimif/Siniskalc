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
        public ArtifactFinderViewModel(GameVersion gameVersion, string title, string actionLabel, Action<Artifact> onChoose, Func<bool>? canChoose = null)
            : base(CreateRows(gameVersion), null, title, "artifacts", FinderColumns)
        {
            this.SetAction(actionLabel, choice => onChoose(choice.Artifact), _ => canChoose?.Invoke() ?? true);
            this.StartFiltering();
        }

        /// <inheritdoc/>
        public override string SearchPlaceholder => "Search by name or effect...";

        /// <inheritdoc/>
        public override string SearchHint => "Type part of a name, or what it adds: attack, defense, spell power, knowledge";

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            new("Artifact", nameof(ArtifactChoiceViewModel.Name), new GridLength(2, GridUnitType.Star)),
            new("Adds to the skills", nameof(ArtifactChoiceViewModel.Bonuses), new GridLength(2, GridUnitType.Star), FinderColumnKind.MutedText),
        ];

        /// <inheritdoc/>
        protected override bool Matches(ArtifactChoiceViewModel row) => this.TextMatches(row.Name, row.Bonuses);

        private static List<ArtifactChoiceViewModel> CreateRows(GameVersion gameVersion) => Artifact.All
            .AvailableIn(gameVersion)
            .OrderBy(a => a.Name)
            .Select(a => new ArtifactChoiceViewModel(a))
            .ToList();
    }
}
