namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what an edit changed in a hero.
    /// </summary>
    public sealed class HeroDiffer : IDiffer<HeroModel>
    {
        /// <summary>
        /// Whether a place on the map says its level: the map of HoMM2 has one level.
        /// </summary>
        private const bool HasLevels = false;

        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public HeroDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int heroIndex, HeroModel original, HeroModel current)
        {
            // The owner of a hero is also in the lists of heroes of the players: putting it back puts those back too (PlayerListsReconciler).
            var changes = new ChangeSink<HeroModel>((change, revert) => this.pendingChanges.ForHero(current.Name, heroIndex, change, revert));
            changes.AddOwner(original.Owner, current.Owner, (h, owner) => h with { Owner = owner });

            changes.AddPosition(original.Position, current.Position, HasLevels, (h, position) => h with { Position = position });
            changes.AddDestination(original.Destination, current.Destination, HasLevels, (h, destination) => h with { Destination = destination });
            changes.AddSpellPoints(original.SpellPoints, current.SpellPoints, (h, points) => h with { SpellPoints = points });
            changes.AddMovement(original.MovementPoints, current.MovementPoints, (h, movement) => h with { MovementPoints = movement });
            changes.AddHeroLevel(original.HeroLevel, current.HeroLevel, (h, level) => h with { HeroLevel = level });

            changes.AddNextBattleBonuses(
                original.NextBattle.Morale,
                current.NextBattle.Morale,
                original.NextBattle.Luck,
                current.NextBattle.Luck,
                (h, morale) => h with { NextBattle = h.NextBattle with { Morale = morale } },
                (h, luck) => h with { NextBattle = h.NextBattle with { Luck = luck } });

            if (original.Effects != current.Effects)
            {
                changes.Add(ChangeAreas.General, "Remembered effects", original.Effects.ToString(), current.Effects.ToString(), h => h with { Effects = original.Effects });
            }

            if (original.Visited != current.Visited)
            {
                changes.Add(ChangeAreas.General, "Places visited", original.Visited.Count.ToString(), current.Visited.Count.ToString(), h => h with { Visited = original.Visited });
            }

            DiffPrimarySkills(original, current, changes);
            DiffSecondarySkills(original, current, changes);

            changes.AddArmy(ChangeAreas.Creatures, original.Army, current.Army, h => h.Army, (h, army) => h with { Army = army });

            for (var slot = 0; slot < Constants.Hero.ArtifactSlotCount; slot++)
            {
                var (before, after, index) = (original.Artifacts.Slots[slot], current.Artifacts.Slots[slot], slot);
                if (before != after)
                {
                    changes.Add(ChangeAreas.Artifacts, $"Slot {slot + 1}", before?.Name ?? ChangeText.Empty, after?.Name ?? ChangeText.Empty, h => h with { Artifacts = h.Artifacts.WithSlot(index, before) });
                }
            }

            changes.AddLearnedSpells(original.Spells, current.Spells, (h, spell, learned) => h with { Spells = learned ? h.Spells.With(spell) : h.Spells.Without(spell) });

            return changes.All;
        }

        private static void DiffPrimarySkills(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes) =>
            changes.AddPrimarySkills(original.PrimarySkills, current.PrimarySkills, h => h.PrimarySkills, (h, skills) => h with { PrimarySkills = skills });

        private static void DiffSecondarySkills(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes) =>
            changes.AddSecondarySkills(SecondarySkill.All, original.SecondarySkills, current.SecondarySkills, (h, skills) => h with { SecondarySkills = skills });
    }
}
