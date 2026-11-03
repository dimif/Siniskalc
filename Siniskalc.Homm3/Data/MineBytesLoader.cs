namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>Returns the mines within an independently located, complete owner/guard table.</summary>
    public sealed class MineBytesLoader(EntityCatalog catalog, IMapObjectTablesLocator tablesLocator) : IMineBytesLoader
    {
        private static readonly ILogger<MineBytesLoader> Logger = LogFactory.For<MineBytesLoader>();

        /// <inheritdoc/>
        public IReadOnlyList<RecordChunk> LoadMineChunks(byte[] bytes)
        {
            var table = tablesLocator.Locate(bytes, catalog.Version);
            if (table is null)
            {
                return [];
            }

            var chunks = new List<RecordChunk>();
            for (var index = 0; index < table.OwnerGuardCount; index++)
            {
                var start = table.OwnerGuardStart + (index * MineLayout.RecordSize);
                if (bytes[start + MineLayout.Kind] <= MineLayout.LastMineKind)
                {
                    var kind = (MineKind)bytes[start + MineLayout.Kind];
                    chunks.Add(new RecordChunk(
                        catalog.MapMetadata.MineNameAt(kind, new MapPosition(bytes[start + MineLayout.X], bytes[start + MineLayout.Y], bytes[start + MineLayout.Level])),
                        bytes.AsSpan(start, MineLayout.RecordSize).ToArray(),
                        start,
                        chunks.Count));
                }
            }

            Logger.LogDebug("Read {Mines} mines from {Records} owner/guard records at {Offset:X}", chunks.Count, table.OwnerGuardCount, table.OwnerGuardStart);
            return chunks;
        }
    }
}
