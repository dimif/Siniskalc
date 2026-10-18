namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what an edit changed in a hero. What the game works out itself (the movement of a full day) and the place the hero stands on are
    /// not listed, but they are saved.
    /// </summary>
    public sealed class HeroDiffer : IHeroDiffer
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int heroIndex, HeroModel original, HeroModel current)
        {

            var changes = new ChangeSink<HeroModel>((change, revert) => Homm2PendingChanges.ForHero(current.Name, heroIndex, change, revert));

            changes.AddOwner(original.Owner, current.Owner, (h, owner) => h with { Owner = owner });

            if (original.Position != current.Position)
            {
                changes.Add(ChangeAreas.General, "Position", original.Position.ToString(), current.Position.ToString(), h => h with { Position = original.Position });
            }

            if (original.Facing != current.Facing)
            {
                changes.Add(ChangeAreas.General, "Facing", original.Facing.ToString(), current.Facing.ToString(), h => h with { Facing = original.Facing });
            }

            if (original.Destination != current.Destination)
            {
                changes.Add(ChangeAreas.General, "Destination", original.Destination == MapPosition.None ? ChangeText.Empty : original.Destination.ToString(), current.Destination == MapPosition.None ? ChangeText.Empty : current.Destination.ToString(), h => h with { Destination = original.Destination });
            }

            if (original.SpellPoints != current.SpellPoints)
            {
                changes.Add(ChangeAreas.General, "Spell points", original.SpellPoints.ToString("N0"), current.SpellPoints.ToString("N0"), h => h with { SpellPoints = original.SpellPoints });
            }

            if (original.MovementLeft != current.MovementLeft)
            {
                changes.Add(ChangeAreas.General, "Movement points left", original.MovementLeft.ToString("N0"), current.MovementLeft.ToString("N0"), h => h with { MovementLeft = original.MovementLeft });
            }

            if (original.Experience != current.Experience)
            {
                changes.Add(ChangeAreas.General, "Experience", original.Experience.ToString("N0"), current.Experience.ToString("N0"), h => h with { Experience = original.Experience });
            }

            if (original.Level != current.Level)
            {
                changes.Add(ChangeAreas.General, "Level", original.Level.ToString(), current.Level.ToString(), h => h with { Level = original.Level });
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

        private static void DiffPrimarySkills(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
        {
            var before = original.PrimarySkills;
            var after = current.PrimarySkills;
            if (before.Attack != after.Attack)
            {
                changes.Add(ChangeAreas.PrimarySkills, "Attack", before.Attack.ToString(), after.Attack.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Attack = before.Attack } });
            }

            if (before.Defense != after.Defense)
            {
                changes.Add(ChangeAreas.PrimarySkills, "Defense", before.Defense.ToString(), after.Defense.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Defense = before.Defense } });
            }

            if (before.SpellPower != after.SpellPower)
            {
                changes.Add(ChangeAreas.PrimarySkills, "Spell power", before.SpellPower.ToString(), after.SpellPower.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { SpellPower = before.SpellPower } });
            }

            if (before.Knowledge != after.Knowledge)
            {
                changes.Add(ChangeAreas.PrimarySkills, "Knowledge", before.Knowledge.ToString(), after.Knowledge.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Knowledge = before.Knowledge } });
            }
        }

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
