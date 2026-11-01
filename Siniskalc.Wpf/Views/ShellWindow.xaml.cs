namespace Siniskalc.Wpf.Views
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The window of the application: the menus, the toolbar and the panel of pending changes around the editor of the
    /// game that is open. It knows no game; the editor is shown through the data template of its type.
    /// </summary>
    public partial class ShellWindow : Window
    {
        private readonly IUrlLauncher urlLauncher;
        private readonly AboutInfo about;
        private readonly HelpLinks help;
        private readonly IMessageService messages;
        private readonly SavegameFileCommands files;
        private readonly Func<LoggingSettingsViewModel>? loggingSettings;

        public ShellWindow(RecentFilesMenuViewModel recentMenu, IUrlLauncher urlLauncher, AboutInfo about, HelpLinks help, IMessageService messages, SavegameFileCommands files, Func<LoggingSettingsViewModel>? loggingSettings = null)
        {
            this.loggingSettings = loggingSettings;
            this.messages = messages;
            this.files = files;
            this.urlLauncher = urlLauncher;
            this.about = about;
            this.help = help;
            this.InitializeComponent();
            this.recentMenuItem.DataContext = recentMenu;
            this.showLogsMenuItem.Visibility = help.LogFolder is null ? Visibility.Collapsed : Visibility.Visible;
            recentMenu.OpenRequested += path =>
            {
                this.WithViewModel(shell => this.files.OpenRecent(shell, path));
                this.RequeryCommands();
            };
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private ShellViewModel? ViewModel => this.DataContext as ShellViewModel;

        private bool IsGameLoaded => this.ViewModel?.IsGameLoaded ?? false;

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!e.Cancel && this.ViewModel is { } viewModel)
            {
                e.Cancel = !this.files.ConfirmClose(viewModel);
            }
        }

        private void ClosePendingChangesClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.IsPendingChangesOpen = false;
            }
        }

        private void AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void GameLoadedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsGameLoaded;
        }

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.WithViewModel(this.files.Open);
            this.RequeryCommands();
        }

        private void SaveCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.WithViewModel(this.files.Save);
            this.RequeryCommands();
        }

        private void SaveAsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.WithViewModel(this.files.SaveAs);
            this.RequeryCommands();
        }

        private void UndoCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ViewModel?.CanUndo ?? false;
        }

        private void UndoCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel?.Undo();
        }

        private void RedoCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ViewModel?.CanRedo ?? false;
        }

        private void RedoCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel?.Redo();
        }

        private void ReloadCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ViewModel?.CanReload ?? false;
        }

        private void ReloadCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.WithViewModel(this.files.Reload);
        }

        private void OpenFolderCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel?.File is { } file && Path.GetDirectoryName(Path.GetFullPath(file)) is { Length: > 0 } folder && !this.urlLauncher.TryOpenFolder(folder))
            {
                this.messages.ShowInformation("Open folder", $"Could not open the folder of the savegame. It is at:\n{folder}");
            }
        }

        private async void DumpToFileCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel is { } viewModel)
            {
                await this.files.ExportBytesAsync(viewModel);
                this.RequeryCommands();
            }
        }

        private void ViewSaveHexCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel is not { IsGameLoaded: true } viewModel)
            {
                return;
            }

            var dump = viewModel.CreateSaveHexDump;
            this.files.Run(() => new SaveHexWindow(dump) { Owner = this }.Show(), "View save hex", "show the savefile bytes");
        }

        private void LoadFromDumpCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.WithViewModel(this.files.LoadBytes);
            this.RequeryCommands();
        }

        private void HelpCmdExecuted(object sender, ExecutedRoutedEventArgs e) => this.OpenInBrowser(this.help.UserGuideUrl);

        private void OpenIssuesCmdExecuted(object sender, ExecutedRoutedEventArgs e) => this.OpenInBrowser(this.help.IssuesUrl);

        private void EditSettingsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel is { } shell && new SettingsWindow(new SettingsViewModel(shell, this.loggingSettings?.Invoke())) { Owner = this }.ShowDialog() == true)
            {
                this.messages.ShowInformation("Settings", this.loggingSettings is null ? "Settings saved." : "Settings saved. The logging choices apply the next time the editor starts.");
                this.RequeryCommands();
            }
        }

        private void ShowLogsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.help.LogFolder is { } folder && !this.urlLauncher.TryOpenFolder(folder))
            {
                this.messages.ShowInformation("Help", $"Could not open the folder of the logs. It is at:\n{folder}");
            }
        }

        private void AboutCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutWindow(new AboutViewModel(this.about, this.urlLauncher)) { Owner = this }.ShowDialog();
        }

        private T? WithViewModel<T>(Func<ShellViewModel, T> action)
        {
            return this.ViewModel is { } viewModel ? action(viewModel) : default;
        }

        private void RequeryCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OpenInBrowser(string url)
        {
            if (!this.urlLauncher.TryOpen(url))
            {
                this.messages.ShowInformation("Help", $"Could not open the browser. The page is at:\n{url}");
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
