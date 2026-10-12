namespace Siniskalc.Views
{
    using System.Windows.Controls;
    using Siniskalc.ViewModels;

    /// <summary>
    /// Interaction logic for ArtifactsPage.xaml. The DataContext is the hero's <see cref="EquippedArtifactsViewModel"/>.
    /// </summary>
    public partial class ArtifactsPage : Page
    {
        public ArtifactsPage(EquippedArtifactsViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
