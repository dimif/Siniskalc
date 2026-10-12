namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using Siniskalc.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for artifacts. The search text is matched against the name, where the artifact is worn, its type and what it does:
    /// "relic helm", "ring", "combined" or "knowledge +10" all work.
    /// It does not know what to do with the artifact that is picked: the owner passes that in, so the same finder
    /// can equip an artifact on the artifacts page, drop it in the backpack, or fill a slot of something else later.
    /// </summary>
    public sealed class ArtifactFinderViewModel : FinderViewModel<ArtifactChoiceViewModel>
    {
        /// <summary>
        /// The word that finds artifacts that take the place of several others, such as Angelic Alliance.
        /// </summary>
        public const string CombinedWord = "combined";

        private static readonly string[] SlotOrder = ["Helm", "Neck", "Armor", "Cloak", "Boots", "Weapon", "Shield", "Ring", "Item", "Other"];

        /// <param name="gameVersion">The game version; artifacts from later versions are left out.</param>
        /// <param name="effectLines">Words what the artifacts do.</param>
        /// <param name="title">The heading of the finder.</param>
        /// <param name="actionLabel">The text of the button on every row, such as "Equip".</param>
        /// <param name="onChoose">Called with the artifact when the user picks one.</param>
        /// <param name="canChoose">Tells whether an artifact can be picked right now; the button is disabled when not. Always when omitted.</param>
        public ArtifactFinderViewModel(GameVersion gameVersion, EffectLineProvider effectLines, string title, string actionLabel, Action<Artifact> onChoose, Func<Artifact, bool>? canChoose = null)
            : this(gameVersion, effectLines, title, actionLabel, OfArtifact(onChoose), OfArtifact(canChoose), false)
        {
        }

        /// <summary>
        /// Creates a finder whose action gets the whole row, so it can use the spell of a scroll.
        /// </summary>
        /// <param name="includeScrolls">Lists one Spell Scroll row per spell instead of a single spell-less one.</param>
        /// <param name="rowFilter">Leaves out the artifacts it returns false for, e.g. those that do not fit a slot. All are listed when omitted.</param>
        public ArtifactFinderViewModel(GameVersion gameVersion, EffectLineProvider effectLines, string title, string actionLabel, Action<ArtifactChoiceViewModel> onChoose, Func<ArtifactChoiceViewModel, bool>? canChoose, bool includeScrolls, Func<Artifact, bool>? rowFilter = null)
            : base(CreateRows(gameVersion, effectLines, includeScrolls, rowFilter), nameof(ArtifactChoiceViewModel.SlotName), title, "artifacts", FinderColumns)
        {
            this.SetAction(actionLabel, onChoose, canChoose);
            this.StartFiltering();
        }

        /// <summary>
        /// Gets the artifacts, grouped by where they are worn and narrowed by the search text.
        /// </summary>
        public ListCollectionView Artifacts => this.Items;

        public override string? RowToolTipPath => nameof(ArtifactChoiceViewModel.Effects);

        public override string SearchPlaceholder => "Search by name, slot, type, effect...";

        public override string SearchHint => "Type any of: a name, a slot (helm, ring, item...), a type (relic, major, minor, treasure, combined) or an effect";

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            new("Artifact", nameof(ArtifactChoiceViewModel.Name), new GridLength(2, GridUnitType.Star)),
            new("Slot", nameof(ArtifactChoiceViewModel.SlotName), new GridLength(64), FinderColumnKind.MutedText),
            new("Type", nameof(ArtifactChoiceViewModel.TypeName), new GridLength(64), FinderColumnKind.MutedText),
            new("Gold", nameof(ArtifactChoiceViewModel.Cost), new GridLength(64), FinderColumnKind.MutedText),
            new("Effects", nameof(ArtifactChoiceViewModel.Effects), new GridLength(5, GridUnitType.Star), FinderColumnKind.MutedText),
        ];

        /// <summary>
        /// Gives every row a second button, such as "Backpack", that does something else with the artifact.
        /// </summary>
        public void AddAction(string label, Action<Artifact> onChoose, Func<Artifact, bool>? canChoose = null)
        {
            this.SetSecondaryAction(label, OfArtifact(onChoose), OfArtifact(canChoose));
        }

        protected override bool Matches(ArtifactChoiceViewModel choice)
        {
            return this.TextMatches(choice.Name, choice.SlotName, choice.TypeName, choice.IsCombined ? CombinedWord : null, choice.Effects);
        }

        private static Action<ArtifactChoiceViewModel> OfArtifact(Action<Artifact> onChoose)
        {
            return choice => onChoose(choice.Artifact);
        }

        private static Func<ArtifactChoiceViewModel, bool>? OfArtifact(Func<Artifact, bool>? canChoose)
        {
            return canChoose == null ? null : choice => canChoose(choice.Artifact);
        }

        private static List<ArtifactChoiceViewModel> CreateRows(GameVersion gameVersion, EffectLineProvider effectLines, bool includeScrolls, Func<Artifact, bool>? rowFilter)
        {
            var rows = Artifact.All.Values
                .Where(a => a.IsAvailableIn(gameVersion) && a.Slot != ArtifactSlots.None && (rowFilter?.Invoke(a) ?? true))
                .GroupBy(a => a.Name)
                .Select(g => g.First())
                .Where(a => !includeScrolls || a != Artifact.SpellScroll)
                .Select(a => new ArtifactChoiceViewModel(a, effectLines))
                .ToList();

            if (includeScrolls && (rowFilter?.Invoke(Artifact.SpellScroll) ?? true))
            {
                rows.AddRange(SpellScroll.For(gameVersion).Select(s => new ArtifactChoiceViewModel(Artifact.SpellScroll, effectLines, s)));
            }

            return rows
                .OrderBy(c => Array.IndexOf(SlotOrder, c.SlotName))
                .ThenBy(c => c.Artifact.Class)
                .ThenBy(c => c.Name)
                .ToList();
        }
    }
}
