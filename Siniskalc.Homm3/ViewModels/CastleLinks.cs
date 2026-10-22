namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Makes the links to the castles of a savefile and to the heroes in them, with the words HoMM3 uses for a settlement.
    /// </summary>
    public static class CastleLinks
    {
        /// <summary>
        /// Returns the link of each castle <paramref name="owner"/> owns, ordered by name, with the hall and fortification that stand in it,
        /// whether it is the capital, its Grail structure and the heroes in it.
        /// </summary>
        /// <param name="document">The savefile.</param>
        /// <param name="owner">The colour whose castles are wanted.</param>
        /// <param name="openCastle">Opens the page of the castle at an index; the links do nothing without it.</param>
        /// <param name="openHero">Opens the page of the hero at an index among the heroes.</param>
        public static IReadOnlyList<CastleLinkViewModel> OwnedBy(SaveDocument document, PlayerColor owner, Action<int>? openCastle, Action<int>? openHero) =>
            OwnedLinks.Castles<CastleModel, PlayerColor, Creature, Faction>(
                document.Castles().GetModels(),
                document.Heroes(),
                owner,
                openCastle,
                openHero,
                castle => [HallOf(castle), FortOf(castle)],
                castle => castle.HasBuilt(Building.Capitol),
                castle => castle.Buildings.FirstOrDefault(b => b.IsGrail)?.Name ?? string.Empty);

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
