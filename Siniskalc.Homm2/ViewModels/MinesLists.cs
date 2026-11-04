namespace Siniskalc.Homm2.ViewModels
{
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Makes the list of the mines of a HoMM2 map, for the map page (all of them) and for the Mines tab of a player (its own).
    /// </summary>
    public static class MinesLists
    {
        private const string MineOwnerToolTip = "Who owns the mine. The flag on the map and the daily income change with it: the game works the income out at the start of the next day. Not tried in the game.";

        /// <summary>
        /// Creates the list of the mines of <paramref name="game"/>, or of the mines of <paramref name="owner"/> when it is given.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="guardianText">Words the read-only guardian is shown with.</param>
        /// <param name="owner">The owner of the mines to list, or <see langword="null"/> for every mine.</param>
        /// <returns>The list.</returns>
        public static MinesListViewModel<MineRowViewModel> Create(Game game, MineGuardianTextProvider guardianText, PlayerColor? owner = null)
        {
            var count = game.History.Current.Mines().Count;
            MineRowViewModel Row(int index) => new(game.History.CreateMineSession(index), game.Catalog, MineOwnerToolTip, guardianText);
            return owner == null
                ? new MinesListViewModel<MineRowViewModel>(Enumerable.Range(0, count).Select(Row).ToList())
                : OwnedMines.List(count, Row, owner);
        }
    }
}
