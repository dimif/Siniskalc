namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Entities;

    /// <summary>Returns single-creature dwellings from an independently located, complete generator table.</summary>
    public sealed class DwellingBytesLoader(EntityCatalog catalog, IMapObjectTablesLocator tablesLocator) : IDwellingBytesLoader
    {
        private static readonly ILogger<DwellingBytesLoader> Logger = LogFactory.For<DwellingBytesLoader>();

        /// <inheritdoc/>
        public IReadOnlyList<RecordChunk> LoadDwellingChunks(byte[] bytes, IEnumerable<BytesChunk> others)
        {
            var table = tablesLocator.Locate(bytes, catalog.Version);
            if (table is null)
            {
                return [];
            }

            if (table.GeneratorCount > 0 && others.Any(o => o.StartIndex < table.GeneratorEnd && o.End > table.GeneratorStart))
            {
                Logger.LogWarning("The generator table at {Offset:X} overlaps another save record; no dwellings are exposed for editing", table.GeneratorStart);
                return [];
            }

            var chunks = new List<RecordChunk>();
            var unknownCreatures = 0;
            for (var index = 0; index < table.GeneratorCount; index++)
            {
                var start = table.GeneratorStart + (index * DwellingLayout.RecordSize);
                if (bytes[start + DwellingLayout.ObjectType] != DwellingLayout.CreatureGeneratorType)
                {
                    continue;
                }

                var code = bytes[start + DwellingLayout.Creature];
                var creature = catalog.Creatures.Find(code);
                if (creature is null)
                {
                    unknownCreatures++;
                    Logger.LogTrace("Dwelling creature code {Code:X2} at {Offset:X} is unknown; showing its code and preserving the record", code, start);
                }

                chunks.Add(new RecordChunk(
                    $"{creature?.Name ?? $"Creature {code}"} {catalog.MapMetadata.DwellingName} {bytes[start + DwellingLayout.X]}, {bytes[start + DwellingLayout.Y]}",
                    bytes.AsSpan(start, DwellingLayout.EditableSize).ToArray(),
                    start,
                    chunks.Count));
            }

            if (unknownCreatures > 0)
            {
                Logger.LogWarning("Found {Count} dwellings of an unknown creature at {Offset:X}; showing their codes and preserving the records", unknownCreatures, table.GeneratorStart);
            }

            Logger.LogDebug("Read {Count} dwellings from {Generators} generator records at {Offset:X}; only type-17 dwellings are editable", chunks.Count, table.GeneratorCount, table.GeneratorStart);
            return chunks;
        }
    }
}
