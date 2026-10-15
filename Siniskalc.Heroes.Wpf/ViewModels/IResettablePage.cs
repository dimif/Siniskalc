namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// A page whose edits can be put back: what the header of a page (<c>PageHeader</c>) shows an "edited" mark and Reset for.
    /// </summary>
    public interface IResettablePage
    {
        /// <summary>
        /// Gets whether the savegame holds edits of what the page is about that are not saved.
        /// </summary>
        bool HasPendingChanges { get; }

        /// <summary>
        /// Gets the command that puts everything of the page back to what the savegame had.
        /// </summary>
        ICommand ResetCommand { get; }
    }
}
