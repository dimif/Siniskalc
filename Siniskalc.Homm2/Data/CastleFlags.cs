namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The two flags of a castle on the map: addons of the tile on each side of the castle's tile (the one at its left first), of the class
    /// <see cref="MapObjectLayout.FlagClass"/>, whose picture is twice the number of the owner's colour plus 0 for the left flag and 1 for the right one;
    /// a castle of nobody has the pictures of the colour after the last one (12 and 13). A game that conquers a castle redraws them; the
    /// record of the castle alone does not move them (the castle then shows its old colour on the map and is its new owner's in the lists).
    /// </summary>
    public static class CastleFlags
    {
        /// <summary>
        /// The number of the colour the flags of a castle of nobody use.
        /// </summary>
        public const int NeutralColorIndex = 6;

        /// <summary>
        /// The byte of the table of owners that a castle of nobody has.
        /// </summary>
        public const byte NobodyMark = 0xFF;

        /// <summary>
        /// Returns where the table of the colour of each castle starts: right after the records of the castles, one byte for each of them, <c>FF</c> for a
        /// castle of nobody. The game changes it when it conquers a castle, and the minimap draws the castles from it.
        /// </summary>
        /// <param name="layout">Where the tables of the savegame are.</param>
        /// <returns>The offset of the first byte.</returns>
        public static int OwnerTableStart(SaveLayout layout) => layout.CastleTableStart + (Constants.Table.CastleSlotCount * CastleLayout.RecordSize);

        /// <summary>
        /// Returns the byte of the table of owners for an owner.
        /// </summary>
        /// <param name="owner">The owner of the castle.</param>
        /// <returns>The colour byte, or <see cref="NobodyMark"/>.</returns>
        public static byte MarkOf(PlayerColor owner) => owner.IsNeutral ? NobodyMark : owner.Code;

        /// <summary>
        /// Returns the picture of a flag.
        /// </summary>
        /// <param name="owner">The owner of the castle.</param>
        /// <param name="rightSide">Whether it is the flag on the right of the castle.</param>
        /// <returns>The picture.</returns>
        public static byte PictureOf(PlayerColor owner, bool rightSide) => (byte)(((owner.IsNeutral ? NeutralColorIndex : owner.Code) * 2) + (rightSide ? 1 : 0));

        /// <summary>
        /// Finds the flags of the castle at <paramref name="x"/>, <paramref name="y"/>: those of the two neighbouring tiles that hold a flag picture.
        /// </summary>
        /// <param name="bytes">The savegame.</param>
        /// <param name="layout">Where the tables of the savegame are.</param>
        /// <param name="tiles">Where the table of tiles starts.</param>
        /// <param name="x">The column of the castle.</param>
        /// <param name="y">The row of the castle.</param>
        /// <returns>The class and picture of each flag found, with where they are in the savegame.</returns>
        public static IReadOnlyList<MapObjectPart> Find(byte[] bytes, SaveLayout layout, int tiles, int x, int y)
        {
            var size = layout.Width;
            var flags = new List<MapObjectPart>();
            if (tiles < 0 || y < 0 || y >= size)
            {
                return flags;
            }

            var addons = tiles + (size * size * MapTileTable.TileSize) + MapObjectLayout.AddonsFromTilesEnd;
            foreach (var column in new[] { x - 1, x + 1 })
            {
                if (column < 0 || column >= size)
                {
                    continue;
                }

                var tile = tiles + (MapTileTable.TileSize * ((y * size) + column));
                var addon = addons + (MapObjectLayout.AddonSize * BitConverter.ToUInt16(bytes, tile + MapObjectLayout.TileLink));
                if (addon + 2 <= bytes.Length && bytes[addon] == MapObjectLayout.FlagClass && bytes[addon + 1] <= (NeutralColorIndex * 2) + 1 && (bytes[addon + 1] & 1) == (column > x ? 1 : 0))
                {
                    flags.Add(new MapObjectPart(addon, [bytes[addon], bytes[addon + 1]]));
                }
            }

            return flags;
        }
    }
}
