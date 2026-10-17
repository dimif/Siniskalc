namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The mines, shrines and sphinxes of the map. All behaviour is in <see cref="MapViewModel"/>; the page only binds to it.
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
