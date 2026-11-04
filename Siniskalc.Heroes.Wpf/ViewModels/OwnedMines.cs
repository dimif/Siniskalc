namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Makes the list of the mines one owner has on the map, for the Mines tab of the owner's page in any game. A mine leaves the list when its
    /// owner is changed to another one, and joins the list of the new owner.
    /// </summary>
    public static class OwnedMines
    {
        /// <summary>
        /// Creates the list of the mines of <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TRow">The row of a mine of the game.</typeparam>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <param name="count">How many mines the savegame has.</param>
        /// <param name="createRow">Makes the row of the mine with the given number among the mines of the savegame.</param>
        /// <param name="owner">The owner whose mines the list shows (<c>Neutral</c> for the mines nobody owns).</param>
        /// <returns>The list.</returns>
        public static MinesListViewModel<TRow> List<TRow, TColor>(int count, Func<int, TRow> createRow, TColor owner)
            where TRow : MineRowViewModelBase
            where TColor : AbstractEnumeration, IPlayerColor
        {
            var ownerName = OwnerNames<TColor>.Of(owner);
            return new MinesListViewModel<TRow>(Enumerable.Range(0, count).Select(createRow).ToList(), mine => mine.OwnerSort == ownerName);
        }
    }
}
