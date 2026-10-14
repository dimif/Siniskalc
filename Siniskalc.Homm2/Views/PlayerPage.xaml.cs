namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// A player's resources, heroes and towns. All behaviour is in <see cref="PlayerViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class PlayerPage : Page
    {
        public PlayerPage(PlayerViewModel viewModel)
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
