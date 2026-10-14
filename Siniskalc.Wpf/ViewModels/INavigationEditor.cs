namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// What the view of an editor with a navigation tree and a page frame needs of the editor.
    /// </summary>
    public interface INavigationEditor : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the top level nodes of the navigation tree.
        /// </summary>
        ObservableCollection<NavNodeViewModel> NavigationNodes { get; }

        /// <summary>
        /// Gets or sets the text the navigation tree is filtered by.
        /// </summary>
        string NavigationFilter { get; set; }

        bool IsGameLoaded { get; }

        /// <summary>
        /// Gets what the editor shows, or nothing while no savegame is loaded.
        /// </summary>
        object? CurrentPage { get; }
    }
}
