namespace Siniskalc.Wpf.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// The heading of a column of a sortable list: its name with an arrow when the list is sorted by it, and the command that sorts by it.
    /// It knows no game.
    /// </summary>
    public sealed class SortColumnViewModel : AbstractViewModelBase
    {
        private string arrow = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortColumnViewModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sort">The sort.</param>
        public SortColumnViewModel(string name, ICommand sort)
        {
            this.Name = name;
            this.SortCommand = sort;
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name with the arrow of the direction when the list is sorted by this column.
        /// </summary>
        public string Label => this.arrow.Length == 0 ? this.Name : $"{this.Name} {this.arrow}";

        /// <summary>
        /// Gets the command that sorts the list by this column.
        /// </summary>
        public ICommand SortCommand { get; }

        /// <summary>
        /// Shows which way the list is sorted by this column.
        /// </summary>
        /// <param name="newArrow">The arrow to show, or empty when the list is not sorted by this column.</param>
        internal void ShowDirection(string newArrow)
        {
            this.arrow = newArrow;
            this.RaisePropertyChanged(nameof(this.Label));
        }
    }
}
