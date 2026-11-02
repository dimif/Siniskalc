namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// A hero's general information, skills, army, spells and artifacts. All behaviour is in <see cref="HeroViewModel"/>; the page only binds to it.
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
            if (viewModel.CreateByteEditTarget() is { } target)
            {
                this.hexEditor.Bind(target);
            }
        }
    }
}
