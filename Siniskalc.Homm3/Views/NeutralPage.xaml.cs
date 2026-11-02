namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// What nobody owns: its heroes, towns, mines and dwellings. All behaviour is in <see cref="NeutralViewModel"/>; the page only binds to it.
    /// </summary>
    public partial class NeutralPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeutralPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public NeutralPage(NeutralViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
