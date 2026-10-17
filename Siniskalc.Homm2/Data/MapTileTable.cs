namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Finds and reads the table of tiles of a savegame: after the castles the map part has the width and the height of the map as two 32 bit
    /// numbers, then 12 bytes for each tile, row by row. The byte at 9 of a tile tells what the object a hero enters by is (see
    /// <see cref="ObjectBytes"/>); on the tile of a hero it holds a marker, and the hero keeps the real byte (<see cref="HeroLayout.TileObject"/>).
    /// </summary>
    public static class MapTileTable
    {
        public const int TileSize = 12;

        public const int ClassOffset = 2;

        public const int PictureOffset = 3;

        public const int ObjectOffset = 9;

        /// <summary>
        /// Returns where the tiles begin in the bytes of a savegame, or -1 when the table is not found.
        /// </summary>
        public static int FindStart(byte[] bytes, SaveLayout layout)
        {
            var size = layout.Width;
            if (layout.Width != layout.Height || size <= 0)
            {
                return -1;
            }

            var start = layout.CastleTableStart + (Constants.Table.CastleSlotCount * CastleLayout.RecordSize);
            var pair = new byte[8];
            BitConverter.GetBytes(size).CopyTo(pair, 0);
            BitConverter.GetBytes(size).CopyTo(pair, 4);
            var at = bytes.AsSpan(start).IndexOf(pair);
            if (at < 0)
            {
                return -1;
            }

            var tiles = start + at + 8;
            return tiles + (size * size * TileSize) <= bytes.Length ? tiles : -1;
        }

        /// <summary>
        /// Returns the byte at 9 of every tile in the order of the map, with the bytes of the tiles that heroes stand on put back from the
        /// records of the heroes.
        /// </summary>
        public static byte[] ObjectBytes(byte[] bytes, SaveLayout layout, int tiles)
        {
            var size = layout.Width;
            var result = new byte[size * size];
            for (var tile = 0; tile < result.Length; tile++)
            {
                result[tile] = bytes[tiles + (tile * TileSize) + ObjectOffset];
            }

            for (var hero = 0; hero < Constants.Table.HeroCount; hero++)
            {
                var record = layout.HeroStart(hero);
                if (bytes[record + HeroLayout.Owner] == Bytes.Empty)
                {
                    continue;
                }

                var (x, y) = (BitConverter.ToUInt32(bytes, record + HeroLayout.X), BitConverter.ToUInt32(bytes, record + HeroLayout.Y));
                if (x < size && y < size)
                {
                    result[(int)((y * (uint)size) + x)] = bytes[record + HeroLayout.TileObject];
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the position of every tile whose byte at 9 is <paramref name="value"/>, in the order of the map (row by row).
        /// </summary>
        public static IReadOnlyList<MapPosition> Find(byte[] objectBytes, int size, byte value, Func<int, bool>? keep = null)
        {
            var found = new List<MapPosition>();
            for (var tile = 0; tile < objectBytes.Length; tile++)
            {
                if (objectBytes[tile] == value && (keep is null || keep(tile)))
                {
                    found.Add(new MapPosition(tile % size, tile / size));
                }
            }

            return found;
        }
    }
}
