namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// A castle's general information, buildings, army and Mage Guild. All behaviour is in <see cref="CastleViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class CastlePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CastlePage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public CastlePage(CastleViewModel viewModel)
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
