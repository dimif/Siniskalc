namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The mines of the map and their guard. All behaviour is in <see cref="MapViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class MapPage : Page
    {
        public MapPage(MapViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}
