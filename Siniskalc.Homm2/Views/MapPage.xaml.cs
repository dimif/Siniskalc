namespace Siniskalc.Homm2.Views
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The shrines and sphinxes of the map, and the raw bytes of its records. All behaviour is in <see cref="MapViewModel"/>; the page only
    /// binds to it and shows the bytes of the record that is picked.
    /// </summary>
    public partial class MapPage : Page
    {
        private readonly MapViewModel viewModel;
        private MapRecordViewModel? bound;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public MapPage(MapViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
            this.viewModel = viewModel;
            this.hexEditor.IsVisibleChanged += (_, _) => this.ShowBytes();
            viewModel.PropertyChanged += this.OnViewModelChanged;
        }

        private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(MapViewModel.SelectedRecord) or null or "")
            {
                this.ShowBytes();
            }
        }

        // Building the grid of bytes is slow, so it is only built while the tab is on screen (advanced mode, on the Bytes tab).
        private void ShowBytes()
        {
            if (!this.hexEditor.IsVisible)
            {
                this.bound = null;
                return;
            }

            var picked = this.viewModel.SelectedRecord;
            if (picked == this.bound)
            {
                return;
            }

            this.bound = picked;
            if (this.viewModel.CreateByteEditTarget() is { } target)
            {
                this.hexEditor.Bind(target);
            }
            else
            {
                this.hexEditor.Unbind();
            }
        }
    }
}
