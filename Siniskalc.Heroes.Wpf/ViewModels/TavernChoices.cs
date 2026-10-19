namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Works out who can be put in a place of a tavern.
    /// </summary>
    public static class TavernChoices
    {
        /// <summary>
        /// Lists who can be put in the place at <paramref name="place"/> (0 for the first): nobody, the hero that is in it now, and the
        /// heroes no player owns, except the one in the other place.
        /// </summary>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <param name="tavern">The tavern as the savefile has it.</param>
        /// <param name="place">The place, 0 for the first.</param>
        /// <param name="heroes">Every hero of the savefile.</param>
        public static IReadOnlyList<HeroChoiceViewModel> For<TColor>(TavernHeroes tavern, int place, IEnumerable<HeroSummary<TColor>> heroes)
            where TColor : class, IPlayerColor
        {
            var inPlace = tavern.Ids[place];
            var inOther = tavern.Ids[TavernHeroes.Places - 1 - place];
            var candidates = heroes
                .Where(h => h.HeroId == inPlace || (h.Owner == null && h.HeroId != inOther))
                .OrderBy(h => h.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(h => h.HeroId)
                .ToList();

            // Names are not unique in a savefile (there are two Lord Haarts, say), so a repeated name is told apart by the id.
            var repeated = candidates.GroupBy(h => h.Name).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();
            var choices = new List<HeroChoiceViewModel> { new(null, "(none)") };
            choices.AddRange(candidates.Select(h => new HeroChoiceViewModel(h.HeroId, repeated.Contains(h.Name) ? $"{h.Name} (id {h.HeroId})" : h.Name)));
            if (inPlace is { } id && candidates.All(h => h.HeroId != id))
            {
                // The place holds an id that the savefile has no hero for.
                choices.Add(new HeroChoiceViewModel(id, $"Unknown hero (id {id})"));
            }

            return choices;
        }
    }
}
