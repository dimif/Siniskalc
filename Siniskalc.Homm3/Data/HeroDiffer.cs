namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds the differences between two versions of the same hero. Used for the pending changes list.
    /// </summary>
    public sealed class HeroDiffer : IDiffer<HeroModel>
    {
        /// <summary>
        /// Whether a place on the map says its level: the map of HoMM3 may have two.
        /// </summary>
        private const bool HasLevels = true;

        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public HeroDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int heroIndex, HeroModel original, HeroModel current)
        {
            // The owner of a hero is also in the lists of heroes of the players and in the table of hero owners: putting it back puts those back
            // too (PlayerListsReconciler).
            var changes = new ChangeSink<HeroModel>((change, revert) => this.pendingChanges.ForHero(current.Name, heroIndex, change, revert));
            changes.AddOwner(original.Owner, current.Owner, (h, owner) => h with { Owner = owner });

            DiffGeneral(original, current, changes);
            DiffPrimarySkills(original, current, changes);
            DiffSecondarySkills(original, current, changes);
            DiffCreatures(original, current, changes);
            DiffWarMachines(original, current, changes);
            DiffEquipped(original, current, changes);
            DiffBackpack(original, current, changes);
            DiffSpells(original, current, changes);
            DiffPlaceVisits(original, current, changes);

            return changes.All;
        }

        private static void DiffPlaceVisits(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
        {
            foreach (var kind in VisitedPlaceKind.All.Where(k => original.PlaceVisits.Of(k) != current.PlaceVisits.Of(k)))
            {
                changes.Add(
                    ChangeAreas.General,
                    $"Visited: {kind.Name.ToLowerInvariant()}",
                    System.Numerics.BitOperations.PopCount(original.PlaceVisits.Of(kind)).ToString(),
                    System.Numerics.BitOperations.PopCount(current.PlaceVisits.Of(kind)).ToString(),
                    h => h with { PlaceVisits = h.PlaceVisits.With(kind, original.PlaceVisits.Of(kind)) });
            }
        }

        private static void DiffGeneral(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
        {
            if (original.HeroClass != current.HeroClass)
            {
                changes.Add(ChangeAreas.General, "Class", original.HeroClass.Name, current.HeroClass.Name, h => h with { HeroClass = original.HeroClass });
            }

            if (original.Gender != current.Gender)
            {
                changes.Add(ChangeAreas.General, "Gender", FormatGender(original.Gender), FormatGender(current.Gender), h => h with { Gender = original.Gender });
            }

            if (original.MovedToday != current.MovedToday)
            {
                changes.Add(ChangeAreas.General, "Moved today", ChangeText.YesNo(original.MovedToday), ChangeText.YesNo(current.MovedToday), h => h with { MovedToday = original.MovedToday });
            }

            if (original.IsAsleep != current.IsAsleep)
            {
                changes.Add(ChangeAreas.General, "Asleep", ChangeText.YesNo(original.IsAsleep), ChangeText.YesNo(current.IsAsleep), h => h with { IsAsleep = original.IsAsleep });
            }

            foreach (var spell in System.Enum.GetValues<ActiveAdventureSpells>().Where(s => s != ActiveAdventureSpells.None))
            {
                if (original.ActiveSpells.HasFlag(spell) != current.ActiveSpells.HasFlag(spell))
                {
                    changes.Add(
                        ChangeAreas.Spells,
                        $"{spell} cast today",
                        ChangeText.YesNo(original.ActiveSpells.HasFlag(spell)),
                        ChangeText.YesNo(current.ActiveSpells.HasFlag(spell)),
                        h => h with { ActiveSpells = (h.ActiveSpells & ~spell) | (original.ActiveSpells & spell) });
                }
            }

            // What visited buildings gave until the next battle: the bytes of morale and luck and the bit of each building, each put back on its own.
            if (original.MoraleBonus != current.MoraleBonus)
            {
                changes.Add(ChangeAreas.General, "Morale, next battle", Signed(original.MoraleBonus), Signed(current.MoraleBonus), h => h with { MoraleBonus = original.MoraleBonus });
            }

            if (original.LuckBonus != current.LuckBonus)
            {
                changes.Add(ChangeAreas.General, "Luck, next battle", Signed(original.LuckBonus), Signed(current.LuckBonus), h => h with { LuckBonus = original.LuckBonus });
            }

            foreach (var effect in System.Enum.GetValues<VisitedBuildingEffects>().Where(e => e != VisitedBuildingEffects.None))
            {
                if (original.VisitEffects.HasFlag(effect) != current.VisitEffects.HasFlag(effect))
                {
                    changes.Add(
                        ChangeAreas.General,
                        $"Visited building: {VisitedBuildingNames.Of(effect)}",
                        ChangeText.YesNo(original.VisitEffects.HasFlag(effect)),
                        ChangeText.YesNo(current.VisitEffects.HasFlag(effect)),
                        h => h with { VisitEffects = (h.VisitEffects & ~effect) | (original.VisitEffects & effect) });
                }
            }

            changes.AddHeroLevel(original.HeroLevel, current.HeroLevel, (h, level) => h with { HeroLevel = level });

            changes.AddSpellPoints(original.SpellPoints, current.SpellPoints, (h, points) => h with { SpellPoints = points });
            changes.AddMovement(original.MovementPoints, current.MovementPoints, (h, movement) => h with { MovementPoints = movement });
            changes.AddPosition(original.Position, current.Position, HasLevels, (h, position) => h with { Position = position });
            changes.AddDestination(original.Destination, current.Destination, HasLevels, (h, destination) => h with { Destination = destination });
        }

        private static void DiffPrimarySkills(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes) =>
            changes.AddPrimarySkills(original.PrimarySkills, current.PrimarySkills, h => h.PrimarySkills, (h, skills) => h with { PrimarySkills = skills });

        private static void DiffSecondarySkills(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
        {
            // Skills are slots in an order, so they are shown, and reverted, as one list.
            var before = FormatSecondarySkills(original.SecondarySkills);
            var after = FormatSecondarySkills(current.SecondarySkills);

            if (before != after)
            {
                changes.Add(ChangeAreas.SecondarySkills, "Skills", before, after, h => h with { SecondarySkills = original.SecondarySkills });
            }
        }

        private static void DiffCreatures(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes) =>
            changes.AddArmy(ChangeAreas.Creatures, original.Army, current.Army, h => h.Army, (h, army) => h with { Army = army });

        private static void DiffWarMachines(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
        {
            if (original.WarMachines.OrderBy(w => w.Code).SequenceEqual(current.WarMachines.OrderBy(w => w.Code)))
            {
                return;
            }

            changes.Add(
                ChangeAreas.WarMachines,
                "War machines",
                FormatList(original.WarMachines.Select(w => w.Name)),
                FormatList(current.WarMachines.Select(w => w.Name)),
                h => h with { WarMachines = original.WarMachines });
        }

        private static void DiffEquipped(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
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
                changes.Add(
                    ChangeAreas.Artifacts,
                    slotName.ToString(),
                    NameOfEquipped(before, spellBefore),
                    NameOfEquipped(after, spellAfter),
                    h => h.WithEquippedArtifacts(RestoreEquipped(h.EquippedArtifacts, slotName, before, spellBefore)));
            }
        }

        private static void DiffBackpack(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
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

                changes.Add(
                    ChangeAreas.Backpack,
                    $"Slot {index + 1}",
                    before?.Name ?? ChangeText.Empty,
                    after?.Name ?? ChangeText.Empty,
                    h => h with { Backpack = h.Backpack.WithSlot(index, before) });
            }
        }

        private static void DiffSpells(HeroModel original, HeroModel current, ChangeSink<HeroModel> changes)
        {
            if (original.SpellBook.HasSpellBook != current.SpellBook.HasSpellBook)
            {
                changes.Add(ChangeAreas.Spells, "Spell book", ChangeText.YesNo(original.SpellBook.HasSpellBook), ChangeText.YesNo(current.SpellBook.HasSpellBook), h => h with { SpellBook = h.SpellBook.WithHasSpellBook(original.SpellBook.HasSpellBook) });
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
            changes.Add(ChangeAreas.Spells, "Learned spells", before, after, h => h with { SpellBook = h.SpellBook.WithLearned(added, false).WithLearned(removed, true) });
        }

        private static EquippedArtifacts RestoreEquipped(EquippedArtifacts equipped, ArtifactSlots slot, Artifact? artifact, Spell? spell)
        {
            return artifact == null ? equipped.WithUnequipped(slot) : equipped.WithEquipped(slot, artifact, spell);
        }

        private static string NameOfEquipped(Artifact? artifact, Spell? spell)
        {
            return spell != null ? SpellScroll.NameOf(spell) : artifact?.Name ?? ChangeText.Empty;
        }

        private static string FormatSecondarySkills(SecondarySkills skills)
        {
            var parts = Enumerable.Range(0, skills.SlotCount)
                .Select(i => skills.GetSkillInSlot(i))
                .OfType<SecondarySkill>()
                .Select(skill => $"{skill.Name} ({(skills.GetSkillLevel(skill) == SkillLevel.None ? "no level" : skills.GetSkillLevel(skill).Name)})")
                .ToList();

            // Skills without a slot are applied by the game, but not shown on the hero screen.
            var extras = skills.ExtraSkills.OrderBy(s => s.Code).Select(skill => $"{skill.Name} ({skills.GetSkillLevel(skill)})").ToList();
            if (extras.Count > 0)
            {
                parts.Add($"not shown: {string.Join(", ", extras)}");
            }

            return FormatList(parts);
        }

        private static string FormatList(IEnumerable<string> items)
        {
            var list = items.ToList();
            return list.Count == 0 ? "none" : string.Join(", ", list);
        }

        private static string FormatGender(HeroGender? gender) => gender?.ToString() ?? "unknown";

        private static string Signed(int value) => value.ToString("+0;-0;0", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// The pending changes of one hero as they are found.
        /// </summary>
    }
}
