namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// A <see cref="AbstractFinderViewModel"/> whose rows are <typeparamref name="TRow"/>.
    /// </summary>
    public abstract class FinderViewModel<TRow> : AbstractFinderViewModel
        where TRow : class
    {
        private Action<TRow>? onChoose;
        private Func<TRow, bool>? canChoose;
        private Action<TRow>? onChooseSecondary;
        private Func<TRow, bool>? canChooseSecondary;

        protected FinderViewModel(IReadOnlyList<TRow> rows, string? groupPath, string title, string noun, IReadOnlyList<FinderColumn> columns)
            : base(title, noun, columns, CreateItems(rows, groupPath))
        {
            this.Rows = rows;
        }

        /// <summary>
        /// Gets every row, narrowed or not.
        /// </summary>
        protected IReadOnlyList<TRow> Rows { get; }

        protected override int TotalCount => this.Rows.Count;

        /// <summary>
        /// Runs the action on <paramref name="row"/>, when it can be run.
        /// </summary>
        public void Choose(TRow? row)
        {
            if (row != null && this.onChoose != null && (this.canChoose == null || this.canChoose(row)))
            {
                this.onChoose(row);
            }
        }

        public override void ChooseRow(object? row) => this.Choose(row as TRow);

        public override void ChooseRowSecondary(object? row)
        {
            if (row is TRow typed && this.onChooseSecondary != null && (this.canChooseSecondary == null || this.canChooseSecondary(typed)))
            {
                this.onChooseSecondary(typed);
            }
        }

        protected override bool CanChooseRowSecondary(object row) => row is TRow typed && (this.canChooseSecondary == null || this.canChooseSecondary(typed));

        protected override bool CanChooseRow(object row) => row is TRow typed && (this.canChoose == null || this.canChoose(typed));

        protected override bool MatchesRow(object row) => row is TRow typed && this.Matches(typed);

        /// <summary>
        /// Tells whether a row shows under the search text.
        /// </summary>
        protected abstract bool Matches(TRow row);

        /// <summary>
        /// Gives every row an action button.
        /// </summary>
        /// <param name="label">The text of the button, such as "Equip".</param>
        /// <param name="onChoose">Runs when the user picks a row.</param>
        /// <param name="canChoose">Tells whether a row can be picked right now; the button is disabled when not. Always when omitted.</param>
        protected void SetAction(string label, Action<TRow> onChoose, Func<TRow, bool>? canChoose = null)
        {
            this.onChoose = onChoose;
            this.canChoose = canChoose;
            this.EnableAction(label);
        }

        /// <summary>
        /// Gives every row a second action button, after the first.
        /// </summary>
        /// <param name="label">The text of the button, such as "Backpack".</param>
        /// <param name="onChoose">Runs when the user picks a row with it.</param>
        /// <param name="canChoose">Tells whether a row can be picked with it right now; the button is disabled when not. Always when omitted.</param>
        protected void SetSecondaryAction(string label, Action<TRow> onChoose, Func<TRow, bool>? canChoose = null)
        {
            this.onChooseSecondary = onChoose;
            this.canChooseSecondary = canChoose;
            this.EnableSecondaryAction(label);
        }

        private static ListCollectionView CreateItems(IReadOnlyList<TRow> rows, string? groupPath)
        {
            var items = new ListCollectionView(rows.ToList());
            if (groupPath != null)
            {
                items.GroupDescriptions.Add(new PropertyGroupDescription(groupPath));
            }

            return items;
        }
    }
}
