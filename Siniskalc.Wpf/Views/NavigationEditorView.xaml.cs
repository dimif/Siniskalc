namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What an editor with a navigation tree looks like: a search box and the tree on the left, and on the right the page of what is
    /// picked in it. The game gives it the pages, through <see cref="PageFactory"/>, and the editor behind it is an
    /// <see cref="INavigationEditor"/>.
    /// </summary>
    public partial class NavigationEditorView : UserControl
    {
        private readonly Dictionary<object, Page> pages = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationEditorView"/> class.
        /// </summary>
        public NavigationEditorView()
        {
            this.InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;
        }

        /// <summary>
        /// Gets or sets what makes the page of a page view model. Each page is kept once made, so the tab it was on is kept.
        /// </summary>
        public Func<object, Page>? PageFactory { get; set; }

        private INavigationEditor? ViewModel => this.DataContext as INavigationEditor;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INavigationEditor oldViewModel)
            {
                oldViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            }

            // The pages are of the editor that was shown.
            this.pages.Clear();
            if (e.NewValue is INavigationEditor newViewModel)
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
            if (e.PropertyName == "CurrentGame")
            {
                // The pages are of the game that was open.
                this.pages.Clear();
            }
            else if (e.PropertyName == nameof(INavigationEditor.CurrentPage))
            {
                this.ShowCurrentPage();
            }
        }

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
                    var factory = this.PageFactory ?? throw new InvalidOperationException("The view has no way to make the page of a page view model.");
                    page = factory(current);
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
