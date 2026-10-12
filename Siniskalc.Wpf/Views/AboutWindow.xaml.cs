namespace Siniskalc.Wpf.Views
{
    using System.Windows;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Says what the application is, where it lives and under which license.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow(AboutViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
