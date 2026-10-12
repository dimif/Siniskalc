namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the differences between two versions of the same player. Used for the pending changes list.
    /// </summary>
    public sealed class PlayerDiffer : IPlayerDiffer
    {
        private const string NoHero = "(none)";

        public IReadOnlyList<PendingChange> Diff(int playerIndex, PlayerModel original, PlayerModel current, Func<int, string> heroName)
        {
            var changes = new List<PendingChange>();

            foreach (var resource in PlayerResources.All)
            {
                var before = original.Resources[resource];
                var after = current.Resources[resource];
                if (before == after)
                {
                    continue;
                }

                changes.Add(Homm3PendingChanges.ForPlayer(
                    current.Color.ToString(),
                    playerIndex,
                    new Change(ChangeAreas.Resources, resource.ToString(), before.ToString("N0"), after.ToString("N0")),
                    p => p with { Resources = p.Resources.With(resource, before) }));
            }

            for (var place = 0; place < TavernHeroes.Places; place++)
            {
                var before = original.Tavern.Ids[place];
                var after = current.Tavern.Ids[place];
                if (before == after)
                {
                    continue;
                }

                var index = place;
                changes.Add(Homm3PendingChanges.ForPlayer(
                    current.Color.ToString(),
                    playerIndex,
                    new Change(ChangeAreas.General, $"Tavern hero {place + 1}", before is { } b ? heroName(b) : NoHero, after is { } a ? heroName(a) : NoHero),
                    p => p with { Tavern = p.Tavern.With(index, before) }));
            }

            return changes;
        }
    }
}
