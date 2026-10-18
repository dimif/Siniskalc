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

        public SortColumnViewModel(string name, ICommand sort)
        {
            this.Name = name;
            this.SortCommand = sort;
        }

        public string Name { get; }

        /// <summary>
        /// Gets the name with the arrow of the direction when the list is sorted by this column.
        /// </summary>
        public string Label => this.arrow.Length == 0 ? this.Name : $"{this.Name} {this.arrow}";

        public ICommand SortCommand { get; }

        internal void ShowDirection(string newArrow)
        {
            this.arrow = newArrow;
            this.RaisePropertyChanged(nameof(this.Label));
        }
    }
}
