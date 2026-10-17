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
        public static CastleLinkViewModel For(CastleModel castle, IReadOnlyList<HeroModel> heroes, ICommand open, Action<int> openHero) => new(
            castle.Name,
            castle.Faction?.ToString() ?? string.Empty,
            open,
            StatusOf(castle),
            heroes: VisitorOf(castle, heroes, openHero));

        private static string StatusOf(CastleModel castle)
        {
            var dwellings = Enumerable.Range(1, Constants.Castle.DwellingCount).Count(d => castle.DwellingLevel(d) > 0);
            return string.Join(" · ", new[]
            {
                castle.HasCastle ? "Castle" : "Town",
                castle.MageGuildLevel > 0 ? $"Mage Guild {castle.MageGuildLevel}" : string.Empty,
                OverviewText.Count(dwellings, "dwelling", "dwellings"),
            }.Where(t => t.Length > 0));
        }

        private static IReadOnlyList<CastleHeroRowViewModel> VisitorOf(CastleModel castle, IReadOnlyList<HeroModel> heroes, Action<int> openHero) =>
            castle.VisitingHero is { } id && id >= 0 && id < heroes.Count
                ? [new CastleHeroRowViewModel("Visiting", heroes[id].Name, new DelegateCommand(_ => openHero(id)))]
                : [];
    }
}
