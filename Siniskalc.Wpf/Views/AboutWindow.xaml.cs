namespace Siniskalc.Wpf.Views
{
    using System.Windows;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Says what the application is, where it lives and under which license.
    /// </summary>
    public partial class AboutWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutWindow"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public AboutWindow(AboutViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
