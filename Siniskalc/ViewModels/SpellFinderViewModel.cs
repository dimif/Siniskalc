namespace Siniskalc.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for spells: every spell of the game version. The search text is matched against the name, the school, the level
    /// ("level 3") and whether the spell is for the adventure map or for combat: "fire", "level 5" and "air combat" all work.
    /// It has no action button; each row has its own "learned" check box, and an "available" one that only shows what
    /// artifacts give the hero.
    /// </summary>
    public sealed class SpellFinderViewModel : FinderViewModel<SpellRowViewModel>
    {
        /// <param name="rows">The spells, in the order they are listed.</param>
        public SpellFinderViewModel(IReadOnlyList<SpellRowViewModel> rows)
            : base(rows, nameof(SpellRowViewModel.School), string.Empty, "spells", FinderColumns)
        {
            this.StartFiltering();
        }

        public override string SearchPlaceholder => "Search by name, school, level, type...";

        public override string SearchHint => "Type any of: a name, a school (fire, air, water, earth), a level (level 3) or adventure / combat";

        public override string Summary
        {
            get
            {
                var learned = this.Rows.Count(r => r.IsLearned);
                var available = this.Rows.Count(r => r.IsAvailable);
                var shown = this.Items.Count;
                var showing = shown == this.Rows.Count ? string.Empty : $", showing {shown}";
                return $"{learned} of {this.Rows.Count} learned, {available} available from artifacts{showing}";
            }
        }

        public override bool AllowsMultiSelect => true;

        public override string Footer => "Shift-click or Ctrl-click to select several spells, then tick or untick one of them to change them all.";

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            new("Spell", nameof(SpellRowViewModel.Name), new GridLength(1, GridUnitType.Star)),
            new("Level", nameof(SpellRowViewModel.Level), new GridLength(60)),
            new("Type", nameof(SpellRowViewModel.Kind), new GridLength(100), FinderColumnKind.MutedText),
            new("Learned", nameof(SpellRowViewModel.IsLearned), new GridLength(90), FinderColumnKind.CheckBox),
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

        protected override bool Matches(SpellRowViewModel row)
        {
            return this.TextMatches(row.Name, row.School, $"Level {row.Level}", row.Kind);
        }
    }
}
