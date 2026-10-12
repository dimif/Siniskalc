namespace Siniskalc.Views
{
    using System.Windows.Controls;
    using Siniskalc.ViewModels;

    /// <summary>
    /// A castle: its garrison and buildings. All behaviour is in <see cref="CastleViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class CastlePage : Page
    {
        public CastlePage(CastleViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
            this.hexEditor.Bind(viewModel.CreateByteEditTarget());
        }
    }
}
