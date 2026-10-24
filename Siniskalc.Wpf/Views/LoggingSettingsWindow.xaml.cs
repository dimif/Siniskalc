namespace Siniskalc.Wpf.Views
{
    using System.Windows;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Lets the user choose the log level and the folder of the log files.
    /// </summary>
    public partial class LoggingSettingsWindow : Window
    {
        private readonly LoggingSettingsViewModel viewModel;

        public LoggingSettingsWindow(LoggingSettingsViewModel viewModel)
        {
            this.InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = viewModel;
        }

        // The window stays open, showing why, while the choice cannot be kept.
        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.TrySave())
            {
                this.DialogResult = true;
            }
        }
    }
}
