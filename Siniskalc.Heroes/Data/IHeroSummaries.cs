namespace Siniskalc.Heroes.Data
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The heroes of a savefile as the lists of the pages need them: a short summary of each, and a way to find one by the id a castle or
    /// a tavern holds. A game's collection of heroes implements it, so a page that only lists heroes need not know how the game stores them.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public interface IHeroSummaries<TColor>
        where TColor : class, IPlayerColor
    {
        /// <summary>
        /// Returns a short summary of every hero, in the order of the savefile.
        /// </summary>
        IReadOnlyList<HeroSummary<TColor>> GetSummaries();

        /// <summary>
        /// Returns the summary of the hero with the given id, or <c>null</c> if the savefile has no such hero.
        /// </summary>
        /// <param name="heroId">The id a castle or a tavern holds for the hero.</param>
        HeroSummary<TColor>? FindById(int heroId);
    }
}
