namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// Interaction logic for SpellsPage.xaml.
    /// All behaviour is in <see cref="SpellsViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class SpellsPage : Page
    {
        public SpellsPage(SpellsViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}
