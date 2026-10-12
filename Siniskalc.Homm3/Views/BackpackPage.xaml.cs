namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// Interaction logic for BackpackPage.xaml. The DataContext is the hero's <see cref="BackpackViewModel"/>.
    /// </summary>
    public partial class BackpackPage : Page
    {
        public BackpackPage(BackpackViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
