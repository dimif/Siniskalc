namespace Siniskalc.Views
{
    using System.Windows.Controls;
    using Siniskalc.ViewModels;

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
