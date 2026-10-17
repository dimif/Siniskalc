namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One kind of place a hero remembers having visited (forts, gazebos...): how many, a command that makes the hero forget all of them so
    /// that it can visit them again, and, when the row is expanded, a box for each place of the map with where it is, to forget or remember
    /// them one at a time.
    /// </summary>
    public sealed class VisitedPlaceRowViewModel : AbstractViewModelBase
    {
        private readonly Func<uint> get;
        private readonly int onMap;
        private bool isExpanded;

        public VisitedPlaceRowViewModel(string name, Func<uint> get, Action forget, string toolTip, IReadOnlyList<ToggleRowViewModel> places)
        {
            this.Name = name;
            this.get = get;
            this.ToolTip = toolTip;
            this.Places = places;
            this.onMap = places.Count;
            this.ForgetCommand = new DelegateCommand(_ => forget(), _ => this.get() != 0);
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
            ? $"{BitOperations.PopCount(this.get())} of {this.onMap}"
            : BitOperations.PopCount(this.get()) switch { 0 => "none", var n => n.ToString() };

        public ICommand ForgetCommand { get; }

        public void Refresh()
        {
            this.RaisePropertyChanged(nameof(this.Text));
            foreach (var place in this.Places)
            {
                place.Refresh();
            }

            (this.ForgetCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }
}
