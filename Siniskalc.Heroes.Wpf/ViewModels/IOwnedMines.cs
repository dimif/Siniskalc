namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.ComponentModel;
    using Siniskalc.Core.Data;

    /// <summary>
    /// A page of any game that lists the mines one owner has on the map, as the owned-mines card (<c>OwnedMinesCard</c>) reads it: what tells
    /// of another mine being picked, the number of that mine, and its bytes. A page implements <see cref="IOwnedMineList{TRow}"/>, which says
    /// the first two through its list of mines.
    /// </summary>
    public interface IOwnedMines
    {
        /// <summary>
        /// Gets what raises a change when another mine is picked.
        /// </summary>
        INotifyPropertyChanged MineSelection { get; }

        /// <summary>
        /// Gets the number of the mine that is picked among the mines of the savegame, or <c>null</c> when the owner has none.
        /// </summary>
        int? SelectedMineIndex { get; }

        /// <summary>
        /// Creates the raw bytes of the mine that is picked for the hex editor, or <c>null</c> when there is none or the page has no way to show
        /// bytes. The caller owns the target and disposes it.
        /// </summary>
        /// <returns>The target, or <see langword="null"/> when no mine is picked.</returns>
        IByteEditTarget? CreateMineByteEditTarget();
    }
}
