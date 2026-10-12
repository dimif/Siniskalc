namespace Siniskalc
{
    using System;
    using System.Windows;
    using System.Windows.Threading;
    using Siniskalc.Wpf.Views;
    using Siniskalc.Views;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IMessageService? messages;
        private bool isReporting;

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += this.OnUnhandledException;

            var services = new AppServices();
            this.messages = services.Messages;
            this.MainWindow = new MainWindow(services.RecentFilesMenu, services.UrlLauncher, services.About, services.Messages, services.FileCommands);
            this.MainWindow.Show();

            this.MainWindow.DataContext = services.CreateMainViewModel();
        }

        /// <summary>
        /// Keeps the editor and its open savegame alive when an edit throws something nobody caught, so the user can still save
        /// (under another name if in doubt) instead of losing every unsaved edit.
        /// </summary>
        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO:LOG error: an unexpected exception reached the dispatcher; log the whole exception (type, message, stack trace) and the page the user was on.
            e.Handled = true;

            // The dialog is shown from inside the handler: an exception it raises itself (or raised again while it is open) must not
            // be what ends the application and loses the edits.
            if (this.isReporting)
            {
                return;
            }

            this.isReporting = true;
            var message = $"Something went wrong: {e.Exception.Message}\n\nThe last change may not have been applied. If the editor misbehaves, save under another name and restart it.";
            try
            {
                if (this.messages != null)
                {
                    this.messages.ShowUnexpectedError("Unexpected error", message, e.Exception.ToString());
                }
                else
                {
                    MessageBox.Show(message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception)
            {
                // TODO:LOG error: the unexpected error window itself failed ({exception type and message}); the plain message box is the last resort.
                MessageBox.Show(message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.isReporting = false;
            }
        }
    }
}
