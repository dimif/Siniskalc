namespace Siniskalc.Views
{
    using System.Windows.Controls;
    using Siniskalc.ViewModels;

    /// <summary>
    /// A player's resources, heroes and towns, and the raw bytes. All behaviour is in <see cref="PlayerViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class PlayerPage : Page
    {
        public PlayerPage(PlayerViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
            this.hexEditor.Bind(viewModel.CreateByteEditTarget());
        }
    }
}
