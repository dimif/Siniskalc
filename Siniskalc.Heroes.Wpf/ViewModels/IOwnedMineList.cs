namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.ComponentModel;

    /// <summary>
    /// A page of any game that lists the mines one owner has on the map, with the rows of its game. What the owned-mines card reads of it
    /// (<see cref="IOwnedMines"/>) comes from <see cref="Mines"/>, so a page only gives the list and the bytes of a mine.
    /// </summary>
    /// <typeparam name="TRow">The row of a mine of the game.</typeparam>
    public interface IOwnedMineList<TRow> : IOwnedMines
        where TRow : MineRowViewModelBase
    {
        /// <summary>
        /// Gets the mines of the owner.
        /// </summary>
        MinesListViewModel<TRow> Mines { get; }

        /// <inheritdoc/>
        INotifyPropertyChanged IOwnedMines.MineSelection => this.Mines;

        /// <inheritdoc/>
        int? IOwnedMines.SelectedMineIndex => this.Mines.SelectedMine?.Index;
    }
}
