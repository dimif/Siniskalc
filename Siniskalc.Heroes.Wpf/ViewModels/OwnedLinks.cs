namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Makes the lists of what a colour owns that the pages of every game show: its heroes, by name.
    /// </summary>
    public static class OwnedLinks
    {
        /// <summary>
        /// Returns a link for each hero <paramref name="owner"/> owns, ordered by name.
        /// </summary>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <param name="heroes">The heroes of the savefile.</param>
        /// <param name="owner">The colour whose heroes are wanted.</param>
        /// <param name="openHero">Opens the page of the hero at an index among the heroes; the links do nothing without it.</param>
        public static IReadOnlyList<HeroLinkViewModel> Heroes<TColor>(IHeroSummaries<TColor> heroes, TColor owner, Action<int>? openHero)
            where TColor : class, IPlayerColor =>
            heroes.GetSummaries()
                .Where(s => s.Owner == owner)
                .OrderBy(s => s.Name)
                .Select(s => new HeroLinkViewModel(s.Index, s.Name, s.Description, new DelegateCommand(_ => openHero?.Invoke(s.Index), _ => openHero != null)))
                .ToList();
    }
}
