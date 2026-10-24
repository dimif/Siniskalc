namespace Siniskalc
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILogger<App> Logger = LogFactory.For<App>();

        private IMessageService? messages;
        private AppServices? services;
        private bool isReporting;

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += this.OnUnhandledException;

            var services = this.StartServices();
            if (services is null)
            {
                this.Shutdown();
                return;
            }

            this.services = services;
            this.messages = services.Messages;
            this.MainWindow = new ShellWindow(services.RecentFilesMenu, services.UrlLauncher, services.About, services.Help, services.Messages, services.FileCommands);
            this.MainWindow.Show();

            this.MainWindow.DataContext = services.CreateShellViewModel();
        }

        /// <summary>
        /// Flushes the log file.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            LogFactory.Reset();
            this.services?.LoggerFactory.Dispose();
            base.OnExit(e);
        }

        /// <summary>
        /// Builds the components, or tells the user why it cannot: a settings file that cannot be read, or a log folder that cannot be used.
        /// A settings file can be reset (kept as <c>.bad</c> next to it), so a damaged file never locks the user out. Null when the editor must not start.
        /// </summary>
        private AppServices? StartServices()
        {
            while (true)
            {
                try
                {
                    return new AppServices();
                }
                catch (SettingsException ex)
                {
                    var answer = MessageBox.Show(
                        $"{ex.Message}\n\nReset the settings? The file is kept as {Path.GetFileName(ex.Path)}.bad and the editor starts with the defaults.",
                        "Settings error",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error);
                    if (answer != MessageBoxResult.Yes)
                    {
                        return null;
                    }

                    try
                    {
                        File.Move(ex.Path, ex.Path + ".bad", overwrite: true);
                    }
                    catch (Exception moveFailure) when (moveFailure is IOException or UnauthorizedAccessException)
                    {
                        MessageBox.Show($"The settings file {ex.Path} could not be moved aside: {moveFailure.Message}", "Settings error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
                catch (LoggingException ex)
                {
                    MessageBox.Show(
                        $"{ex.Message}\n\nChoose another folder with LogFolder in the settings file, or remove it to use the default.",
                        "Logging error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return null;
                }
            }
        }

        /// <summary>
        /// Keeps the editor and its open savegame alive when an edit throws something nobody caught, so the user can still save
        /// (under another name if in doubt) instead of losing every unsaved edit.
        /// </summary>
        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.LogCritical(e.Exception, "An unexpected exception reached the dispatcher");
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
                Logger.LogError("The unexpected error window itself failed; the plain message box is the last resort");
                MessageBox.Show(message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.isReporting = false;
            }
        }
    }
}
