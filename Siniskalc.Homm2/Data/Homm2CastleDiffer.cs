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
    /// Lists what an edit changed in a castle.
    /// </summary>
    public sealed class Homm2CastleDiffer : IHomm2CastleDiffer
    {
        public const string Empty = "empty";

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int castleIndex, Homm2CastleModel original, Homm2CastleModel current)
        {
            var changes = new List<PendingChange>();

            void Add(ChangeArea area, string what, string before, string after, Func<Homm2CastleModel, Homm2CastleModel> revert)
            {
                changes.Add(Homm2PendingChanges.ForCastle(current.Name, castleIndex, new Change(area, what, before, after), revert));
            }

            if (original.Owner != current.Owner)
            {
                Add(Homm2ChangeAreas.General, "Owner", OwnerText(original.Owner), OwnerText(current.Owner), c => c with { Owner = original.Owner });
            }

            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                var (before, after, index) = (original.Garrison.Slots[slot], current.Garrison.Slots[slot], slot);
                if (before != after)
                {
                    Add(Homm2ChangeAreas.Garrison, $"Slot {slot + 1}", before?.ToString() ?? Empty, after?.ToString() ?? Empty, c => c with { Garrison = c.Garrison.WithSlot(index, before) });
                }
            }

            foreach (var building in Enum.GetValues<Homm2TownBuildings>().Where(b => b != Homm2TownBuildings.None))
            {
                var (had, has) = (original.Town.HasFlag(building), current.Town.HasFlag(building));
                if (had != has)
                {
                    var which = building;
                    Add(Homm2ChangeAreas.Buildings, which.ToString(), had ? "built" : "not built", has ? "built" : "not built", c => c with { Town = had ? c.Town | which : c.Town & ~which });
                }
            }

            foreach (var dwelling in Enum.GetValues<Homm2Dwellings>().Where(d => d != Homm2Dwellings.None))
            {
                var (had, has) = (original.Dwellings.HasFlag(dwelling), current.Dwellings.HasFlag(dwelling));
                if (had != has)
                {
                    var which = dwelling;
                    Add(Homm2ChangeAreas.Dwellings, which.ToString(), had ? "built" : "not built", has ? "built" : "not built", c => c with { Dwellings = had ? c.Dwellings | which : c.Dwellings & ~which });
                }
            }

            for (var dwelling = 0; dwelling < Constants.Castle.DwellingCount; dwelling++)
            {
                var (before, after, index) = (original.Creatures[dwelling], current.Creatures[dwelling], dwelling);
                if (before != after)
                {
                    Add(Homm2ChangeAreas.Dwellings, $"Creatures in dwelling {dwelling + 1}", before.ToString("N0"), after.ToString("N0"), c => c with { Creatures = c.Creatures.With(index, before) });
                }
            }

            for (var slot = 0; slot < original.GuildSpells.Count && slot < current.GuildSpells.Count; slot++)
            {
                var (before, after, index) = (original.GuildSpells[slot], current.GuildSpells[slot], slot);
                if (before != after)
                {
                    var level = (slot / Constants.Castle.MageGuildSlotsPerLevel) + 1;
                    Add(Homm2ChangeAreas.MageGuild, $"Level {level}, spell {(slot % Constants.Castle.MageGuildSlotsPerLevel) + 1}", SpellText(before), SpellText(after), c => c with { GuildSpells = c.GuildSpells.With(index, before) });
                }
            }

            return changes;
        }

        private static string OwnerText(Homm2Color? owner) => owner?.ToString() ?? "nobody";

        private static string SpellText(int number) => number == 0 ? "none" : Homm2Spell.NameOf((byte)number);
    }
}
