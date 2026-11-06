namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The heroes the game keeps out of play in a savefile: the ones disabled in the map editor and the campaign heroes. Their entry in the
    /// table of hero owners is reserved, and unlike the heroes a tavern offers (whose entry is reserved too) no player's tavern holds them.
    /// </summary>
    public static class DisabledHeroes
    {
        /// <summary>
        /// Returns the ids of the heroes the game keeps out of play in <paramref name="document"/>.
        /// </summary>
        /// <param name="document">The savefile.</param>
        /// <returns>The ids, or none for a savefile without a table of hero owners.</returns>
        public static IReadOnlySet<int> Of(SaveDocument document)
        {
            var owners = document.HeroOwners();
            if (owners.Count == 0)
            {
                return new HashSet<int>();
            }

            var table = owners.GetModel(0);
            var players = document.Players();
            var offered = Enumerable.Range(0, players.Count).SelectMany(i => players.GetModel(i).Tavern.Ids).ToHashSet();
            return Enumerable.Range(0, table.Heroes.Count)
                .Where(id => table.Has(id) && table.Heroes[id].IsReserved && !offered.Contains(id))
                .ToHashSet();
        }
    }
}
