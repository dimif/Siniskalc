namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Heroes.Models;
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
        public static MinesListViewModel<MineRowViewModel<MineModel, PlayerColor>> Create(Game game, PlayerColor? owner = null)
        {
            var rows = Enumerable.Range(0, game.History.Current.Mines().Count)
                .Select(i => new MineRowViewModel<MineModel, PlayerColor>(game.History.CreateMineSession(i), (mine, color) => mine with { Owner = color }, MineOwnerToolTip, hasLevels: false))
                .ToList();
            if (owner == null)
            {
                return new MinesListViewModel<MineRowViewModel<MineModel, PlayerColor>>(rows);
            }

            var ownerName = OwnerNames<PlayerColor>.Of(owner);
            return new MinesListViewModel<MineRowViewModel<MineModel, PlayerColor>>(rows, mine => mine.OwnerSort == ownerName);
        }
    }
}
