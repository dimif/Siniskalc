namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// A hero: the header and the tabs. All behaviour is in <see cref="HeroViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class HeroPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeroPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public HeroPage(HeroViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
            this.hexEditor.Bind(viewModel.CreateByteEditTarget());
        }
    }
}
