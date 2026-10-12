namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Lists the obelisks that a colour visited or no longer has visited, one change for each obelisk and colour.
    /// </summary>
    public sealed class ObeliskDiffer : IObeliskDiffer
    {
        public IReadOnlyList<PendingChange> Diff(int index, ObeliskVisits original, ObeliskVisits current)
        {
            var changes = new List<PendingChange>();
            for (var i = 0; i < original.Count && i < current.Count; i++)
            {
                foreach (var color in Enum.GetValues<PlayerColor>())
                {
                    var before = original.IsVisited(i, color);
                    var after = current.IsVisited(i, color);
                    if (before == after)
                    {
                        continue;
                    }

                    var obelisk = i;
                    var who = color;
                    changes.Add(Homm3PendingChanges.ForObelisks(
                        index,
                        new Change(ChangeAreas.General, $"Obelisk {i + 1}, {color}", Text(before), Text(after)),
                        visits => visits.WithVisited(obelisk, who, before)));
                }
            }

            return changes;
        }

        private static string Text(bool visited) => visited ? "visited" : "not visited";
    }
}
