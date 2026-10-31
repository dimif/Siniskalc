namespace Siniskalc.Wpf.Views
{
    using System.Windows;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The one window of the settings of the user: saving, the advanced tools and the logging.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel viewModel;

        public SettingsWindow(SettingsViewModel viewModel)
        {
            this.InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = viewModel;
        }

        // The window stays open, showing why, while the choices cannot be kept.
        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.TrySave())
            {
                this.DialogResult = true;
            }
        }
    }
}
