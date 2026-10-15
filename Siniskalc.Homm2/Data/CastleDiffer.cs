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
    /// Lists what an edit changed in a castle.
    /// </summary>
    public sealed class CastleDiffer : ICastleDiffer
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int castleIndex, CastleModel original, CastleModel current)
        {
            var changes = new List<PendingChange>();

            void Add(ChangeArea area, string what, string before, string after, Func<CastleModel, CastleModel> revert)
            {
                changes.Add(Homm2PendingChanges.ForCastle(current.Name, castleIndex, new Change(area, what, before, after), revert));
            }

            if (original.Owner != current.Owner)
            {
                Add(ChangeAreas.General, "Owner", OwnerNames.Of(original.Owner), OwnerNames.Of(current.Owner), c => c with { Owner = original.Owner });
            }

            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                var (before, after, index) = (original.Garrison.Slots[slot], current.Garrison.Slots[slot], slot);
                if (before != after)
                {
                    Add(ChangeAreas.Garrison, $"Slot {slot + 1}", before?.ToString() ?? DifferText.Empty, after?.ToString() ?? DifferText.Empty, c => c with { Garrison = c.Garrison.WithSlot(index, before) });
                }
            }

            foreach (var building in Enum.GetValues<TownBuildings>().Where(b => b != TownBuildings.None && b != TownBuildings.MageGuild))
            {
                var (had, has) = (original.Town.HasFlag(building), current.Town.HasFlag(building));
                if (had != has)
                {
                    var which = building;
                    Add(ChangeAreas.Buildings, which.ToString(), had ? "built" : "not built", has ? "built" : "not built", c => c with { Town = had ? c.Town | which : c.Town & ~which });
                }
            }

            // A dwelling, the creatures that wait in it (wherever the savegame keeps them for its level) and the Mage Guild (its level, its
            // building and the spells each level shows) change together, so each is one change that is reverted as a whole.
            for (var dwelling = 1; dwelling <= Constants.Castle.DwellingCount; dwelling++)
            {
                var number = dwelling;
                if (!current.HasSameDwelling(original, number))
                {
                    Add(ChangeAreas.Dwellings, $"Dwelling {number}", DwellingText(original, number), DwellingText(current, number), c => c.WithDwellingFrom(original, number));
                }
            }

            if (!current.HasSameMageGuild(original))
            {
                Add(ChangeAreas.MageGuild, "Mage Guild", MageGuildText(original), MageGuildText(current), c => c.WithMageGuildFrom(original));
            }

            for (var slot = 0; slot < original.GuildSpells.Count && slot < current.GuildSpells.Count; slot++)
            {
                var (before, after, index) = (original.GuildSpells[slot], current.GuildSpells[slot], slot);
                if (before != after)
                {
                    var level = (slot / Constants.Castle.MageGuildSlotsPerLevel) + 1;
                    Add(ChangeAreas.MageGuild, $"Level {level}, spell {(slot % Constants.Castle.MageGuildSlotsPerLevel) + 1}", SpellText(before), SpellText(after), c => c with { GuildSpells = c.GuildSpells.With(index, before) });
                }
            }

            return changes;
        }

        private static string DwellingText(CastleModel castle, int dwelling) =>
            castle.DwellingLevel(dwelling) switch { 0 => "not built", 1 => "built", 2 => "upgraded", _ => "upgraded twice" }
            + (castle.DwellingLevel(dwelling) > 0 ? $", {castle.Waiting(dwelling):N0} waiting" : string.Empty);

        private static string MageGuildText(CastleModel castle) =>
            castle.MageGuildLevel == 0 ? "none" : $"level {castle.MageGuildLevel}, spells shown {string.Join("/", castle.GuildSpellCounts)}";

        private static string SpellText(int number) => number == 0 ? "none" : Spell.NameOf((byte)number);
    }
}
