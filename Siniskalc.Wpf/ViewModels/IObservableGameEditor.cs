namespace Siniskalc.Wpf.ViewModels
{
    using System.ComponentModel;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// An <see cref="IGameEditor"/> that tells the window when something it shows has changed: what the shell binds to.
    /// </summary>
    public interface IObservableGameEditor : IGameEditor, INotifyPropertyChanged
    {
        /// <summary>
        /// Makes a read-only finder of every artifact of the open game, with the byte code of each, for the View menu.
        /// </summary>
        /// <returns>The finder; its rows have no action.</returns>
        /// <exception cref="System.InvalidOperationException">No game is loaded.</exception>
        AbstractFinderViewModel CreateArtifactCatalog();

        /// <summary>
        /// Makes a read-only finder of every creature of the open game, with the byte code of each, for the View menu.
        /// </summary>
        /// <returns>The finder; its rows have no action.</returns>
        /// <exception cref="System.InvalidOperationException">No game is loaded.</exception>
        AbstractFinderViewModel CreateCreatureCatalog();
    }
}
