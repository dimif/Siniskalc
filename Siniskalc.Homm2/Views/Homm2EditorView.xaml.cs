namespace Siniskalc.Homm2.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// What the shell shows for the HoMM2 editor: the navigation tree of the players, heroes and towns, and the page of what is picked in it.
    /// </summary>
    public partial class Homm2EditorView : UserControl
    {
        private readonly Dictionary<object, Page> pages = [];

        public Homm2EditorView()
        {
            this.InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;
        }

        private Homm2EditorViewModel? ViewModel => this.DataContext as Homm2EditorViewModel;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is Homm2EditorViewModel oldViewModel)
            {
                oldViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            }

            // The pages are of the editor that was shown.
            this.pages.Clear();
            if (e.NewValue is Homm2EditorViewModel newViewModel)
            {
                newViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
                this.ShowCurrentPage();
            }
            else
            {
                this.pageFrame.Content = null;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Homm2EditorViewModel.CurrentGame))
            {
                // The pages are of the game that was open.
                this.pages.Clear();
            }
            else if (e.PropertyName == nameof(Homm2EditorViewModel.CurrentPage))
            {
                this.ShowCurrentPage();
            }
        }

        /// <summary>
        /// Shows the page of what is picked in the navigation tree. Each keeps its page, so the tab it was on is kept.
        /// </summary>
        private void ShowCurrentPage()
        {
            var current = this.ViewModel?.CurrentPage;

            if (current == null)
            {
                this.pageFrame.Content = null;
            }
            else
            {
                if (!this.pages.TryGetValue(current, out var page))
                {
                    page = current switch
                    {
                        HeroViewModel hero => new HeroPage(hero),
                        PlayerViewModel player => new PlayerPage(player),
                        CastleViewModel castle => new CastlePage(castle),
                        OverviewViewModel overview => new OverviewPage(overview),
                        _ => throw new InvalidOperationException($"No page for {current.GetType().Name}."),
                    };
                    this.pages[current] = page;
                }

                if (!ReferenceEquals(this.pageFrame.Content, page))
                {
                    this.pageFrame.Navigate(page);

                    // Pages are not visited in order, so there is nothing to go back to.
                    while (this.pageFrame.CanGoBack)
                    {
                        this.pageFrame.RemoveBackEntry();
                    }
                }
            }

            CommandManager.InvalidateRequerySuggested();
        }
    }
}
