namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// What the savegame contains. All behaviour is in <see cref="OverviewViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class OverviewPage : Page
    {
        public OverviewPage(OverviewViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}
