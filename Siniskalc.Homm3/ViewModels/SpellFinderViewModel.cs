namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for spells: every spell of the game version. The search text is matched against the name, the school, the level
    /// ("level 3") and whether the spell is for the adventure map or for combat: "fire", "level 5" and "air combat" all work.
    /// It has no action button; each row has its own "learned" check box, and an "available" one that only shows what
    /// artifacts give the hero.
    /// </summary>
    public sealed class SpellFinderViewModel : SpellFinderViewModelBase<SpellRowViewModel>
    {
        /// <param name="rows">The spells, in the order they are listed.</param>
        public SpellFinderViewModel(IReadOnlyList<SpellRowViewModel> rows)
            : base(rows, nameof(SpellRowViewModel.School), FinderColumns)
        {
            this.StartFiltering();
        }

        /// <inheritdoc/>
        public override string SearchPlaceholder => "Search by name, school, level, type...";

        /// <inheritdoc/>
        public override string SearchHint => "Type any of: a name, a school (fire, air, water, earth), a level (level 3) or adventure / combat";

        /// <inheritdoc/>
        protected override string SummaryDetail => $", {this.Rows.Count(r => r.IsAvailable)} available from artifacts";

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            SpellColumn,
            LevelColumn,
            TypeColumn,
            LearnedColumn,
            new(
                "Available",
                nameof(SpellRowViewModel.IsAvailable),
                new GridLength(100),
                FinderColumnKind.ReadOnlyCheckBox,
                "Spells the hero can cast through artifacts. The game works these out, so they cannot be changed here."),
            new(
                "Source",
                nameof(SpellRowViewModel.Source),
                new GridLength(220),
                FinderColumnKind.MutedText,
                "What lets the hero cast the spell without having learned it"),
        ];
    }
}
