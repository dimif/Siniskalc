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
        /// Returns the heroes in <paramref name="castle"/>: the garrisoned one, then the one at the gate, if there are any.
        /// </summary>
        /// <param name="castle">The castle.</param>
        /// <param name="heroes">The heroes of the savefile, to find the name and the place of a hero by the id the castle holds.</param>
        /// <param name="openHero">Opens the page of the hero at an index among the heroes; the links do nothing without it.</param>
        public static IReadOnlyList<CastleHeroRowViewModel> HeroesOf(CastleModel castle, HeroChunkCollection heroes, Action<int>? openHero)
        {
            var rows = new List<CastleHeroRowViewModel>();
            Add(rows, nameof(CastleHeroRole.Garrisoned), castle.GarrisonedHeroId);
            Add(rows, nameof(CastleHeroRole.Visiting), castle.VisitingHeroId);
            return rows;

            void Add(List<CastleHeroRowViewModel> list, string role, int? heroId)
            {
                if (heroId == null)
                {
                    return;
                }

                // The castle holds the id of the hero, which is not its place among the heroes.
                var hero = heroes.FindById(heroId.Value);
                var index = hero?.Index ?? -1;
                list.Add(new CastleHeroRowViewModel(role, hero?.Name ?? $"Unknown hero {heroId}", new DelegateCommand(_ => openHero?.Invoke(index), _ => index >= 0 && openHero != null)));
            }
        }

        /// <summary>
        /// Returns the link to a castle, with the hall and fortification that stand in it.
        /// </summary>
        /// <param name="castle">The castle.</param>
        /// <param name="open">Opens the page of the castle.</param>
        /// <param name="heroes">The heroes in the castle; see <see cref="HeroesOf"/>.</param>
        public static CastleLinkViewModel For(CastleModel castle, ICommand open, IReadOnlyList<CastleHeroRowViewModel>? heroes = null)
        {
            var dwellings = castle.Buildings.Count(b => b.IsDwelling);
            var status = string.Join(" · ", new[]
            {
                HallOf(castle),
                FortOf(castle),
                castle.MageGuildLevel > 0 ? $"Mage Guild {castle.MageGuildLevel}" : string.Empty,
                $"{dwellings} {(dwellings == 1 ? "dwelling" : "dwellings")}",
            }.Where(t => t.Length > 0));
            return new CastleLinkViewModel(
                castle.Name,
                castle.Faction?.Name ?? string.Empty,
                open,
                status,
                castle.HasBuilt(Building.Capitol),
                castle.Buildings.FirstOrDefault(b => b.IsGrail)?.Name ?? string.Empty,
                heroes);
        }

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
