namespace Siniskalc.Homm2.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// What nobody owns: its heroes, towns and mines. All behaviour is in <see cref="NeutralViewModel"/>; the page only binds to it.
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
