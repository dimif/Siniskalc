namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// The shrines and sphinxes of the map, and the raw bytes of its records. All behaviour is in <see cref="MapViewModel"/>; the page only
    /// binds to it and shows the bytes of the record that is picked.
    /// </summary>
    public partial class MapPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public MapPage(MapViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
            _ = new SelectedRecordHexPresenter<MapViewModel>(
                this,
                this.recordHexPanel,
                this.hexEditor,
                page => page,
                page => page.SelectedRecord,
                page => page.CreateByteEditTarget());
        }
    }
}
