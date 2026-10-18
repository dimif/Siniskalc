namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
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
            var name = current.Color.ToString();

            var changes = new ChangeSink<PlayerModel>((change, revert) => Homm2PendingChanges.ForPlayer(name, playerIndex, change, revert));

            foreach (var resource in Enum.GetValues<Resource>())
            {
                var (before, after) = (original.Resources[resource], current.Resources[resource]);
                if (before != after)
                {
                    var which = resource;
                    changes.Add(ChangeAreas.Resources, which.ToString(), before.ToString("N0"), after.ToString("N0"), p => p with { Resources = p.Resources.With(which, before) });
                }
            }

            if (!original.HeroIds.Equals(current.HeroIds))
            {
                changes.Add(ChangeAreas.Heroes, "Heroes", List(original.HeroIds), List(current.HeroIds), p => p with { HeroIds = original.HeroIds });
            }

            if (!original.CastleIds.Equals(current.CastleIds))
            {
                changes.Add(ChangeAreas.Castles, "Castles", List(original.CastleIds), List(current.CastleIds), p => p with { CastleIds = original.CastleIds });
            }

            if (original.DaysLeft != current.DaysLeft)
            {
                changes.Add(ChangeAreas.General, "Days left without a castle", DaysText(original.DaysLeft), DaysText(current.DaysLeft), p => p with { DaysLeft = original.DaysLeft });
            }

            if (original.SelectedHero != current.SelectedHero)
            {
                changes.Add(ChangeAreas.General, "Selected hero", ChangeText.Number(original.SelectedHero), ChangeText.Number(current.SelectedHero), p => p with { SelectedHero = original.SelectedHero });
            }

            if (original.SelectedCastle != current.SelectedCastle)
            {
                changes.Add(ChangeAreas.General, "Selected castle", ChangeText.Number(original.SelectedCastle), ChangeText.Number(current.SelectedCastle), p => p with { SelectedCastle = original.SelectedCastle });
            }

            if (original.Tavern != current.Tavern)
            {
                changes.Add(ChangeAreas.Heroes, "Tavern", TavernText(original.Tavern), TavernText(current.Tavern), p => p with { Tavern = original.Tavern });
            }

            return changes.All;
        }

        private static string List(Numbers ids) => ids.Count == 0 ? "none" : string.Join(", ", ids);

        private static string TavernText(TavernHeroes tavern) => $"{ChangeText.Number(tavern.First)}, {ChangeText.Number(tavern.Second)}";

        private static string DaysText(int? days) => days?.ToString() ?? "not counting";
    }
}
