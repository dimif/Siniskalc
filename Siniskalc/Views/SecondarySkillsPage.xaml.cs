namespace Siniskalc.Views
{
    using System.Windows.Controls;
    using Siniskalc.ViewModels;

    /// <summary>
    /// Interaction logic for SecondarySkillsPage.xaml.
    /// All behaviour is in <see cref="SecondarySkillsViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class SecondarySkillsPage : Page
    {
        public SecondarySkillsPage(SecondarySkillsViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}
