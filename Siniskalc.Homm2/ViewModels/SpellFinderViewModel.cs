namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Windows;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for the spells of Heroes of Might and Magic II: the search text is matched against the name, the level ("level 3"), the type (adventure or combat) and the magic (mind or elemental). Each row has its own
    /// "learned" check box.
    /// </summary>
    public sealed class SpellFinderViewModel : SpellFinderViewModelBase<SpellRowViewModel>
    {
        /// <param name="rows">The spells, in the order they are listed.</param>
        public SpellFinderViewModel(IReadOnlyList<SpellRowViewModel> rows)
            : base(rows, null, FinderColumns)
        {
            this.StartFiltering();
        }

        /// <inheritdoc/>
        public override string SearchPlaceholder => "Search by name, level, type, magic...";

        /// <inheritdoc/>
        public override string SearchHint => "Type any of: a name, a level (level 3), adventure / combat or a kind of magic (for example mind)";

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            SpellColumn,
            LevelColumn,
            TypeColumn,
            new("Magic", nameof(SpellRowViewModel.Magic), new GridLength(90), FinderColumnKind.MutedText, "Mind spells do not work on giants and titans"),
            new("Cost", nameof(SpellRowViewModel.Cost), new GridLength(70), FinderColumnKind.MutedText, "Spell points it costs to cast"),
            LearnedColumn,
        ];
    }
}
