namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Makes the links to the castles of a savefile and to the heroes in them, with the words HoMM3 uses for a settlement.
    /// </summary>
    public static class CastleLinks
    {
        /// <summary>
        /// Returns the link to a castle, with the hall and fortification that stand in it.
        /// </summary>
        /// <param name="castle">The castle.</param>
        /// <param name="open">Opens the page of the castle.</param>
        /// <param name="heroes">The heroes in the castle; see <see cref="CastleHeroRows"/>.</param>
        public static CastleLinkViewModel For(CastleModel castle, ICommand open, IReadOnlyList<CastleHeroRowViewModel>? heroes = null) =>
            CastleLinkViewModel.For(
                castle,
                open,
                [HallOf(castle), FortOf(castle)],
                castle.HasBuilt(Building.Capitol),
                castle.Buildings.FirstOrDefault(b => b.IsGrail)?.Name ?? string.Empty,
                heroes);

        private static string HallOf(CastleModel castle)
        {
            return castle.HasBuilt(Building.Capitol) ? "Capitol"
                : castle.HasBuilt(Building.CityHall) ? "City"
                : castle.HasBuilt(Building.TownHall) ? "Town"
                : "Village";
        }

        private static string FortOf(CastleModel castle)
        {
            return castle.HasBuilt(Building.Castle) ? "Castle"
                : castle.HasBuilt(Building.Citadel) ? "Citadel"
                : castle.HasBuilt(Building.Fort) ? "Fort"
                : string.Empty;
        }
    }
}
