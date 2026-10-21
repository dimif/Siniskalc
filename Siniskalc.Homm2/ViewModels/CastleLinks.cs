namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Makes the links to the castles of a savegame and to the hero in them, with the words HoMM2 uses for a settlement.
    /// </summary>
    public static class CastleLinks
    {
        /// <summary>
        /// Returns the link of each castle <paramref name="owner"/> owns that the map uses, ordered by name.
        /// </summary>
        /// <param name="document">The savefile.</param>
        /// <param name="owner">The colour whose castles are wanted.</param>
        /// <param name="openCastle">Opens the page of the castle with a number; the links do nothing without it.</param>
        /// <param name="openHero">Opens the page of the hero at an index among the heroes.</param>
        public static IReadOnlyList<CastleLinkViewModel> OwnedBy(SaveDocument document, PlayerColor owner, Action<int>? openCastle, Action<int>? openHero) =>
            document.Castles().GetModels()
                .Where(c => c.IsUsed && c.Owner == owner)
                .OrderBy(c => c.Name)
                .Select(c => For(c, document.Heroes(), new DelegateCommand(_ => openCastle?.Invoke(c.Index), _ => openCastle != null), openHero))
                .ToList();

        /// <summary>
        /// Returns the link to a castle: its faction, whether it is a castle or a town, the level of its Mage Guild and how many dwellings
        /// stand in it, and the hero that visits it.
        /// </summary>
        /// <param name="castle">The castle.</param>
        /// <param name="heroes">The heroes of the savegame, which a castle names by number.</param>
        /// <param name="open">Opens the page of the castle.</param>
        /// <param name="openHero">Opens the page of the hero with a number.</param>
        public static CastleLinkViewModel For(CastleModel castle, IHeroSummaries<PlayerColor> heroes, ICommand open, Action<int>? openHero) =>
            CastleLinkViewModel.For(
                castle,
                open,
                [castle.HasCastle ? "Castle" : "Town"],
                heroes: CastleHeroRows.For(castle, heroes, openHero));

    }
}
