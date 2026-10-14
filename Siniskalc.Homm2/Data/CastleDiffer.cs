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
        public const string Empty = "empty";

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
                Add(ChangeAreas.General, "Owner", OwnerText(original.Owner), OwnerText(current.Owner), c => c with { Owner = original.Owner });
            }

            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                var (before, after, index) = (original.Garrison.Slots[slot], current.Garrison.Slots[slot], slot);
                if (before != after)
                {
                    Add(ChangeAreas.Garrison, $"Slot {slot + 1}", before?.ToString() ?? Empty, after?.ToString() ?? Empty, c => c with { Garrison = c.Garrison.WithSlot(index, before) });
                }
            }

            foreach (var building in Enum.GetValues<TownBuildings>().Where(b => b != TownBuildings.None))
            {
                var (had, has) = (original.Town.HasFlag(building), current.Town.HasFlag(building));
                if (had != has)
                {
                    var which = building;
                    Add(ChangeAreas.Buildings, which.ToString(), had ? "built" : "not built", has ? "built" : "not built", c => c with { Town = had ? c.Town | which : c.Town & ~which });
                }
            }

            for (var dwelling = 1; dwelling <= Constants.Castle.DwellingCount; dwelling++)
            {
                var (before, after, number) = (original.DwellingLevel(dwelling), current.DwellingLevel(dwelling), dwelling);
                if (before != after)
                {
                    Add(ChangeAreas.Dwellings, $"Dwelling {dwelling}", DwellingText(before), DwellingText(after), c => c.WithDwellingLevel(number, Math.Min(before, CastleModel.MaxDwellingLevel(c.Faction, number))));
                }
            }

            if (original.MageGuildLevel != current.MageGuildLevel)
            {
                var before = original.MageGuildLevel;
                Add(ChangeAreas.MageGuild, "Level", before == 0 ? "none" : before.ToString(), current.MageGuildLevel == 0 ? "none" : current.MageGuildLevel.ToString(), c => c with { Dwellings = (c.Dwellings & ~Dwellings.MageGuildLevelMask) | (Dwellings)((uint)before << 16) });
            }

            for (var level = 0; level < Constants.Castle.MageGuildLevels; level++)
            {
                var (before, after, index) = (original.GuildSpellCounts[level], current.GuildSpellCounts[level], level);
                if (before != after)
                {
                    Add(ChangeAreas.MageGuild, $"Spells shown at level {level + 1}", before.ToString(), after.ToString(), c => c with { GuildSpellCounts = c.GuildSpellCounts.With(index, before) });
                }
            }

            for (var dwelling = 0; dwelling < Constants.Castle.DwellingCount; dwelling++)
            {
                var (before, after, index) = (original.Creatures[dwelling], current.Creatures[dwelling], dwelling);
                if (before != after)
                {
                    Add(ChangeAreas.Dwellings, $"Creatures in dwelling {dwelling + 1}", before.ToString("N0"), after.ToString("N0"), c => c with { Creatures = c.Creatures.With(index, before) });
                }
            }

            for (var upgraded = 0; upgraded < original.UpgradedCreatures.Count; upgraded++)
            {
                var (before, after, index) = (original.UpgradedCreatures[upgraded], current.UpgradedCreatures[upgraded], upgraded);
                if (before != after)
                {
                    Add(ChangeAreas.Dwellings, $"Creatures in upgraded dwelling place {upgraded + 1}", before.ToString("N0"), after.ToString("N0"), c => c with { UpgradedCreatures = c.UpgradedCreatures.With(index, before) });
                }
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

        private static string DwellingText(int level) => level switch { 0 => "not built", 1 => "built", 2 => "upgraded", _ => "upgraded twice" };

        private static string OwnerText(Color? owner) => owner?.ToString() ?? "nobody";

        private static string SpellText(int number) => number == 0 ? "none" : Spell.NameOf((byte)number);
    }
}
