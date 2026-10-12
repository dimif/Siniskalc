namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// What the savegame is: the map, the date and the players. All behaviour is in <see cref="Homm2OverviewViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class Homm2OverviewPage : Page
    {
        public Homm2OverviewPage(Homm2OverviewViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
