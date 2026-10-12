namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what an edit changed in a player. What the game works out itself (the income of the day) is not listed, but it is saved.
    /// </summary>
    public sealed class Homm2PlayerDiffer : IHomm2PlayerDiffer
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int playerIndex, Homm2PlayerModel original, Homm2PlayerModel current)
        {
            var changes = new List<PendingChange>();
            var name = current.Color.ToString();

            void Add(ChangeArea area, string what, string before, string after, Func<Homm2PlayerModel, Homm2PlayerModel> revert)
            {
                changes.Add(Homm2PendingChanges.ForPlayer(name, playerIndex, new Change(area, what, before, after), revert));
            }

            foreach (var resource in Enum.GetValues<Homm2Resource>())
            {
                var (before, after) = (original.Resources[resource], current.Resources[resource]);
                if (before != after)
                {
                    var which = resource;
                    Add(Homm2ChangeAreas.Resources, which.ToString(), before.ToString("N0"), after.ToString("N0"), p => p with { Resources = p.Resources.With(which, before) });
                }
            }

            if (!original.HeroIds.Equals(current.HeroIds))
            {
                Add(Homm2ChangeAreas.Heroes, "Heroes", List(original.HeroIds), List(current.HeroIds), p => p with { HeroIds = original.HeroIds });
            }

            if (!original.CastleIds.Equals(current.CastleIds))
            {
                Add(Homm2ChangeAreas.Castles, "Castles", List(original.CastleIds), List(current.CastleIds), p => p with { CastleIds = original.CastleIds });
            }

            if (original.DaysLeft != current.DaysLeft)
            {
                Add(Homm2ChangeAreas.General, "Days left without a castle", DaysText(original.DaysLeft), DaysText(current.DaysLeft), p => p with { DaysLeft = original.DaysLeft });
            }

            return changes;
        }

        private static string List(Homm2Numbers ids) => ids.Count == 0 ? "none" : string.Join(", ", ids);

        private static string DaysText(int? days) => days?.ToString() ?? "not counting";
    }
}
