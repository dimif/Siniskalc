namespace Siniskalc.Views
{
    using System.Windows.Controls;
    using Siniskalc.ViewModels;

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
