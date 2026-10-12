namespace Siniskalc.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Siniskalc;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Runtime;
    using Siniskalc.ViewModels;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<object, Page> heroPages = new();
        private readonly IUrlLauncher urlLauncher;
        private readonly AboutInfo about;
        private readonly IMessageService messages;
        private readonly SavegameFileCommands files;

        public MainWindow(RecentFilesMenuViewModel recentMenu, IUrlLauncher urlLauncher, AboutInfo about, IMessageService messages, SavegameFileCommands files)
        {
            this.messages = messages;
            this.files = files;
            this.urlLauncher = urlLauncher;
            this.about = about;
            this.InitializeComponent();
            this.recentMenuItem.DataContext = recentMenu;
            recentMenu.OpenRequested += path => this.ShowStatus(this.WithViewModel(main => this.files.OpenRecent(main, path)));
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.DataContextChanged += this.OnDataContextChanged;
        }

        private MainViewModel? ViewModel => this.DataContext as MainViewModel;

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

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MainViewModel oldViewModel)
            {
                oldViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            }

            if (e.NewValue is MainViewModel newViewModel)
            {
                newViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
                this.ShowCurrentPage();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentGame))
            {
                // The pages are of the heroes of the game that was open.
                this.heroPages.Clear();
            }
            else if (e.PropertyName == nameof(MainViewModel.CurrentPage))
            {
                this.ShowCurrentPage();
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
            this.ShowStatus(this.WithViewModel(this.files.Open));
        }

        private void SaveCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShowStatus(this.WithViewModel(this.files.Save));
        }

        private void SaveAsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShowStatus(this.WithViewModel(this.files.SaveAs));
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

        private async void DumpToFileCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel is { } viewModel)
            {
                this.ShowStatus(await this.files.ExportBytesAsync(viewModel));
            }
        }

        private void ViewSaveHexCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel?.CurrentGame == null)
            {
                return;
            }

            var dump = this.ViewModel.CreateSaveHexDump;
            this.files.Run(() => new SaveHexWindow(dump) { Owner = this }.Show(), "View save hex", "show the savefile bytes");
        }

        private void LoadFromDumpCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShowStatus(this.WithViewModel(this.files.LoadBytes));
        }

        private void HelpCmdExecuted(object sender, ExecutedRoutedEventArgs e) => this.OpenInBrowser(Constants.App.UserGuideUrl);

        private void OpenIssuesCmdExecuted(object sender, ExecutedRoutedEventArgs e) => this.OpenInBrowser(Constants.App.IssuesUrl);

        private void AboutCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutWindow(new AboutViewModel(this.about, this.urlLauncher)) { Owner = this }.ShowDialog();
        }

        private T? WithViewModel<T>(Func<MainViewModel, T> action)
        {
            return this.ViewModel is { } viewModel ? action(viewModel) : default;
        }

        private void ShowStatus(string? text)
        {
            if (text != null)
            {
                this.status.Text = text;
            }

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

        /// <summary>
        /// Shows the page of what is picked in the navigation tree: the overview, a player or a hero. Each keeps its page,
        /// so the tab a hero was on is kept.
        /// </summary>
        private void ShowCurrentPage()
        {
            var current = this.ViewModel?.CurrentPage;

            if (current == null)
            {
                this.heroFrame.Content = null;
            }
            else
            {
                if (!this.heroPages.TryGetValue(current, out var page))
                {
                    page = current switch
                    {
                        HeroViewModel hero => new HeroPage(hero),
                        PlayerViewModel player => new PlayerPage(player),
                        CastleViewModel castle => new CastlePage(castle),
                        OverviewViewModel overview => new OverviewPage(overview),
                        _ => throw new InvalidOperationException($"No page for {current.GetType().Name}."),
                    };
                    this.heroPages[current] = page;
                }

                if (!ReferenceEquals(this.heroFrame.Content, page))
                {
                    this.heroFrame.Navigate(page);

                    // Pages are not visited in order, so there is nothing to go back to.
                    while (this.heroFrame.CanGoBack)
                    {
                        this.heroFrame.RemoveBackEntry();
                    }
                }
            }

            CommandManager.InvalidateRequerySuggested();
        }
    }
}
