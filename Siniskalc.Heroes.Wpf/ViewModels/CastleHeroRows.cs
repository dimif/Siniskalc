namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Makes the rows of the heroes that are in a castle of any game: the one that garrisons it, then the one at the gate, if there are any.
    /// </summary>
    public static class CastleHeroRows
    {
        /// <summary>
        /// Returns the heroes in <paramref name="castle"/>.
        /// </summary>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <typeparam name="TCreature">The creature of the game.</typeparam>
        /// <typeparam name="TFaction">The faction of the game.</typeparam>
        /// <param name="castle">The castle.</param>
        /// <param name="heroes">The heroes of the savefile, to find the name and the place of a hero by the id the castle holds.</param>
        /// <param name="openHero">Opens the page of the hero at an index among the heroes; the links do nothing without it.</param>
        public static IReadOnlyList<CastleHeroRowViewModel> For<TColor, TCreature, TFaction>(
            ICastleModel<TColor, TCreature, TFaction> castle,
            IHeroSummaries<TColor> heroes,
            Action<int>? openHero)
            where TColor : class, IPlayerColor
            where TCreature : class, IGameEntity
            where TFaction : class, IFaction
        {
            var rows = new List<CastleHeroRowViewModel>();
            Add(CastleHeroRole.Garrisoned, castle.GarrisonedHeroId);
            Add(CastleHeroRole.Visiting, castle.VisitingHeroId);
            return rows;

            void Add(CastleHeroRole role, int? heroId)
            {
                if (heroId == null)
                {
                    return;
                }

                // The castle holds the id of the hero, which is not necessarily its place among the heroes.
                var hero = heroes.FindById(heroId.Value);
                var index = hero?.Index ?? -1;
                rows.Add(new CastleHeroRowViewModel(role.ToString(), hero?.Name ?? $"Unknown hero {heroId}", new DelegateCommand(_ => openHero?.Invoke(index), _ => index >= 0 && openHero != null)));
            }
        }
    }
}
