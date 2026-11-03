namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// A view model that reads its state again from the current document.
    /// </summary>
    public interface IRefreshableViewModel
    {
        /// <summary>
        /// Reads the state again and notifies the view of every value that may have changed.
        /// </summary>
        void Refresh();
    }
}
