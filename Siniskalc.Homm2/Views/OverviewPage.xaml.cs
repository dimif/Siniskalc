namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// What the savegame is: the map, the date and the players. All behaviour is in <see cref="OverviewViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class OverviewPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverviewPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public OverviewPage(OverviewViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
