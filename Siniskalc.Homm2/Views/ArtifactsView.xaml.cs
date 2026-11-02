namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// The artifacts of a hero. All behaviour is in <see cref="ViewModels.ArtifactsViewModel"/>; the control only binds to it.
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
