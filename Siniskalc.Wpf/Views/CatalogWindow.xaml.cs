namespace Siniskalc.Wpf.Views
{
    using System.Windows;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Shows a finder of what a game has (artifacts, creatures) with the byte code of each, to look things up. It changes nothing.
    /// </summary>
    public partial class CatalogWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogWindow"/> class.
        /// </summary>
        /// <param name="windowTitle">The title of the window.</param>
        /// <param name="finder">The finder to show; its rows should have no action.</param>
        public CatalogWindow(string windowTitle, AbstractFinderViewModel finder)
        {
            this.InitializeComponent();
            this.Title = windowTitle;
            this.DataContext = finder;
        }
    }
}
