namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Makes the links to the castles of a savegame and to the hero in them, with the words HoMM2 uses for a settlement.
    /// </summary>
    public static class CastleLinks
    {
        /// <summary>
        /// Returns the link to a castle: its faction, whether it is a castle or a town, the level of its Mage Guild and how many dwellings
        /// stand in it, and the hero that visits it.
        /// </summary>
        /// <param name="castle">The castle.</param>
        /// <param name="heroes">The heroes of the savegame, which a castle names by number.</param>
        /// <param name="open">Opens the page of the castle.</param>
        /// <param name="openHero">Opens the page of the hero with a number.</param>
        public static CastleLinkViewModel For(CastleModel castle, IReadOnlyList<HeroModel> heroes, ICommand open, Action<int> openHero) =>
            CastleLinkViewModel.For(
                castle,
                open,
                [castle.HasCastle ? "Castle" : "Town"],
                heroes: VisitorOf(castle, heroes, openHero));

        private static IReadOnlyList<CastleHeroRowViewModel> VisitorOf(CastleModel castle, IReadOnlyList<HeroModel> heroes, Action<int> openHero) =>
            castle.VisitingHeroId is { } id && id >= 0 && id < heroes.Count
                ? [new CastleHeroRowViewModel("Visiting", heroes[id].Name, new DelegateCommand(_ => openHero(id)))]
                : [];
    }
}
