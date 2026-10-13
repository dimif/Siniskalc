namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// A castle's general information, buildings, army and Mage Guild. All behaviour is in <see cref="Homm2CastleViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class Homm2CastlePage : Page
    {
        public Homm2CastlePage(Homm2CastleViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
            if (viewModel.CreateByteEditTarget() is { } target)
            {
                this.hexEditor.Bind(target);
            }
        }
    }
}
