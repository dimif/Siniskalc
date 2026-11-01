namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Cuts the mines, the shrines and the sphinxes out of the map part of a savegame (see <see cref="MapObjectLayout"/> and <see cref="MapTileTable"/>).
    /// </summary>
    public sealed class MapObjectsReader : IMapObjectsReader
    {
        /// <summary>
        /// Added to the tag of a shrine that a hero stands on: the tile then holds the marker of the hero in the place of the spell.
        /// </summary>
        public const int HiddenShrine = 0x100;

        private readonly GameCatalogs catalogs;

        public MapObjectsReader(GameCatalogs catalogs)
        {
            this.catalogs = catalogs;
        }

        /// <inheritdoc/>
        public MapObjectChunks Read(byte[] bytes, SaveLayout layout)
        {
            var tiles = MapTileTable.FindStart(bytes, layout);
            var mapStart = layout.CastleTableStart + (Constants.Table.CastleSlotCount * CastleLayout.RecordSize);
            var objects = tiles < 0 ? [] : MapTileTable.ObjectBytes(bytes, layout, tiles);
            var size = layout.Width;
            var sphinxTiles = tiles < 0 ? [] : MapTileTable.Find(objects, size, MapObjectLayout.Sphinx);
            var firstTable = tiles < 0 ? int.MaxValue : tiles;
            var catalog = this.catalogs.For(layout.Version);

            return new MapObjectChunks(
                tiles < 0 ? [] : ReadMines(bytes, layout, mapStart, tiles, catalog),
                tiles < 0 ? [] : ReadShrines(bytes, layout, tiles, objects, catalog),
                ReadSphinxes(bytes, mapStart, firstTable, sphinxTiles, catalog),
                [new MapObjectChunk("Current turn", bytes.AsSpan(layout.CurrentHumanStart, 1).ToArray(), layout.CurrentHumanStart, null, 0, PlayerRecordOrder.Pack(PlayerRecordOrder.ColorsOf(bytes, layout)))],
                ReadBuiltToday(bytes, mapStart));
        }

        private static List<MapObjectChunk> ReadBuiltToday(byte[] bytes, int mapStart)
        {
            var start = mapStart + MapObjectLayout.BuiltToday;
            return [new MapObjectChunk("Built this turn", bytes.AsSpan(start, MapObjectLayout.BuiltTodaySize).ToArray(), start, null, 0)];
        }

        private static List<MapObjectChunk> ReadMines(byte[] bytes, SaveLayout layout, int mapStart, int tiles, GameCatalog catalog)
        {
            var size = layout.Width;
            var chunks = new List<MapObjectChunk>();
            for (var offset = mapStart + MapObjectLayout.MineTable; offset + MapObjectLayout.MineRecordSize <= mapStart + MapObjectLayout.MineTableEnd; offset += MapObjectLayout.MineRecordSize)
            {
                var (owner, kind, x, y) = (bytes[offset], bytes[offset + 1], bytes[offset + 4], bytes[offset + 5]);
                if (bytes[offset + 2] != 0xFF || bytes[offset + 3] != 0xFF || bytes[offset + 6] != 0xFF || x >= size || y >= size || kind > (byte)MineKind.GoldMine || (owner != 0xFF && owner > PlayerColor.Purple.Code))
                {
                    continue;
                }

                var parts = new List<MapObjectPart>();
                if (FlagPlace(bytes, tiles, size, (MineKind)kind, x, y) is { } place)
                {
                    parts.Add(new MapObjectPart(place, bytes.AsSpan(place, 2).ToArray()));
                }

                var position = new MapPosition(x, y);
                chunks.Add(new MapObjectChunk($"{catalog.Mines.Find(kind)!.Name} at {position.X}, {position.Y}", bytes.AsSpan(offset, MapObjectLayout.MineRecordSize).ToArray(), offset, position, chunks.Count, kind, parts));
            }

            return chunks;
        }

        // Where the flag of a mine is drawn: a sawmill has it on the tile up and to the right, the other mines on their own tile, in the
        // second object of the tile when nothing is there and otherwise in the addon the tile links to. An alchemist's lab has none.
        private static int? FlagPlace(byte[] bytes, int tiles, int size, MineKind kind, int x, int y)
        {
            if (kind == MineKind.AlchemistLab)
            {
                return null;
            }

            var picture = MineFlags.PictureOf(kind);
            bool IsFlagOrEmpty(int at) =>
                (bytes[at] == MapObjectLayout.NoClass && bytes[at + 1] == MapObjectLayout.NoPicture)
                || (bytes[at] == MapObjectLayout.FlagClass && bytes[at + 1] >= picture && bytes[at + 1] < picture + 7);

            if (kind == MineKind.Sawmill)
            {
                if (x + 1 >= size || y < 1)
                {
                    return null;
                }

                var up = tiles + (MapTileTable.TileSize * (((y - 1) * size) + x + 1)) + MapObjectLayout.TileSlot;
                return IsFlagOrEmpty(up) ? up : null;
            }

            var tile = tiles + (MapTileTable.TileSize * ((y * size) + x));
            if (IsFlagOrEmpty(tile + MapObjectLayout.TileSlot))
            {
                return tile + MapObjectLayout.TileSlot;
            }

            var addon = tiles + (size * size * MapTileTable.TileSize) + MapObjectLayout.AddonsFromTilesEnd + (MapObjectLayout.AddonSize * BitConverter.ToUInt16(bytes, tile + MapObjectLayout.TileLink));
            return addon + 2 <= bytes.Length && IsFlagOrEmpty(addon) ? addon : null;
        }

        private static List<MapObjectChunk> ReadShrines(byte[] bytes, SaveLayout layout, int tiles, byte[] objects, GameCatalog catalog)
        {
            var size = layout.Width;
            var chunks = new List<MapObjectChunk>();
            for (var tile = 0; tile < objects.Length; tile++)
            {
                var circle = objects[tile] switch
                {
                    MapObjectLayout.ShrineFirstCircle => 1,
                    MapObjectLayout.ShrineSecondCircle => 2,
                    MapObjectLayout.ShrineThirdCircle => 3,
                    _ => 0,
                };
                if (circle == 0)
                {
                    continue;
                }

                var offset = tiles + (tile * MapTileTable.TileSize);
                var hidden = bytes[offset + MapTileTable.ObjectOffset] == MapObjectLayout.HeroMarker;
                var position = new MapPosition(tile % size, tile / size);
                chunks.Add(new MapObjectChunk(catalog.MapObjects.Single(o => o.Role == MapObjectRole.ShrineMarker).FormatName(position.X, position.Y), bytes.AsSpan(offset + MapObjectLayout.TileQuantity, 2).ToArray(), offset + MapObjectLayout.TileQuantity, position, chunks.Count, circle | (hidden ? HiddenShrine : 0)));
            }

            return chunks;
        }

        private static List<MapObjectChunk> ReadSphinxes(byte[] bytes, int mapStart, int end, IReadOnlyList<MapPosition> tiles, GameCatalog catalog)
        {
            var found = new List<(int Start, int Length)>();
            var limit = Math.Min(end, bytes.Length);
            var at = mapStart;
            while (at + MapObjectLayout.BlockHeader <= limit)
            {
                var next = bytes.AsSpan(at, limit - at).IndexOf(MapObjectLayout.BlockMarker);
                if (next < 0)
                {
                    break;
                }

                var block = at + next;
                if (block + MapObjectLayout.BlockHeader > limit)
                {
                    break;
                }

                var length = BitConverter.ToUInt16(bytes, block + 4);
                if (block + MapObjectLayout.BlockHeader + length <= limit && IsSphinx(bytes, block + MapObjectLayout.BlockHeader, length))
                {
                    found.Add((block + MapObjectLayout.BlockHeader, length));
                }

                at = block + 1;
            }

            return found
                .Select((block, index) => new MapObjectChunk(
                    catalog.MapObjects.Single(o => o.Role == MapObjectRole.SphinxUnlocated).FormatName(index + 1),
                    bytes.AsSpan(block.Start, block.Length).ToArray(),
                    block.Start,
                    tiles.Count == found.Count ? tiles[index] : null,
                    index))
                .ToList();
        }

        // A sphinx block: room for a riddle, 1 to 8 answers of printable text, and a flag that is 0 or 1.
        private static bool IsSphinx(byte[] bytes, int payload, int length)
        {
            if (length <= MapObjectLayout.SphinxRiddle || bytes[payload + MapObjectLayout.SphinxActive] > 1)
            {
                return false;
            }

            var count = bytes[payload + MapObjectLayout.SphinxAnswerCount];
            if (count is < 1 or > Constants.Sphinx.MaxAnswers)
            {
                return false;
            }

            for (var answer = 0; answer < count; answer++)
            {
                var text = bytes.AsSpan(payload + MapObjectLayout.SphinxAnswers + (answer * MapObjectLayout.SphinxAnswerSize), MapObjectLayout.SphinxAnswerSize);
                var end = text.IndexOf((byte)0);
                var printable = end > 0 && text[..end].ToArray().All(b => b is >= 32 and < 127);
                if (!printable)
                {
                    return false;
                }
            }

            var riddle = bytes.AsSpan(payload + MapObjectLayout.SphinxRiddle, length - MapObjectLayout.SphinxRiddle);
            return riddle.IndexOf((byte)0) > 0;
        }
    }
}
