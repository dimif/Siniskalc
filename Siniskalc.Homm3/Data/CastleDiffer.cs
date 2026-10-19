namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds the differences between two versions of the same castle. Used for the pending changes list.
    /// </summary>
    public sealed class CastleDiffer : ICastleDiffer
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public CastleDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        public IReadOnlyList<PendingChange> Diff(int castleIndex, CastleModel original, CastleModel current)
        {
            var changes = new ChangeSink<CastleModel>((change, revert) => this.pendingChanges.ForCastle(current.Name, castleIndex, change, revert));

            DiffGeneral(original, current, changes);
            DiffGarrison(original, current, changes);
            this.DiffBuildings(original, current, changes);
            DiffDwellings(original, current, changes);
            DiffMageGuildSpells(original, current, changes);

            return changes.All;
        }

        /// <summary>
        /// Puts one building back to what <paramref name="original"/> has, together with what depends on it: a building that
        /// was built since goes away with the buildings that need it, and one that was removed returns with what it needs.
        /// Whatever the building changed in the dwellings and the Mage Guild goes back too.
        /// </summary>
        public CastleModel RestoreBuilding(CastleModel castle, Building building, CastleModel original)
        {
            var restored = castle;
            if (original.HasBuilt(building))
            {
                foreach (var needed in (building.Upgrades == null ? [] : new[] { building.Upgrades }).Where(p => !restored.HasBuilt(p) && original.HasBuilt(p)))
                {
                    restored = this.RestoreBuilding(restored, needed, original);
                }
            }
            else
            {
                foreach (var dependent in restored.Buildings.Where(b => !CastleBuilder.IsAutomatic(b) && !original.HasBuilt(b) && b.Upgrades == building).ToList())
                {
                    restored = this.RestoreBuilding(restored, dependent, original);
                }
            }

            var dwellingLevel = building.DwellingLevel;
            var holderBefore = dwellingLevel != null ? CastleBuilder.DwellingOf(restored, dwellingLevel.Value) : null;
            restored = CastleBuilder.SetBuilt(restored, building, original.HasBuilt(building));

            // The creatures and spells of the building's level are what the original had, whatever the build moved.
            var creatures = restored.CreaturesAvailable.ToArray();
            var spells = restored.SpellCounts.ToArray();
            if (dwellingLevel != null)
            {
                // Counts edited on a dwelling that kept standing are their own pending change, so they are left alone.
                var level = dwellingLevel.Value;
                if (CastleBuilder.DwellingOf(restored, level) != holderBefore)
                {
                    creatures[level] = original.CreaturesAvailable[level];
                }
            }
            else if (building.IsMageGuild)
            {
                var spellLevel = building.MageGuildLevel!.Value - 1;
                spells[spellLevel] = original.SpellCounts[spellLevel];
            }

            return restored with { CreaturesAvailable = new Numbers(creatures), SpellCounts = new Numbers(spells) };
        }

        private static void DiffGeneral(CastleModel original, CastleModel current, ChangeSink<CastleModel> changes)
        {
            changes.AddOwner(original.Owner, current.Owner, (c, owner) => c with { Owner = owner });

            if (original.BuiltThisTurn != current.BuiltThisTurn)
            {
                changes.Add(ChangeAreas.General, "Built this turn", ChangeText.YesNo(original.BuiltThisTurn), ChangeText.YesNo(current.BuiltThisTurn), c => c with { BuiltThisTurn = original.BuiltThisTurn });
            }
        }

        private static void DiffGarrison(CastleModel original, CastleModel current, ChangeSink<CastleModel> changes) =>
            changes.AddArmy(ChangeAreas.Garrison, original.Garrison, current.Garrison, c => c.Garrison, (c, army) => c with { Garrison = army });

        private static void DiffDwellings(CastleModel original, CastleModel current, ChangeSink<CastleModel> changes)
        {
            for (var level = 0; level < original.CreaturesAvailable.Count; level++)
            {
                // A dwelling that was built, upgraded or removed has its creatures changed by that, which the building says.
                var dwelling = CastleBuilder.DwellingOf(current, level);
                if (dwelling == null || dwelling != CastleBuilder.DwellingOf(original, level))
                {
                    continue;
                }

                var before = CastleBuilder.CreaturesWaiting(original, level)!.Value;
                var after = CastleBuilder.CreaturesWaiting(current, level)!.Value;
                if (before == after)
                {
                    continue;
                }

                var index = level;
                changes.Add(ChangeAreas.Dwellings, $"{dwelling.Name} creatures", before.ToString("N0"), after.ToString("N0"), c => CastleBuilder.DwellingOf(c, index) == null ? c : CastleBuilder.SetCreaturesWaiting(c, index, before));
            }
        }

        private static void DiffMageGuildSpells(CastleModel original, CastleModel current, ChangeSink<CastleModel> changes)
        {
            for (var level = 1; level <= original.MageGuildSpells.Count && level <= current.MageGuildSpells.Count; level++)
            {
                var before = original.MageGuildSpells[level - 1];
                var after = current.MageGuildSpells[level - 1];
                for (var slot = 0; slot < before.Count && slot < after.Count; slot++)
                {
                    if (before[slot] == after[slot])
                    {
                        continue;
                    }

                    var (spellLevel, index, originalSpell) = (level, slot, before[slot]);
                    changes.Add(ChangeAreas.Spells, $"Mage Guild level {level}, spell {slot + 1}", before[slot].Name, after[slot].Name, c => c.MageGuildSpells.Count >= spellLevel && c.MageGuildSpells[spellLevel - 1].Count > index ? c.WithMageGuildSpell(spellLevel, index, originalSpell) : c);
                }
            }
        }

        private void DiffBuildings(CastleModel original, CastleModel current, ChangeSink<CastleModel> changes)
        {
            // The upgraded horde buildings are built by the game together with the building they belong to, so they are not listed.
            foreach (var building in current.Buildings.Except(original.Buildings).Concat(original.Buildings.Except(current.Buildings))
                .Where(b => !CastleBuilder.IsAutomatic(b))
                .OrderBy(b => b.Code))
            {
                var built = current.HasBuilt(building);
                changes.Add(ChangeAreas.Buildings, building.Name, built ? "not built" : "built", built ? "built" : "not built", c => this.RestoreBuilding(c, building, original));
            }
        }
    }
}
