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
    /// Lists what an edit changed in a hero. What the game works out itself (the movement of a full day) and the place the hero stands on are
    /// not listed, but they are saved.
    /// </summary>
    public sealed class Homm2HeroDiffer : IHomm2HeroDiffer
    {
        public const string Empty = "empty";

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int heroIndex, Homm2HeroModel original, Homm2HeroModel current)
        {
            var changes = new List<PendingChange>();

            void Add(ChangeArea area, string what, string before, string after, Func<Homm2HeroModel, Homm2HeroModel> revert)
            {
                changes.Add(Homm2PendingChanges.ForHero(current.Name, heroIndex, new Change(area, what, before, after), revert));
            }

            if (original.Owner != current.Owner)
            {
                Add(Homm2ChangeAreas.General, "Owner", OwnerText(original.Owner), OwnerText(current.Owner), h => h with { Owner = original.Owner });
            }

            if (original.SpellPoints != current.SpellPoints)
            {
                Add(Homm2ChangeAreas.General, "Spell points", original.SpellPoints.ToString("N0"), current.SpellPoints.ToString("N0"), h => h with { SpellPoints = original.SpellPoints });
            }

            if (original.MovementLeft != current.MovementLeft)
            {
                Add(Homm2ChangeAreas.General, "Movement points left", original.MovementLeft.ToString("N0"), current.MovementLeft.ToString("N0"), h => h with { MovementLeft = original.MovementLeft });
            }

            if (original.Experience != current.Experience)
            {
                Add(Homm2ChangeAreas.General, "Experience", original.Experience.ToString("N0"), current.Experience.ToString("N0"), h => h with { Experience = original.Experience });
            }

            if (original.Level != current.Level)
            {
                Add(Homm2ChangeAreas.General, "Level", original.Level.ToString(), current.Level.ToString(), h => h with { Level = original.Level });
            }

            DiffPrimarySkills(original, current, Add);
            DiffSecondarySkills(original, current, Add);

            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                var (before, after, index) = (original.Army.Slots[slot], current.Army.Slots[slot], slot);
                if (before != after)
                {
                    Add(Homm2ChangeAreas.Army, $"Slot {slot + 1}", SlotText(before), SlotText(after), h => h with { Army = h.Army.WithSlot(index, before) });
                }
            }

            for (var slot = 0; slot < Constants.Hero.ArtifactSlotCount; slot++)
            {
                var (before, after, index) = (original.Artifacts.Slots[slot], current.Artifacts.Slots[slot], slot);
                if (before != after)
                {
                    Add(Homm2ChangeAreas.Artifacts, $"Slot {slot + 1}", before?.Name ?? Empty, after?.Name ?? Empty, h => h with { Artifacts = h.Artifacts.WithSlot(index, before) });
                }
            }

            foreach (var spell in Homm2Spell.All)
            {
                var (had, has) = (original.Spells.Contains(spell), current.Spells.Contains(spell));
                if (had != has)
                {
                    Add(Homm2ChangeAreas.Spells, spell.Name, had ? "known" : "not known", has ? "known" : "not known", h => h with { Spells = had ? h.Spells.With(spell) : h.Spells.Without(spell) });
                }
            }

            return changes;
        }

        private static void DiffPrimarySkills(Homm2HeroModel original, Homm2HeroModel current, Action<ChangeArea, string, string, string, Func<Homm2HeroModel, Homm2HeroModel>> add)
        {
            var before = original.PrimarySkills;
            var after = current.PrimarySkills;
            if (before.Attack != after.Attack)
            {
                add(Homm2ChangeAreas.Skills, "Attack", before.Attack.ToString(), after.Attack.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Attack = before.Attack } });
            }

            if (before.Defense != after.Defense)
            {
                add(Homm2ChangeAreas.Skills, "Defense", before.Defense.ToString(), after.Defense.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Defense = before.Defense } });
            }

            if (before.SpellPower != after.SpellPower)
            {
                add(Homm2ChangeAreas.Skills, "Spell power", before.SpellPower.ToString(), after.SpellPower.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { SpellPower = before.SpellPower } });
            }

            if (before.Knowledge != after.Knowledge)
            {
                add(Homm2ChangeAreas.Skills, "Knowledge", before.Knowledge.ToString(), after.Knowledge.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Knowledge = before.Knowledge } });
            }
        }

        private static void DiffSecondarySkills(Homm2HeroModel original, Homm2HeroModel current, Action<ChangeArea, string, string, string, Func<Homm2HeroModel, Homm2HeroModel>> add)
        {
            foreach (var skill in Homm2SecondarySkill.All)
            {
                var (before, after) = (original.SecondarySkills.LevelOf(skill), current.SecondarySkills.LevelOf(skill));
                if (before != after)
                {
                    add(Homm2ChangeAreas.Skills, skill.Name, LevelText(before), LevelText(after), h => h with { SecondarySkills = h.SecondarySkills.With(skill, before) });
                }
            }
        }

        private static string OwnerText(Homm2Color? owner) => owner?.ToString() ?? "nobody";

        private static string LevelText(Homm2SkillLevel level) => level == Homm2SkillLevel.None ? "none" : level.ToString();

        private static string SlotText(Homm2ArmySlot? slot) => slot?.ToString() ?? Empty;
    }
}
