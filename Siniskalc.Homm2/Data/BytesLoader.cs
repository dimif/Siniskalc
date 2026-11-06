namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Cuts the records out of a savegame: every table has a fixed number of records of a fixed size, so nothing has to be searched for.
    /// </summary>
    public sealed class BytesLoader : IBytesLoader
    {
        /// <inheritdoc/>
        public IReadOnlyList<HeroChunk> LoadHeroChunks(byte[] bytes, SaveLayout layout)
        {
            return Enumerable.Range(0, Constants.Table.HeroCount)
                .Select(i => new HeroChunk(
                    Bytes.ReadText(bytes, layout.HeroStart(i) + HeroLayout.Name, HeroLayout.NameLength),
                    bytes.AsSpan(layout.HeroStart(i), layout.HeroStride).ToArray(),
                    layout.HeroStart(i)))
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<PlayerChunk> LoadPlayerChunks(byte[] bytes, SaveLayout layout)
        {
            // The records are in the order the players play, not in the order of the colours, so each one says which colour it is.
            var colors = PlayerRecordOrder.ColorsOf(bytes, layout);
            return Enumerable.Range(0, Constants.Table.PlayerCount)
                .Select(color => colors.ToList().IndexOf(color))
                .Select(record => new PlayerChunk(PlayerColor.All[colors[record]].ToString(), bytes.AsSpan(layout.PlayerStart(record), PlayerLayout.RecordSize).ToArray(), layout.PlayerStart(record)))
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<ObeliskChunk> LoadObeliskChunks(byte[] bytes, SaveLayout layout)
        {
            var tiles = MapTileTable.FindStart(bytes, layout);
            if (tiles < 0)
            {
                return [];
            }

            var obelisks = MapTileTable.Find(MapTileTable.ObjectBytes(bytes, layout, tiles), layout.Width, ObeliskLayout.TileMarker)
                .Take(ObeliskLayout.Capacity)
                .ToList();
            if (obelisks.Count == 0)
            {
                return [];
            }

            var start = layout.CastleTableStart + (Constants.Table.CastleSlotCount * CastleLayout.RecordSize) + ObeliskLayout.FlagsFromCastlesEnd;
            return start + ObeliskLayout.Capacity <= bytes.Length
                ? [new ObeliskChunk("Obelisks", bytes.AsSpan(start, ObeliskLayout.Capacity).ToArray(), start, obelisks)]
                : [];
        }

        /// <inheritdoc/>
        public IReadOnlyList<CastleChunk> LoadCastleChunks(byte[] bytes, SaveLayout layout)
        {
            var tiles = MapTileTable.FindStart(bytes, layout);
            var owners = CastleFlags.OwnerTableStart(layout);
            return Enumerable.Range(0, Constants.Table.CastleSlotCount)
                .Select(i => new CastleChunk(
                    Bytes.ReadText(bytes, layout.CastleStart(i) + CastleLayout.Name, Constants.Castle.NameLength),
                    bytes.AsSpan(layout.CastleStart(i), CastleLayout.RecordSize).ToArray(),
                    layout.CastleStart(i),
                    Bytes.ReadText(bytes, layout.CastleStart(i) + CastleLayout.Name, Constants.Castle.NameLength).Length == 0
                        ? []
                        : CastleFlags.Find(bytes, layout, tiles, bytes[layout.CastleStart(i) + CastleLayout.X], bytes[layout.CastleStart(i) + CastleLayout.Y]),
                    Bytes.ReadText(bytes, layout.CastleStart(i) + CastleLayout.Name, Constants.Castle.NameLength).Length == 0 || owners + i >= bytes.Length
                        ? null
                        : new MapObjectPart(owners + i, [bytes[owners + i]])))
                .ToList()
                .AsReadOnly();
        }
    }
}
