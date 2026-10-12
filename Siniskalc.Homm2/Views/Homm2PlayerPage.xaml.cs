namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// A player's resources, heroes and towns. All behaviour is in <see cref="Homm2PlayerViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class Homm2PlayerPage : Page
    {
        public Homm2PlayerPage(Homm2PlayerViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
