namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// What the map keeps that no player owns (its merchants). All behaviour is in <see cref="MapViewModel"/>; the page only binds to it.
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
        }
    }
}
