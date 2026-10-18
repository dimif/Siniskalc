namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One kind of place a hero remembers having visited (forts, gazebos...): how many, commands that make the hero forget all of them (so
    /// that it can visit them again) or remember all of them, and, when the row is expanded, a box for each place of the map with where it is, to forget or remember
    /// them one at a time. It knows no game: the game says how many places the hero has visited and what each place is.
    /// </summary>
    public sealed class VisitedPlaceRowViewModel : AbstractViewModelBase
    {
        private readonly Func<int> visited;
        private readonly int onMap;
        private bool isExpanded;

        public VisitedPlaceRowViewModel(string name, Func<int> visited, Action forget, Action remember, string toolTip, IReadOnlyList<ToggleRowViewModel> places)
        {
            this.Name = name;
            this.visited = visited;
            this.ToolTip = toolTip;
            this.Places = places;
            this.onMap = places.Count;
            this.ForgetCommand = new DelegateCommand(_ => forget(), _ => this.visited() != 0);
            this.RememberCommand = new DelegateCommand(_ => remember(), _ => this.Places.Any(p => !p.IsChecked));
        }

        public string Name { get; }

        public string ToolTip { get; }

        /// <summary>
        /// Gets the places of the map, each with a box that is ticked when the hero has visited it.
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> Places { get; }

        public bool HasPlaces => this.Places.Count > 0;

        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (this.isExpanded != value)
                {
                    this.isExpanded = value;
                    this.RaisePropertyChanged(nameof(this.IsExpanded));
                }
            }
        }

        /// <summary>
        /// Gets how many places of the kind the hero has visited, such as "2 of 5".
        /// </summary>
        public string Text => this.onMap > 0
            ? $"{this.visited()} of {this.onMap}"
            : this.visited() switch { 0 => "none", var n => n.ToString() };

        /// <summary>
        /// Gets the command that makes the hero forget every place of the kind ("None").
        /// </summary>
        public ICommand ForgetCommand { get; }

        /// <summary>
        /// Gets the command that makes the hero remember every place of the kind on the map ("All"); it does nothing when the map has none.
        /// </summary>
        public ICommand RememberCommand { get; }

        public void Refresh()
        {
            this.RaisePropertyChanged(nameof(this.Text));
            foreach (var place in this.Places)
            {
                place.Refresh();
            }

            (this.ForgetCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (this.RememberCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }
}
