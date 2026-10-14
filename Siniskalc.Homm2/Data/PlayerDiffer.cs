namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what an edit changed in a player. What the game works out itself (the income of the day) is not listed, but it is saved.
    /// </summary>
    public sealed class PlayerDiffer : IPlayerDiffer
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int playerIndex, PlayerModel original, PlayerModel current)
        {
            var changes = new List<PendingChange>();
            var name = current.Color.ToString();

            void Add(ChangeArea area, string what, string before, string after, Func<PlayerModel, PlayerModel> revert)
            {
                changes.Add(Homm2PendingChanges.ForPlayer(name, playerIndex, new Change(area, what, before, after), revert));
            }

            foreach (var resource in Enum.GetValues<Resource>())
            {
                var (before, after) = (original.Resources[resource], current.Resources[resource]);
                if (before != after)
                {
                    var which = resource;
                    Add(ChangeAreas.Resources, which.ToString(), before.ToString("N0"), after.ToString("N0"), p => p with { Resources = p.Resources.With(which, before) });
                }
            }

            if (!original.HeroIds.Equals(current.HeroIds))
            {
                Add(ChangeAreas.Heroes, "Heroes", List(original.HeroIds), List(current.HeroIds), p => p with { HeroIds = original.HeroIds });
            }

            if (!original.CastleIds.Equals(current.CastleIds))
            {
                Add(ChangeAreas.Castles, "Castles", List(original.CastleIds), List(current.CastleIds), p => p with { CastleIds = original.CastleIds });
            }

            if (original.DaysLeft != current.DaysLeft)
            {
                Add(ChangeAreas.General, "Days left without a castle", DaysText(original.DaysLeft), DaysText(current.DaysLeft), p => p with { DaysLeft = original.DaysLeft });
            }

            return changes;
        }

        private static string List(Numbers ids) => ids.Count == 0 ? "none" : string.Join(", ", ids);

        private static string DaysText(int? days) => days?.ToString() ?? "not counting";
    }
}
