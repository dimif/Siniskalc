namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// Interaction logic for ArmyPage.xaml.
    /// All behaviour is in <see cref="ArmyViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class ArmyPage : Page
    {
        public ArmyPage(ArmyViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}
