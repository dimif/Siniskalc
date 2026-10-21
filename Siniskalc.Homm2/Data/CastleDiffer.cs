namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what an edit changed in a castle.
    /// </summary>
    public sealed class CastleDiffer : ICastleDiffer
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public CastleDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int castleIndex, CastleModel original, CastleModel current)
        {
            var changes = new ChangeSink<CastleModel>((change, revert) => this.pendingChanges.ForCastle(current.Name, castleIndex, change, revert));

            changes.AddOwner(original.Owner, current.Owner, (c, owner) => c with { Owner = owner });

            changes.AddArmy(ChangeAreas.Garrison, original.Garrison, current.Garrison, c => c.Garrison, (c, army) => c with { Garrison = army });

            foreach (var building in Enum.GetValues<TownBuildings>().Where(b => b != TownBuildings.None && b != TownBuildings.MageGuild))
            {
                var (had, has) = (original.Town.HasFlag(building), current.Town.HasFlag(building));
                if (had != has)
                {
                    var which = building;
                    changes.Add(ChangeAreas.Buildings, which.ToString(), had ? "built" : "not built", has ? "built" : "not built", c => c with { Town = had ? c.Town | which : c.Town & ~which });
                }
            }

            // A dwelling, the creatures that wait in it (wherever the savegame keeps them for its level) and the Mage Guild (its level, its
            // building and the spells each level shows) change together, so each is one change that is reverted as a whole.
            for (var dwelling = 1; dwelling <= Constants.Castle.DwellingCount; dwelling++)
            {
                var number = dwelling;
                if (!current.HasSameDwelling(original, number))
                {
                    changes.Add(ChangeAreas.Dwellings, $"Dwelling {number}", DwellingText(original, number), DwellingText(current, number), c => c.WithDwellingFrom(original, number));
                }
            }

            if (!current.HasSameMageGuild(original))
            {
                changes.Add(ChangeAreas.MageGuild, "Mage Guild", MageGuildText(original), MageGuildText(current), c => c.WithMageGuildFrom(original));
            }

            for (var slot = 0; slot < original.MageGuildSpells.Count && slot < current.MageGuildSpells.Count; slot++)
            {
                var (before, after, index) = (original.MageGuildSpells[slot], current.MageGuildSpells[slot], slot);
                if (before != after)
                {
                    var level = (slot / Constants.Castle.MageGuildSlotsPerLevel) + 1;
                    changes.Add(ChangeAreas.MageGuild, $"Level {level}, spell {(slot % Constants.Castle.MageGuildSlotsPerLevel) + 1}", SpellText(before), SpellText(after), c => c with { MageGuildSpells = c.MageGuildSpells.With(index, before) });
                }
            }

            return changes.All;
        }

        private static string DwellingText(CastleModel castle, int dwelling) =>
            castle.DwellingLevel(dwelling) switch { 0 => "not built", 1 => "built", 2 => "upgraded", _ => "upgraded twice" }
            + (castle.DwellingLevel(dwelling) > 0 ? $", {castle.CreaturesWaiting(dwelling):N0} waiting" : string.Empty);

        private static string MageGuildText(CastleModel castle) =>
            castle.MageGuildLevel == 0 ? "none" : $"level {castle.MageGuildLevel}, spells shown {string.Join("/", castle.MageGuildSpellCounts)}";

        private static string SpellText(int number) => number == 0 ? "none" : Spell.NameOf((byte)number);
    }
}
