namespace Siniskalc.Heroes.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Lists the obelisks that a colour visited or no longer has visited, one change for each obelisk and colour.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public sealed class ObeliskDiffer<TColor> : IDiffer<ObeliskVisits<TColor>>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        private readonly IReadOnlyList<TColor> colors;
        private readonly Func<int, Change, Func<ObeliskVisits<TColor>, ObeliskVisits<TColor>>, PendingChange> makeChange;

        /// <param name="colors">The colours of the players of the game.</param>
        /// <param name="makeChange">Makes the pending change of the obelisks from a change and the way to put it back (a game's <c>ForObelisks</c>).</param>
        public ObeliskDiffer(IReadOnlyList<TColor> colors, Func<int, Change, Func<ObeliskVisits<TColor>, ObeliskVisits<TColor>>, PendingChange> makeChange)
        {
            this.colors = colors;
            this.makeChange = makeChange;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, ObeliskVisits<TColor> original, ObeliskVisits<TColor> current)
        {
            var changes = new List<PendingChange>();
            for (var i = 0; i < original.Count && i < current.Count; i++)
            {
                foreach (var color in this.colors)
                {
                    var before = original.IsVisited(i, color);
                    var after = current.IsVisited(i, color);
                    if (before == after)
                    {
                        continue;
                    }

                    var obelisk = i;
                    var who = color;
                    changes.Add(this.makeChange(
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
