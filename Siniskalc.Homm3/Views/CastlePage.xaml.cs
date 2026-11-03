namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// A castle: its garrison and buildings. All behaviour is in <see cref="CastleViewModel"/>; the page only binds to it.
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
            this.hexEditor.Bind(viewModel.CreateByteEditTarget());
        }
    }
}
