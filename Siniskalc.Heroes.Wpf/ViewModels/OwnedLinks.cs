namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Makes the lists of what a colour owns that the pages of every game show: its heroes and its castles, by name.
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
        /// <returns>The links.</returns>
        public static IReadOnlyList<HeroLinkViewModel> Heroes<TColor>(IHeroSummaries<TColor> heroes, TColor owner, Action<int>? openHero)
            where TColor : class, IPlayerColor =>
            heroes.GetSummaries()
                .Where(s => s.Owner == owner)
                .OrderBy(s => s.Name)
                .Select(s => new HeroLinkViewModel(s.Index, s.Name, s.Description, new DelegateCommand(_ => openHero?.Invoke(s.Index), _ => openHero != null)))
                .ToList();

        /// <summary>
        /// Returns a link for each castle <paramref name="owner"/> owns that the map uses, ordered by name, with the heroes in it. The game
        /// says what it calls the settlement, and whether a castle is a capital and has a Grail structure.
        /// </summary>
        /// <typeparam name="TCastle">The castle of the game.</typeparam>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <typeparam name="TCreature">The creature of the game.</typeparam>
        /// <typeparam name="TFaction">The faction of the game.</typeparam>
        /// <param name="castles">The castles of the savefile, in their order; a castle is opened by its place in the list.</param>
        /// <param name="heroes">The heroes of the savefile, which a castle names by number.</param>
        /// <param name="owner">The colour whose castles are wanted.</param>
        /// <param name="openCastle">Opens the page of the castle at an index; the links do nothing without it.</param>
        /// <param name="openHero">Opens the page of the hero at an index among the heroes.</param>
        /// <param name="settlement">What the game calls the settlement of a castle, such as "City" and "Citadel", or "Castle".</param>
        /// <param name="isCapital">Tells whether a castle is a capital, or <c>null</c> in a game that has none.</param>
        /// <param name="grailText">Gives the name of the Grail structure built in a castle, or <c>null</c> in a game that has none.</param>
        public static IReadOnlyList<CastleLinkViewModel> Castles<TCastle, TColor, TCreature, TFaction>(
            IReadOnlyList<TCastle> castles,
            IHeroSummaries<TColor> heroes,
            TColor owner,
            Action<int>? openCastle,
            Action<int>? openHero,
            Func<TCastle, IEnumerable<string>> settlement,
            Func<TCastle, bool>? isCapital = null,
            Func<TCastle, string>? grailText = null)
            where TCastle : ICastleModel<TColor, TCreature, TFaction>
            where TColor : class, IPlayerColor
            where TCreature : class, IGameEntity
            where TFaction : class, IFaction =>
            castles
                .Select((castle, index) => (Castle: castle, Index: index))
                .Where(c => c.Castle.IsUsed && EqualityComparer<TColor>.Default.Equals(c.Castle.Owner, owner))
                .OrderBy(c => c.Castle.Name)
                .Select(c => CastleLinkViewModel.For(
                    c.Castle,
                    new DelegateCommand(_ => openCastle?.Invoke(c.Index), _ => openCastle != null),
                    settlement(c.Castle),
                    isCapital?.Invoke(c.Castle) ?? false,
                    grailText?.Invoke(c.Castle) ?? string.Empty,
                    CastleHeroRows.For(c.Castle, heroes, openHero)))
                .ToList();
    }
}
