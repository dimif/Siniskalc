namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// Finds the differences between two versions of the same hero. Used for the pending changes list.
    /// </summary>
    public sealed class HeroDiffer : IHeroDiffer
    {
        public const string Empty = "empty";

        public IReadOnlyList<HeroChange> Diff(HeroModel original, HeroModel current)
        {
            var changes = new List<HeroChange>();

            DiffGeneral(original, current, changes);
            DiffPrimarySkills(original, current, changes);
            DiffSecondarySkills(original, current, changes);
            DiffCreatures(original, current, changes);
            DiffWarMachines(original, current, changes);
            DiffEquipped(original, current, changes);
            DiffBackpack(original, current, changes);
            DiffSpells(original, current, changes);

            return changes;
        }

        private static void DiffGeneral(HeroModel original, HeroModel current, List<HeroChange> changes)
        {
            if (original.HeroClass != current.HeroClass)
            {
                changes.Add(new HeroChange(ChangeAreas.General, "Class", original.HeroClass.Name, current.HeroClass.Name, h => h with { HeroClass = original.HeroClass }));
            }

            if (original.Gender != current.Gender)
            {
                changes.Add(new HeroChange(ChangeAreas.General, "Gender", FormatGender(original.Gender), FormatGender(current.Gender), h => h with { Gender = original.Gender }));
            }

            if (original.MovedToday != current.MovedToday)
            {
                changes.Add(new HeroChange(ChangeAreas.General, "Moved today", ChangeText.YesNo(original.MovedToday), ChangeText.YesNo(current.MovedToday), h => h with { MovedToday = original.MovedToday }));
            }

            if (original.IsAsleep != current.IsAsleep)
            {
                changes.Add(new HeroChange(ChangeAreas.General, "Asleep", ChangeText.YesNo(original.IsAsleep), ChangeText.YesNo(current.IsAsleep), h => h with { IsAsleep = original.IsAsleep }));
            }

            foreach (var spell in System.Enum.GetValues<ActiveAdventureSpells>().Where(s => s != ActiveAdventureSpells.None))
            {
                if (original.ActiveSpells.HasFlag(spell) != current.ActiveSpells.HasFlag(spell))
                {
                    changes.Add(new HeroChange(
                        ChangeAreas.Spells,
                        $"{spell} cast today",
                        ChangeText.YesNo(original.ActiveSpells.HasFlag(spell)),
                        ChangeText.YesNo(current.ActiveSpells.HasFlag(spell)),
                        h => h with { ActiveSpells = (h.ActiveSpells & ~spell) | (original.ActiveSpells & spell) }));
                }
            }

            if (!original.HeroLevel.Equals(current.HeroLevel))
            {
                changes.Add(new HeroChange(
                    ChangeAreas.General,
                    "Experience",
                    $"{original.HeroLevel.Experience:N0} (level {original.HeroLevel.Level})",
                    $"{current.HeroLevel.Experience:N0} (level {current.HeroLevel.Level})",
                    h => h with { HeroLevel = original.HeroLevel }));
            }

            if (original.SpellPoints != current.SpellPoints)
            {
                changes.Add(new HeroChange(ChangeAreas.General, "Spell points", original.SpellPoints.ToString(), current.SpellPoints.ToString(), h => h with { SpellPoints = original.SpellPoints }));
            }

            if (!original.MovementPoints.Equals(current.MovementPoints))
            {
                changes.Add(new HeroChange(
                    ChangeAreas.General,
                    "Movement points",
                    $"{original.MovementPoints.CurrentMovementPoints} / {original.MovementPoints.MaxMovementPoints}",
                    $"{current.MovementPoints.CurrentMovementPoints} / {current.MovementPoints.MaxMovementPoints}",
                    h => h with { MovementPoints = original.MovementPoints }));
            }

            if (original.Position != current.Position)
            {
                changes.Add(new HeroChange(ChangeAreas.General, "Position", FormatPosition(original.Position), FormatPosition(current.Position), h => h with { Position = original.Position }));
            }

            if (original.MarkerPosition != current.MarkerPosition)
            {
                changes.Add(new HeroChange(ChangeAreas.General, "Marker", FormatPosition(original.MarkerPosition), FormatPosition(current.MarkerPosition), h => h with { MarkerPosition = original.MarkerPosition }));
            }
        }

        private static void DiffPrimarySkills(HeroModel original, HeroModel current, List<HeroChange> changes)
        {
            var before = original.PrimarySkills;
            var after = current.PrimarySkills;

            if (before.Atk != after.Atk)
            {
                changes.Add(new HeroChange(ChangeAreas.PrimarySkills, "Attack", before.Atk.ToString(), after.Atk.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Atk = before.Atk } }));
            }

            if (before.Def != after.Def)
            {
                changes.Add(new HeroChange(ChangeAreas.PrimarySkills, "Defense", before.Def.ToString(), after.Def.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Def = before.Def } }));
            }

            if (before.Pow != after.Pow)
            {
                changes.Add(new HeroChange(ChangeAreas.PrimarySkills, "Spell power", before.Pow.ToString(), after.Pow.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Pow = before.Pow } }));
            }

            if (before.Kno != after.Kno)
            {
                changes.Add(new HeroChange(ChangeAreas.PrimarySkills, "Knowledge", before.Kno.ToString(), after.Kno.ToString(), h => h with { PrimarySkills = h.PrimarySkills with { Kno = before.Kno } }));
            }
        }

        private static void DiffSecondarySkills(HeroModel original, HeroModel current, List<HeroChange> changes)
        {
            // Skills are slots in an order, so they are shown, and reverted, as one list.
            var before = FormatSecondarySkills(original.SecondarySkills);
            var after = FormatSecondarySkills(current.SecondarySkills);

            if (before != after)
            {
                changes.Add(new HeroChange(ChangeAreas.SecondarySkills, "Skills", before, after, h => h with { SecondarySkills = original.SecondarySkills }));
            }
        }

        private static void DiffCreatures(HeroModel original, HeroModel current, List<HeroChange> changes)
        {
            for (var slot = 1; slot <= Constants.CreatureSlotCount; slot++)
            {
                var slotNumber = slot;
                var before = original.Army.FindSlot(slotNumber);
                var after = current.Army.FindSlot(slotNumber);

                if (before == after)
                {
                    continue;
                }

                changes.Add(new HeroChange(
                    ChangeAreas.Creatures,
                    $"Slot {slotNumber}",
                    FormatCreatureSlot(before),
                    FormatCreatureSlot(after),
                    h => h with { Army = RestoreCreatureSlot(h.Army, slotNumber, before) }));
            }
        }

        private static void DiffWarMachines(HeroModel original, HeroModel current, List<HeroChange> changes)
        {
            if (original.WarMachines.OrderBy(w => w.Code).SequenceEqual(current.WarMachines.OrderBy(w => w.Code)))
            {
                return;
            }

            changes.Add(new HeroChange(
                ChangeAreas.WarMachines,
                "War machines",
                FormatList(original.WarMachines.Select(w => w.Name)),
                FormatList(current.WarMachines.Select(w => w.Name)),
                h => h with { WarMachines = original.WarMachines }));
        }

        private static void DiffEquipped(HeroModel original, HeroModel current, List<HeroChange> changes)
        {
            foreach (var slot in Enum.GetValues<ArtifactSlots>().Where(EquippedArtifacts.IsSingleSlot))
            {
                var before = original.EquippedArtifacts.GetArtifact(slot);
                var after = current.EquippedArtifacts.GetArtifact(slot);
                var spellBefore = original.EquippedArtifacts.GetScrollSpell(slot);
                var spellAfter = current.EquippedArtifacts.GetScrollSpell(slot);

                if (before == after && spellBefore == spellAfter)
                {
                    continue;
                }

                var slotName = slot;
                changes.Add(new HeroChange(
                    ChangeAreas.Artifacts,
                    slotName.ToString(),
                    NameOfEquipped(before, spellBefore),
                    NameOfEquipped(after, spellAfter),
                    h => h.WithEquippedArtifacts(RestoreEquipped(h.EquippedArtifacts, slotName, before, spellBefore))));
            }
        }

        private static void DiffBackpack(HeroModel original, HeroModel current, List<HeroChange> changes)
        {
            for (var i = 0; i < Math.Min(original.Backpack.All.Count, current.Backpack.All.Count); i++)
            {
                var index = i;
                var before = original.Backpack.All[index]?.Art;
                var after = current.Backpack.All[index]?.Art;

                if (before == after)
                {
                    continue;
                }

                changes.Add(new HeroChange(
                    ChangeAreas.Backpack,
                    $"Slot {index + 1}",
                    before?.Name ?? Empty,
                    after?.Name ?? Empty,
                    h => h with { Backpack = h.Backpack.WithSlot(index, before) }));
            }
        }

        private static void DiffSpells(HeroModel original, HeroModel current, List<HeroChange> changes)
        {
            if (original.SpellBook.HasSpellBook != current.SpellBook.HasSpellBook)
            {
                changes.Add(new HeroChange(ChangeAreas.Spells, "Spell book", ChangeText.YesNo(original.SpellBook.HasSpellBook), ChangeText.YesNo(current.SpellBook.HasSpellBook), h => h with { SpellBook = h.SpellBook.WithHasSpellBook(original.SpellBook.HasSpellBook) }));
            }

            // Only learned spells are edited; the available ones come from artifacts.
            var added = Spell.All.Where(s => current.SpellBook.IsLearned(s) && !original.SpellBook.IsLearned(s)).ToList();
            var removed = Spell.All.Where(s => original.SpellBook.IsLearned(s) && !current.SpellBook.IsLearned(s)).ToList();

            if (added.Count == 0 && removed.Count == 0)
            {
                return;
            }

            var before = removed.Count == 0 ? "nothing forgotten" : $"forgotten: {FormatList(removed.Select(s => s.Name))}";
            var after = added.Count == 0 ? "nothing learned" : $"learned: {FormatList(added.Select(s => s.Name))}";
            changes.Add(new HeroChange(ChangeAreas.Spells, "Learned spells", before, after, h => h with { SpellBook = h.SpellBook.WithLearned(added, false).WithLearned(removed, true) }));
        }

        private static Army RestoreCreatureSlot(Army creatures, int slotNumber, ArmySlot? slot)
        {
            if (slot == null)
            {
                return creatures.WithoutSlot(slotNumber);
            }

            var creature = Creature.FromByte(slot.CreatureCode);
            return creature == null ? creatures : creatures.WithSlot(slotNumber, creature, slot.CreatureCount);
        }

        private static EquippedArtifacts RestoreEquipped(EquippedArtifacts equipped, ArtifactSlots slot, Artifact? artifact, Spell? spell)
        {
            return artifact == null ? equipped.WithUnequipped(slot) : equipped.WithEquipped(slot, artifact, spell);
        }

        private static string NameOfEquipped(Artifact? artifact, Spell? spell)
        {
            return spell != null ? SpellScroll.NameOf(spell) : artifact?.Name ?? Empty;
        }

        private static string FormatCreatureSlot(ArmySlot? slot)
        {
            if (slot == null)
            {
                return Empty;
            }

            return $"{slot.CreatureCount:N0} {Creature.NameOf(slot.CreatureCode)}";
        }

        private static string FormatSecondarySkills(SecondarySkills skills)
        {
            var parts = Enumerable.Range(0, skills.SlotCount)
                .Select(i => skills.GetSkillInSlot(i))
                .OfType<SecondarySkill>()
                .Select(skill => $"{skill.Name} ({skills.GetSkillLevel(skill)?.ToString() ?? "no level"})")
                .ToList();

            // Skills without a slot are applied by the game, but not shown on the hero screen.
            var extras = skills.ExtraSkills.OrderBy(s => s.Code).Select(skill => $"{skill.Name} ({skills.GetSkillLevel(skill)})").ToList();
            if (extras.Count > 0)
            {
                parts.Add($"not shown: {string.Join(", ", extras)}");
            }

            return FormatList(parts);
        }

        private static string FormatPosition(Position position)
        {
            return position is HeroPosition heroPosition
                ? $"{position.X}, {position.Y}, {position.Z} facing {heroPosition.FacingDirection}"
                : $"{position.X}, {position.Y}, {position.Z}";
        }

        private static string FormatList(IEnumerable<string> items)
        {
            var list = items.ToList();
            return list.Count == 0 ? "none" : string.Join(", ", list);
        }

        private static string FormatGender(HeroGender? gender) => gender?.ToString() ?? "unknown";
    }
}
