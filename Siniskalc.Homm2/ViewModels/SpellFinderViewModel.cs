namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for the spells of Heroes of Might and Magic II: the search text is matched against the name. Each row has its own
    /// "learned" check box.
    /// </summary>
    public sealed class SpellFinderViewModel : FinderViewModel<SpellRowViewModel>
    {
        /// <param name="rows">The spells, in the order they are listed.</param>
        public SpellFinderViewModel(IReadOnlyList<SpellRowViewModel> rows)
            : base(rows, null, string.Empty, "spells", FinderColumns)
        {
            this.StartFiltering();
        }

        public override bool AllowsMultiSelect => true;

        public override string Footer => "Shift-click or Ctrl-click to select several spells, then tick or untick one of them to change them all.";

        public override string Summary
        {
            get
            {
                var learned = this.Rows.Count(r => r.IsLearned);
                var shown = this.Items.Count;
                var showing = shown == this.Rows.Count ? string.Empty : $", showing {shown}";
                return $"{learned} of {this.Rows.Count} learned{showing}";
            }
        }

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            new("Spell", nameof(SpellRowViewModel.Name), new GridLength(1, GridUnitType.Star)),
            new("Level", nameof(SpellRowViewModel.Level), new GridLength(60)),
            new("Type", nameof(SpellRowViewModel.Kind), new GridLength(100), FinderColumnKind.MutedText),
            new("Magic", nameof(SpellRowViewModel.Magic), new GridLength(90), FinderColumnKind.MutedText, "Mind spells do not work on giants and titans"),
            new("Cost", nameof(SpellRowViewModel.Cost), new GridLength(70), FinderColumnKind.MutedText, "Spell points it costs to cast"),
            new("Learned", nameof(SpellRowViewModel.IsLearned), new GridLength(90), FinderColumnKind.CheckBox),
        ];

        protected override bool Matches(SpellRowViewModel row) => this.TextMatches(row.Name, $"Level {row.Level}", row.Kind, row.Magic);
    }
}
