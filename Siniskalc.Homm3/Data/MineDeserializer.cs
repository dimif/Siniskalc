namespace Siniskalc.Homm3.Data
{
    using System;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the record of a mine: its kind, its owner, the tile it gives and its guard.
    /// </summary>
    public sealed class MineDeserializer(CreatureCodes creatureCodes) : IDeserializer<RecordChunk, MineModel>
    {
        private static readonly ILogger<MineDeserializer> Logger = LogFactory.For<MineDeserializer>();

        /// <inheritdoc/>
        public MineModel Deserialize(RecordChunk chunk)
        {
            using var subject = Logger.BeginSubject("Mine", chunk.Name);
            var kind = (MineKind)chunk.GetByteAt(MineLayout.Kind);
            if (!Enum.IsDefined(kind))
            {
                Logger.LogTrace("Mine kind {Code:X2} uses {StandIn} metadata and retains its original byte", (byte)kind, creatureCodes.Catalog.MapMetadata.Mine(kind).Name);
            }

            return new(
                chunk.Index,
                kind,
                PlayerColor.FromByte(chunk.GetByteAt(MineLayout.Owner)),
                new MapPosition(chunk.GetByteAt(MineLayout.X), chunk.GetByteAt(MineLayout.Y), chunk.GetByteAt(MineLayout.Level)),
                ArmyReader.Read(chunk, MineLayout.Guard, creatureCodes),
                creatureCodes.Catalog);
        }
    }
}
