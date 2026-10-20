namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what an edit changed in a hero. What the game works out itself (the movement of a full day) and the place the hero stands on are
    /// not listed, but they are saved.
    /// </summary>
    public sealed class HeroDiffer : IHeroDiffer
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public HeroDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int heroIndex, HeroModel original, HeroModel current)
        {
            var changes = new ChangeSink<HeroModel>((change, revert) => this.pendingChanges.ForHero(current.Name, heroIndex, change, revert));

            changes.AddOwner(original.Owner, current.Owner, (h, owner) => h with { Owner = owner });

            if (original.Position.X != current.Position.X || original.Position.Y != current.Position.Y)
            {
                changes.Add(ChangeAreas.General, "Position", original.Position.ToString(), current.Position.ToString(), h => h with { Position = h.Position with { X = original.Position.X, Y = original.Position.Y } });
            }

            if (original.Position.FacingDirection != current.Position.FacingDirection)
            {
                changes.Add(ChangeAreas.General, "Facing", original.Position.FacingDirection.ToString(), current.Position.FacingDirection.ToString(), h => h with { Position = h.Position with { FacingDirection = original.Position.FacingDirection } });
            }

            if (original.Destination != current.Destination)
            {
                changes.Add(ChangeAreas.General, "Destination", original.Destination == MapPosition.None ? ChangeText.Empty : original.Destination.ToString(), current.Destination == MapPosition.None ? ChangeText.Empty : current.Destination.ToString(), h => h with { Destination = original.Destination });
            }

            if (original.SpellPoints != current.SpellPoints)
            {
                changes.Add(ChangeAreas.General, "Spell points", original.SpellPoints.ToString("N0"), current.SpellPoints.ToString("N0"), h => h with { SpellPoints = original.SpellPoints });
            }

            if (original.MovementPoints.Current != current.MovementPoints.Current)
            {
                changes.Add(ChangeAreas.General, "Movement points left", original.MovementPoints.Current.ToString("N0"), current.MovementPoints.Current.ToString("N0"), h => h with { MovementPoints = h.MovementPoints with { Current = original.MovementPoints.Current } });
            }

            if (original.HeroLevel.Experience != current.HeroLevel.Experience)
            {
                changes.Add(ChangeAreas.General, "Experience", original.HeroLevel.Experience.ToString("N0"), current.HeroLevel.Experience.ToString("N0"), h => h with { HeroLevel = h.HeroLevel with { Experience = original.HeroLevel.Experience } });
            }

            if (original.HeroLevel.Level != current.HeroLevel.Level)
            {
                changes.Add(ChangeAreas.General, "Level", original.HeroLevel.Level.ToString(), current.HeroLevel.Level.ToString(), h => h with { HeroLevel = h.HeroLevel with { Level = original.HeroLevel.Level } });
            }

            if (original.NextBattle.Morale != current.NextBattle.Morale)
            {
                changes.Add(ChangeAreas.General, "Morale, next battle", original.NextBattle.Morale.ToString("+0;-0;0"), current.NextBattle.Morale.ToString("+0;-0;0"), h => h with { NextBattle = h.NextBattle with { Morale = original.NextBattle.Morale } });
            }

            if (original.NextBattle.Luck != current.NextBattle.Luck)
            {
                changes.Add(ChangeAreas.General, "Luck, next battle", original.NextBattle.Luck.ToString("+0;-0;0"), current.NextBattle.Luck.ToString("+0;-0;0"), h => h with { NextBattle = h.NextBattle with { Luck = original.NextBattle.Luck } });
            }

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

            foreach (var spell in Spell.All)
            {
                var (had, has) = (original.Spells.Contains(spell), current.Spells.Contains(spell));
                if (had != has)
                {
                    changes.Add(ChangeAreas.Spells, spell.Name, had ? "known" : "not known", has ? "known" : "not known", h => h with { Spells = had ? h.Spells.With(spell) : h.Spells.Without(spell) });
                }
            }

            return changes.All;
        }

        private static void DiffPrimarySkills(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes) =>
            changes.AddPrimarySkills(original.PrimarySkills, current.PrimarySkills, h => h.PrimarySkills, (h, skills) => h with { PrimarySkills = skills });

        private static void DiffSecondarySkills(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
        {
            foreach (var skill in SecondarySkill.All)
            {
                var (before, after) = (original.SecondarySkills.GetSkillLevel(skill), current.SecondarySkills.GetSkillLevel(skill));
                if (before != after)
                {
                    changes.Add(ChangeAreas.SecondarySkills, skill.Name, LevelText(before), LevelText(after), h => h with { SecondarySkills = h.SecondarySkills.WithSkill(skill, before) });
                }
            }
        }

        private static string LevelText(SkillLevel level) => level == SkillLevel.None ? "none" : level.ToString();

        private static string SlotText(ArmySlot<Creature>? slot) => slot?.ToString() ?? ChangeText.Empty;
    }
}
