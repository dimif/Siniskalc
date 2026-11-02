namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// Interaction logic for ArtifactsView.xaml. The DataContext is the hero's <see cref="EquippedArtifactsViewModel"/>.
    /// </summary>
    public partial class ArtifactsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactsView"/> class.
        /// </summary>
        public ArtifactsView()
        {
            this.InitializeComponent();
        }
    }
}
