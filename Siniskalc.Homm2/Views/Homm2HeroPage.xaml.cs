namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// A hero's general information, skills, army, spells and artifacts. All behaviour is in <see cref="Homm2HeroViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class Homm2HeroPage : Page
    {
        public Homm2HeroPage(Homm2HeroViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
