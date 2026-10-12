namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Entities;

    /// <summary>
    /// Builds and removes the buildings of a <see cref="CastleModel"/> the way the game does, so the three building masks of
    /// the savefile stay consistent with each other. Every method returns a new model.
    /// </summary>
    /// <remarks>
    /// What the savefiles of Rampart, Bulwark, Cove and Factory castles show about a build:
    /// the building joins <see cref="CastleModel.Buildings"/> (everything ever built) and <see cref="CastleModel.CurrentBuildings"/>
    /// (what stands now), and the buildings it <see cref="Building.Upgrades"/> leave the second set. A dwelling that is built
    /// starts with one week of creatures; when it is upgraded the creatures waiting move to the upgraded dwelling. When a horde
    /// building (Miners' Guild, Hay Bins, ...) and the upgrade of its dwelling both exist the game adds the upgraded horde building
    /// by itself, and that one replaces the horde building and the upgraded dwelling in the second set.
    /// The mask of the buildings the map allows is never touched by a build.
    /// </remarks>
    public static class CastleBuilder
    {
        /// <summary>
        /// Gets whether the game builds <paramref name="building"/> by itself once what it needs exists: the upgraded horde
        /// buildings, whose bits are 19 and 25.
        /// </summary>
        public static bool IsAutomatic(Building building) => building.Faction != null && (building.Code == 19 || building.Code == 25);

        /// <summary>
        /// Returns the buildings that <paramref name="building"/> requires (<see cref="Building.Requires"/>) and the castle has
        /// not built. This is information only: the buildings may be built and removed in any order.
        /// </summary>
        public static IReadOnlyList<Building> UnmetRequirements(CastleModel castle, Building building)
        {
            return building.Requires.Where(r => !castle.HasBuilt(r)).ToList();
        }

        /// <summary>
        /// Returns why <paramref name="building"/> cannot be built in the castle now, or <see cref="CastleBuildFailureReason.None"/> if it can. The only rule is that an
        /// upgrade needs the building it upgrades; the other prerequisites are not enforced.
        /// </summary>
        public static CastleBuildFailureReason GetBuildFailureReason(CastleModel castle, Building building)
        {
            if (castle.Faction == null || !building.IsAvailableFor(castle.Faction))
            {
                return CastleBuildFailureReason.NotAvailableForTown;
            }

            if (castle.HasBuilt(building))
            {
                return CastleBuildFailureReason.AlreadyBuilt;
            }

            if (IsAutomatic(building))
            {
                return CastleBuildFailureReason.AutomaticBuilding;
            }

            if (!castle.EnabledBuildings.Contains(building))
            {
                return CastleBuildFailureReason.NotAllowedByMap;
            }

            // Only the building an upgrade replaces has to exist; what a building merely requires (Building.Requires) may come later.
            return building.Upgrades != null && !castle.HasBuilt(building.Upgrades) ? CastleBuildFailureReason.UpgradedBuildingMissing : CastleBuildFailureReason.None;
        }

        /// <summary>
        /// Returns why <paramref name="building"/> cannot be removed from the castle now, or <see cref="CastleBuildFailureReason.None"/> if it can.
        /// </summary>
        public static CastleBuildFailureReason GetUnbuildFailureReason(CastleModel castle, Building building)
        {
            if (!castle.HasBuilt(building))
            {
                return CastleBuildFailureReason.NotBuilt;
            }

            if (building == Building.VillageHall)
            {
                return CastleBuildFailureReason.VillageHallRequired;
            }

            if (IsAutomatic(building))
            {
                return CastleBuildFailureReason.AutomaticBuilding;
            }

            return UpgradesBuiltOn(castle, building).Count == 0 ? CastleBuildFailureReason.None : CastleBuildFailureReason.NeededByUpgrade;
        }

        /// <summary>
        /// Returns the built buildings that are upgrades on top of <paramref name="building"/> and so keep it from being removed. Buildings
        /// that merely require it (<see cref="Building.Requires"/>) do not, and neither do the ones the game builds by itself.
        /// </summary>
        public static IReadOnlyList<Building> UpgradesBuiltOn(CastleModel castle, Building building)
        {
            return castle.Buildings.Where(b => b != building && !IsAutomatic(b) && b.Upgrades == building).ToList();
        }

        /// <summary>
        /// Builds <paramref name="building"/>. Throws <see cref="GameRuleException"/> if the game would not allow it.
        /// </summary>
        public static CastleModel Build(CastleModel castle, Building building)
        {
            var reason = GetBuildFailureReason(castle, building);
            return reason == CastleBuildFailureReason.None ? SetBuilt(castle, building, true) : throw new GameRuleException($"{building.Name}: {reason}.");
        }

        /// <summary>
        /// Removes <paramref name="building"/>. Throws <see cref="GameRuleException"/> if another building needs it.
        /// </summary>
        public static CastleModel Unbuild(CastleModel castle, Building building)
        {
            var reason = GetUnbuildFailureReason(castle, building);
            return reason == CastleBuildFailureReason.None ? SetBuilt(castle, building, false) : throw new GameRuleException($"{building.Name}: {reason}.");
        }

        /// <summary>
        /// Returns why the map permission of <paramref name="building"/> cannot be changed, or <see cref="CastleBuildFailureReason.None"/> if it can. The permission only
        /// matters for a building that is not built, and the game decides the upgraded horde buildings by itself.
        /// </summary>
        public static CastleBuildFailureReason GetSetAllowedFailureReason(CastleModel castle, Building building)
        {
            if (castle.Faction == null || !building.IsAvailableFor(castle.Faction))
            {
                return CastleBuildFailureReason.NotAvailableForTown;
            }

            if (castle.HasBuilt(building))
            {
                return CastleBuildFailureReason.AlreadyBuilt;
            }

            return IsAutomatic(building) ? CastleBuildFailureReason.AutomaticBuilding : CastleBuildFailureReason.None;
        }

        /// <summary>
        /// Allows or forbids building <paramref name="building"/> (the mask of the buildings the map allows). Throws
        /// <see cref="GameRuleException"/> if the permission does not apply to the building.
        /// </summary>
        public static CastleModel SetAllowed(CastleModel castle, Building building, bool allowed)
        {
            var reason = GetSetAllowedFailureReason(castle, building);
            if (reason != CastleBuildFailureReason.None)
            {
                throw new GameRuleException($"{building.Name}: {reason}.");
            }

            var enabled = new HashSet<Building>(castle.EnabledBuildings);
            _ = allowed ? enabled.Add(building) : enabled.Remove(building);
            return castle with { EnabledBuildings = enabled };
        }

        /// <summary>
        /// Puts <paramref name="spell"/> in a slot of the Mage Guild. The spell must be one of the spell level of the slot and one a guild
        /// can hold. If the guild already holds it in another slot of the level, the two slots swap places, as a guild never holds a spell twice.
        /// Throws <see cref="GameRuleException"/> when the rules do not allow it, and <see cref="ArgumentOutOfRangeException"/> for a slot that does not exist.
        /// </summary>
        /// <param name="level">The spell level, 1 to 5.</param>
        /// <param name="slot">The place in the list of the level, from 0.</param>
        public static CastleModel SetMageGuildSpell(CastleModel castle, int level, int slot, Spell spell)
        {
            RangeGuard.ThrowIfOutOfRange(level, 1, castle.MageGuildSpells.Count, nameof(level));
            RangeGuard.ThrowIfNotIndex(slot, castle.MageGuildSpells[level - 1].Count, nameof(slot));
            if (spell.Level != level)
            {
                throw new GameRuleException($"{spell.Name} is a level {spell.Level} spell and cannot be in the level {level} spells of a Mage Guild.");
            }

            if (!spell.CanBeInMageGuild)
            {
                throw new GameRuleException($"{spell.Name} cannot be in a Mage Guild.");
            }

            var current = castle.MageGuildSpells[level - 1];
            var other = current.ToList().IndexOf(spell);
            if (other == slot)
            {
                return castle;
            }

            var result = castle;
            if (other >= 0)
            {
                result = result.WithMageGuildSpell(level, other, current[slot]);
            }

            return result.WithMageGuildSpell(level, slot, spell);
        }

        /// <summary>
        /// Builds the Mage Guild up to <paramref name="level"/>, or removes the levels above it. Throws
        /// <see cref="GameRuleException"/> if a level cannot be built or removed.
        /// </summary>
        public static CastleModel SetMageGuildLevel(CastleModel castle, int level)
        {
            var levels = new[] { Building.MagesGuild1, Building.MagesGuild2, Building.MagesGuild3, Building.MagesGuild4, Building.MagesGuild5 };
            var maximum = castle.Faction == null ? 0 : levels.Count(l => l.IsAvailableFor(castle.Faction));
            RangeGuard.ThrowIfOutOfRange(level, 0, int.MaxValue, nameof(level));
            if (level > maximum)
            {
                throw new GameRuleException($"A Mage Guild of this town goes up to level {maximum}.");
            }

            for (var next = 1; next <= level; next++)
            {
                if (!castle.HasBuilt(levels[next - 1]))
                {
                    castle = Build(castle, levels[next - 1]);
                }
            }

            for (var top = levels.Length; top > level; top--)
            {
                if (castle.HasBuilt(levels[top - 1]))
                {
                    castle = Unbuild(castle, levels[top - 1]);
                }
            }

            return castle;
        }

        /// <summary>
        /// Returns the dwelling that holds the creatures of <paramref name="level"/> (0 to 6) in the castle: the upgraded dwelling
        /// if it is built, otherwise the dwelling, otherwise null.
        /// </summary>
        public static Building? DwellingOf(CastleModel castle, int level)
        {
            if (castle.Faction == null || !RangeGuard.IsInRange(level, 0, castle.CreaturesAvailable.Count - 1))
            {
                return null;
            }

            var upgraded = Building.FromDwellingLevel(castle.Faction, level, upgraded: true);
            if (upgraded != null && castle.HasBuilt(upgraded))
            {
                return upgraded;
            }

            var dwelling = Building.FromDwellingLevel(castle.Faction, level, upgraded: false);
            return dwelling != null && castle.HasBuilt(dwelling) ? dwelling : null;
        }

        /// <summary>
        /// Returns how many creatures of <paramref name="level"/> (0 to 6) wait in the castle, or null if no dwelling is built for them.
        /// </summary>
        public static int? CreaturesWaiting(CastleModel castle, int level)
        {
            var dwelling = DwellingOf(castle, level);
            return dwelling == null ? null : dwelling.IsUpgradedDwelling ? castle.UpgradedCreaturesAvailable[level] : castle.CreaturesAvailable[level];
        }

        /// <summary>
        /// Sets how many creatures of <paramref name="level"/> (0 to 6) wait in the dwelling that stands for them. Throws
        /// <see cref="GameRuleException"/> if there is no such dwelling.
        /// </summary>
        public static CastleModel SetCreaturesWaiting(CastleModel castle, int level, int count)
        {
            RangeGuard.ThrowIfOutOfRange<long>(
                count,
                Constants.Castle.MinDwellingCreatures,
                Constants.Castle.MaxDwellingCreatures,
                nameof(count),
                $"A dwelling holds {Constants.Castle.MinDwellingCreatures} to {Constants.Castle.MaxDwellingCreatures} creatures.");

            var dwelling = DwellingOf(castle, level) ?? throw new GameRuleException($"No dwelling is built for the creatures of level {level + 1}.");
            var creatures = castle.CreaturesAvailable.ToArray();
            var upgraded = castle.UpgradedCreaturesAvailable.ToArray();
            (dwelling.IsUpgradedDwelling ? upgraded : creatures)[level] = count;

            return castle with { CreaturesAvailable = creatures, UpgradedCreaturesAvailable = upgraded };
        }

        /// <summary>
        /// Builds or removes <paramref name="building"/> without asking whether the game allows it, and follows with what the
        /// game does by itself. Used to put a single change back; <see cref="Build"/> and <see cref="Unbuild"/> check first.
        /// </summary>
        public static CastleModel SetBuilt(CastleModel castle, Building building, bool built)
        {
            if (castle.Faction == null || castle.HasBuilt(building) == built)
            {
                return castle;
            }

            var buildings = castle.Buildings.ToHashSet();
            var current = castle.CurrentBuildings.ToHashSet();
            var creatures = castle.CreaturesAvailable.ToArray();
            var upgraded = castle.UpgradedCreaturesAvailable.ToArray();
            var spells = castle.SpellCounts.ToArray();

            if (built)
            {
                Add(castle.Faction, building, buildings, current, creatures, upgraded, spells);

                // What the game adds by itself once the building and what it works with both exist.
                foreach (var automatic in Building.ForFaction(castle.Faction).Where(b => IsAutomatic(b) && !buildings.Contains(b)))
                {
                    if (automatic.Prerequisites.All(buildings.Contains))
                    {
                        Add(castle.Faction, automatic, buildings, current, creatures, upgraded, spells);
                    }
                }
            }
            else
            {
                var removed = new List<Building> { building };
                removed.AddRange(Building.ForFaction(castle.Faction).Where(b => IsAutomatic(b) && buildings.Contains(b) && b.Prerequisites.Contains(building)));

                foreach (var gone in removed)
                {
                    Remove(gone, buildings, current, creatures, upgraded, spells);
                }

                // The buildings that were hidden by the removed ones stand again, unless another building still covers them.
                var covered = buildings.SelectMany(Replaced).ToHashSet();
                foreach (var shown in removed.SelectMany(Replaced).Where(b => buildings.Contains(b) && !covered.Contains(b)))
                {
                    current.Add(shown);
                }
            }

            var mageGuild = buildings.Where(b => b.IsMageGuild).Select(b => b.MageGuildLevel!.Value).DefaultIfEmpty(0).Max();

            return castle with
            {
                Buildings = buildings,
                CurrentBuildings = current,
                CreaturesAvailable = creatures,
                UpgradedCreaturesAvailable = upgraded,
                SpellCounts = spells,
                MageGuildLevel = mageGuild,
            };
        }

        /// <summary>
        /// Returns the buildings that <paramref name="building"/> takes the place of: those it upgrades, and for an automatic
        /// building also the ones it needs.
        /// </summary>
        public static IEnumerable<Building> Replaced(Building building)
        {
            var result = new List<Building>();
            for (var next = building.Upgrades; next != null; next = next.Upgrades)
            {
                result.Add(next);
            }

            if (IsAutomatic(building))
            {
                foreach (var needed in building.Requires)
                {
                    result.Add(needed);
                    result.AddRange(Replaced(needed));
                }
            }

            return result;
        }

        private static void Add(Faction faction, Building building, HashSet<Building> buildings, HashSet<Building> current, int[] creatures, int[] upgraded, int[] spells)
        {
            // A building that an upgrade already took the place of does not stand when it is built later.
            var covered = buildings.SelectMany(Replaced).Contains(building);
            buildings.Add(building);
            if (!covered)
            {
                current.Add(building);
            }

            current.ExceptWith(Replaced(building));

            if (building.IsDwelling)
            {
                var level = building.DwellingLevel!.Value;
                var creature = Creature.ForFaction(faction).FirstOrDefault(c => c.Tier == level + 1);
                creatures[level] = creature?.Info.Growth ?? 0;
            }
            else if (building.IsUpgradedDwelling)
            {
                // The creatures waiting move to the upgraded dwelling.
                var level = building.DwellingLevel!.Value;
                upgraded[level] = creatures[level];
                creatures[level] = 0;
            }
            else if (building.IsMageGuild)
            {
                spells[building.MageGuildLevel!.Value - 1] = Building.MageGuildLevelCount - building.MageGuildLevel.Value + 1;
            }
        }

        private static void Remove(Building building, HashSet<Building> buildings, HashSet<Building> current, int[] creatures, int[] upgraded, int[] spells)
        {
            buildings.Remove(building);
            current.Remove(building);

            if (building.IsDwelling)
            {
                creatures[building.DwellingLevel!.Value] = 0;
            }
            else if (building.IsUpgradedDwelling)
            {
                var level = building.DwellingLevel!.Value;
                creatures[level] = upgraded[level];
                upgraded[level] = 0;
            }
            else if (building.IsMageGuild)
            {
                spells[building.MageGuildLevel!.Value - 1] = 0;
            }
        }
    }
}
