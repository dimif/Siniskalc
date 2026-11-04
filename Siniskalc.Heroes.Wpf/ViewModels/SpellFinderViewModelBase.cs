namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the spell finders of every game share: several rows can be selected and ticked together, the search text is matched against
    /// the name, the group (the school or the kind of magic), the level ("level 3") and the type, and the summary counts what is learned.
    /// It knows no game; the game adds its columns, its search and hint texts and what else the summary says.
    /// </summary>
    /// <typeparam name="TRow">The row of the game.</typeparam>
    public abstract class SpellFinderViewModelBase<TRow> : FinderViewModel<TRow>
        where TRow : SpellChoiceViewModel
    {
        /// <param name="rows">The spells, in the order they are listed.</param>
        /// <param name="groupPath">The property of a row the rows are grouped by, or <see langword="null"/> for one list.</param>
        /// <param name="columns">The columns of the list.</param>
        protected SpellFinderViewModelBase(IReadOnlyList<TRow> rows, string? groupPath, IReadOnlyList<FinderColumn> columns)
            : base(rows, groupPath, string.Empty, "spells", columns)
        {
        }

        /// <inheritdoc/>
        public sealed override bool AllowsMultiSelect => true;

        /// <inheritdoc/>
        public sealed override string Footer => "Shift-click or Ctrl-click to select several spells, then tick or untick one of them to change them all.";

        /// <inheritdoc/>
        public sealed override string Summary
        {
            get
            {
                var learned = this.Rows.Count(r => r.IsLearned);
                var shown = this.Items.Count;
                var showing = shown == this.Rows.Count ? string.Empty : $", showing {shown}";
                return $"{learned} of {this.Rows.Count} learned{this.SummaryDetail}{showing}";
            }
        }

        /// <summary>
        /// Gets the column with the name of the spell.
        /// </summary>
        protected static FinderColumn SpellColumn { get; } = new("Spell", nameof(SpellChoiceViewModel.Name), new GridLength(1, GridUnitType.Star));

        /// <summary>
        /// Gets the column with the level of the spell.
        /// </summary>
        protected static FinderColumn LevelColumn { get; } = new("Level", nameof(SpellChoiceViewModel.Level), new GridLength(60));

        /// <summary>
        /// Gets the column with the type of the spell (adventure or combat).
        /// </summary>
        protected static FinderColumn TypeColumn { get; } = new("Type", nameof(SpellChoiceViewModel.Kind), new GridLength(100), FinderColumnKind.MutedText);

        /// <summary>
        /// Gets the column with the check box that says whether the hero has learned the spell.
        /// </summary>
        protected static FinderColumn LearnedColumn { get; } = new("Learned", nameof(SpellChoiceViewModel.IsLearned), new GridLength(90), FinderColumnKind.CheckBox);

        /// <summary>
        /// Gets what the summary says between the number of learned spells and the number shown, with its leading comma; nothing by default.
        /// </summary>
        protected virtual string SummaryDetail => string.Empty;

        /// <inheritdoc/>
        protected sealed override bool Matches(TRow row) => this.TextMatches(row.Name, row.Group, $"Level {row.Level}", row.Kind);
    }
}
