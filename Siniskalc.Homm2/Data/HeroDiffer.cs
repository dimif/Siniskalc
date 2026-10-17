namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            var changes = new List<PendingChange>();

            void Add(ChangeArea area, string what, string before, string after, Func<HeroModel, HeroModel> revert)
            {
                changes.Add(Homm2PendingChanges.ForHero(current.Name, heroIndex, new Change(area, what, before, after), revert));
            }

            if (original.Owner != current.Owner)
            {
                Add(ChangeAreas.General, "Owner", OwnerNames<Color>.Of(original.Owner), OwnerNames<Color>.Of(current.Owner), h => h with { Owner = original.Owner });
            }

            if (original.Position != current.Position)
            {
                Add(ChangeAreas.General, "Position", original.Position.ToString(), current.Position.ToString(), h => h with { Position = original.Position });
            }

            if (original.Facing != current.Facing)
            {
                Add(ChangeAreas.General, "Facing", original.Facing.ToString(), current.Facing.ToString(), h => h with { Facing = original.Facing });
            }

            if (original.Destination != current.Destination)
            {
                Add(ChangeAreas.General, "Destination", original.Destination == MapPosition.None ? ChangeText.Empty : original.Destination.ToString(), current.Destination == MapPosition.None ? ChangeText.Empty : current.Destination.ToString(), h => h with { Destination = original.Destination });
            }

            if (original.SpellPoints != current.SpellPoints)
            {
                Add(ChangeAreas.General, "Spell points", original.SpellPoints.ToString("N0"), current.SpellPoints.ToString("N0"), h => h with { SpellPoints = original.SpellPoints });
            }

            if (original.MovementLeft != current.MovementLeft)
            {
                Add(ChangeAreas.General, "Movement points left", original.MovementLeft.ToString("N0"), current.MovementLeft.ToString("N0"), h => h with { MovementLeft = original.MovementLeft });
            }

            if (original.Experience != current.Experience)
            {
                Add(ChangeAreas.General, "Experience", original.Experience.ToString("N0"), current.Experience.ToString("N0"), h => h with { Experience = original.Experience });
            }

            if (original.Level != current.Level)
            {
                Add(ChangeAreas.General, "Level", original.Level.ToString(), current.Level.ToString(), h => h with { Level = original.Level });
            }

            if (original.NextBattle.Morale != current.NextBattle.Morale)
            {
                Add(ChangeAreas.General, "Morale, next battle", original.NextBattle.Morale.ToString("+0;-0;0"), current.NextBattle.Morale.ToString("+0;-0;0"), h => h with { NextBattle = h.NextBattle with { Morale = original.NextBattle.Morale } });
            }

            if (original.NextBattle.Luck != current.NextBattle.Luck)
            {
                Add(ChangeAreas.General, "Luck, next battle", original.NextBattle.Luck.ToString("+0;-0;0"), current.NextBattle.Luck.ToString("+0;-0;0"), h => h with { NextBattle = h.NextBattle with { Luck = original.NextBattle.Luck } });
            }

            if (original.Effects != current.Effects)
            {
                Add(ChangeAreas.General, "Remembered effects", original.Effects.ToString(), current.Effects.ToString(), h => h with { Effects = original.Effects });
            }

            if (original.Visited != current.Visited)
            {
                Add(ChangeAreas.General, "Places visited", original.Visited.Count.ToString(), current.Visited.Count.ToString(), h => h with { Visited = original.Visited });
            }

            DiffPrimarySkills(original, current, Add);
            DiffSecondarySkills(original, current, Add);

            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                var (before, after, index) = (original.Army.Slots[slot], current.Army.Slots[slot], slot);
                if (before != after)
                {
                    Add(ChangeAreas.Creatures, $"Slot {slot + 1}", SlotText(before), SlotText(after), h => h with { Army = h.Army.WithSlot(index, before) });
                }
            }

            for (var slot = 0; slot < Constants.Hero.ArtifactSlotCount; slot++)
            {
                var (before, after, index) = (original.Artifacts.Slots[slot], current.Artifacts.Slots[slot], slot);
                if (before != after)
                {
                    Add(ChangeAreas.Artifacts, $"Slot {slot + 1}", before?.Name ?? ChangeText.Empty, after?.Name ?? ChangeText.Empty, h => h with { Artifacts = h.Artifacts.WithSlot(index, before) });
                }
            }

            foreach (var spell in Spell.All)
            {
                var (had, has) = (original.Spells.Contains(spell), current.Spells.Contains(spell));
                if (had != has)
                {
                    Add(ChangeAreas.Spells, spell.Name, had ? "known" : "not known", has ? "known" : "not known", h => h with { Spells = had ? h.Spells.With(spell) : h.Spells.Without(spell) });
                }
            }

            return changes;
        }

        private static void DiffPrimarySkills(HeroModel original, HeroModel current, Action<ChangeArea, string, string, string, Func<HeroModel, HeroModel>> add)
        {
            var before = original.PrimarySkills;
            var after = current.PrimarySkills;
            if (before.Attack != after.Attack)
            {
                add(ChangeAreas.PrimarySkills, "Attack", before.Attack.ToString(), after.Attack.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Attack = before.Attack } });
            }

            if (before.Defense != after.Defense)
            {
                add(ChangeAreas.PrimarySkills, "Defense", before.Defense.ToString(), after.Defense.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Defense = before.Defense } });
            }

            if (before.SpellPower != after.SpellPower)
            {
                add(ChangeAreas.PrimarySkills, "Spell power", before.SpellPower.ToString(), after.SpellPower.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { SpellPower = before.SpellPower } });
            }

            if (before.Knowledge != after.Knowledge)
            {
                add(ChangeAreas.PrimarySkills, "Knowledge", before.Knowledge.ToString(), after.Knowledge.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Knowledge = before.Knowledge } });
            }
        }

        private static void DiffSecondarySkills(HeroModel original, HeroModel current, Action<ChangeArea, string, string, string, Func<HeroModel, HeroModel>> add)
        {
            foreach (var skill in SecondarySkill.All)
            {
                var (before, after) = (original.SecondarySkills.LevelOf(skill), current.SecondarySkills.LevelOf(skill));
                if (before != after)
                {
                    add(ChangeAreas.SecondarySkills, skill.Name, LevelText(before), LevelText(after), h => h with { SecondarySkills = h.SecondarySkills.With(skill, before) });
                }
            }
        }

        private static string LevelText(SkillLevel level) => level == SkillLevel.None ? "none" : level.ToString();

        private static string SlotText(ArmySlot<Creature>? slot) => slot?.ToString() ?? ChangeText.Empty;
    }
}
