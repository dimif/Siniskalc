namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Entities;

    /// <summary>Validates counted owner/guard records and the following counted creature generators as complete tables.</summary>
    /// <remarks>The one-byte owner/guard count is verified in SoD; its use in RoE, AB and Chronicles remains an assumption.</remarks>
    public sealed class MapObjectTablesLocator : IMapObjectTablesLocator
    {
        private static readonly ILogger<MapObjectTablesLocator> Logger = LogFactory.For<MapObjectTablesLocator>();

        /// <inheritdoc/>
        public MapObjectTables? Locate(byte[] bytes, GameVersion version)
        {
            var countSize = version == GameVersion.HornOfTheAbyss ? MineLayout.HotaCountSize : MineLayout.OriginalCountSize;
            var candidates = new List<MapObjectTables>();
            for (var header = 0; header <= bytes.Length - countSize - MineLayout.RecordSize; header++)
            {
                var count = countSize == sizeof(uint) ? BitConverter.ToUInt32(bytes, header) : bytes[header];
                var start = header + countSize;
                if (count == 0 || count > (bytes.Length - start) / MineLayout.RecordSize)
                {
                    continue;
                }

                var end = start + ((int)count * MineLayout.RecordSize);
                if (LooksLikeOwnerGuard(bytes, start)
                    && Enumerable.Range(1, (int)count - 1).All(i => LooksLikeOwnerGuard(bytes, start + (i * MineLayout.RecordSize)))
                    && GeneratorCount(bytes, end) is int generators)
                {
                    candidates.Add(new(start, (int)count, end + DwellingLayout.CountSize, generators));
                }
            }

            // With no owner/guard records, require the explicit zero count and a nonempty, fully guarded generator table.
            for (var at = countSize; at <= bytes.Length - DwellingLayout.CountSize - DwellingLayout.EditableSize; at++)
            {
                var type = bytes[at + DwellingLayout.CountSize + DwellingLayout.ObjectType];
                if (type < DwellingLayout.CreatureGeneratorType || type > DwellingLayout.LastGeneratorType
                    || (countSize == sizeof(uint) ? BitConverter.ToUInt32(bytes, at - countSize) : bytes[at - countSize]) != 0)
                {
                    continue;
                }

                if (GeneratorCount(bytes, at) is int count && count > 0)
                {
                    candidates.Add(new(at, 0, at + DwellingLayout.CountSize, count));
                }
            }

            // A guard count or trailing zero level can resemble a smaller table within the genuine pair.
            var tables = candidates.Where(c => !candidates.Any(other => other.OwnerGuardStart < c.OwnerGuardStart && other.GeneratorEnd >= c.GeneratorEnd)).ToList();
            if (tables.Count > 1)
            {
                Logger.LogWarning("Found {Count} competing map-object tables; none is exposed for editing", tables.Count);
                return null;
            }

            if (tables.Count == 0)
            {
                Logger.LogDebug("No independently validated map-object tables were found");
                return null;
            }

            var table = tables[0];
            Logger.LogDebug("Located {Owners} owner/guard records at {OwnerOffset:X} and {Generators} generators at {GeneratorOffset:X}", table.OwnerGuardCount, table.OwnerGuardStart, table.GeneratorCount, table.GeneratorStart);
            return table;
        }

        private static bool LooksLikeOwnerGuard(byte[] bytes, int start)
        {
            var owner = bytes[start + MineLayout.Owner];
            return ValidOwner(owner)
                && ValidPosition(bytes, start + MineLayout.X)
                && bytes[start + MineLayout.Abandoned] <= 1
                && ValidGuard(bytes, start + MineLayout.Guard.TypeOffset);
        }

        private static int? GeneratorCount(byte[] bytes, int at)
        {
            if (at > bytes.Length - DwellingLayout.CountSize)
            {
                return null;
            }

            var count = BitConverter.ToUInt16(bytes, at);
            var start = at + DwellingLayout.CountSize;
            if (count > (bytes.Length - start + (DwellingLayout.RecordSize - DwellingLayout.EditableSize)) / DwellingLayout.RecordSize)
            {
                return null;
            }

            for (var index = 0; index < count; index++)
            {
                var record = start + (index * DwellingLayout.RecordSize);
                var type = bytes[record + DwellingLayout.ObjectType];
                if (!ValidOwner(bytes[record + DwellingLayout.Owner])
                    || type < DwellingLayout.CreatureGeneratorType || type > DwellingLayout.LastGeneratorType
                    || !ValidPosition(bytes, record + DwellingLayout.X)
                    || !ValidGuard(bytes, record + DwellingLayout.Guard.TypeOffset))
                {
                    return null;
                }
            }

            return count;
        }

        private static bool ValidOwner(byte owner) => owner == MineLayout.NoOwner || owner < PlayerColor.All.Count;

        private static bool ValidPosition(byte[] bytes, int at) =>
            bytes[at] <= Constants.Map.MaxCoordinate && bytes[at + 1] <= Constants.Map.MaxCoordinate && bytes[at + 2] <= 1;

        private static bool ValidGuard(byte[] bytes, int at)
        {
            for (var slot = 0; slot < MineLayout.SlotCount; slot++)
            {
                var type = BitConverter.ToUInt32(bytes, at + (slot * sizeof(uint)));
                var count = BitConverter.ToUInt32(bytes, at + ((MineLayout.SlotCount + slot) * sizeof(uint)));
                if (type == uint.MaxValue ? count != 0 : type > MineLayout.MaxCreatureCode || count == 0 || count > MineLayout.MaxCount)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
