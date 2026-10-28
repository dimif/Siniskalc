namespace Siniskalc.Homm2.Tests.Data
{
    using System.Text;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A savegame made by hand for the readers of the map: the tables of the players, the heroes and the castles one after the other (no
    /// hero on the map), then the area of the mines and of the sphinxes, then the table of the tiles of a square map and its addons.
    /// </summary>
    internal sealed class MapSavegame
    {
        public const int PlayerTable = 100;
        public const int HeroTable = 1500;
        public const int CastleTable = 14500;
        public const int MapStart = CastleTable + (72 * 100);
        public const int SphinxArea = MapStart + 0x540;
        public const int SphinxBlockStride = 400;

        private readonly int size;

        public MapSavegame(int size, bool withTiles = true)
        {
            this.size = size;
            this.Layout = new SaveLayout(GameVersion.BaseGame, size, size, HeroLayout.BaseRecordSize, 50, PlayerTable, HeroTable, CastleTable);
            this.PairAt = SphinxArea + (8 * SphinxBlockStride) + 100;
            this.Tiles = this.PairAt + 8;
            this.Bytes = new byte[withTiles ? this.AddonAt(20) + 100 : this.PairAt];
            for (var hero = 0; hero < 54; hero++)
            {
                this.Bytes[this.Layout.HeroStart(hero) + HeroLayout.Owner] = 0xFF;
            }

            if (withTiles)
            {
                BitConverter.GetBytes(size).CopyTo(this.Bytes, this.PairAt);
                BitConverter.GetBytes(size).CopyTo(this.Bytes, this.PairAt + 4);
            }
        }

        public SaveLayout Layout { get; }

        public byte[] Bytes { get; }

        public int PairAt { get; }

        public int Tiles { get; }

        public int Tile(int x, int y) => this.Tiles + (MapTileTable.TileSize * ((y * this.size) + x));

        public int AddonAt(int index) => this.Tiles + (this.size * this.size * MapTileTable.TileSize) + MapObjectLayout.AddonsFromTilesEnd + (MapObjectLayout.AddonSize * index);

        public void Object(int x, int y, byte value) => this.Bytes[this.Tile(x, y) + MapTileTable.ObjectOffset] = value;

        /// <summary>Puts a hero on a tile: the tile holds the marker of a hero and the hero keeps the byte of the object that is under it.</summary>
        public void Hero(int index, int x, int y, byte objectUnderIt)
        {
            var record = this.Layout.HeroStart(index);
            this.Bytes[record + HeroLayout.Owner] = 0;
            BitConverter.GetBytes((uint)x).CopyTo(this.Bytes, record + HeroLayout.X);
            BitConverter.GetBytes((uint)y).CopyTo(this.Bytes, record + HeroLayout.Y);
            this.Bytes[record + HeroLayout.TileObject] = objectUnderIt;
            this.Object(x, y, MapObjectLayout.HeroMarker);
        }

        /// <summary>Writes the record of a mine as the table of mines has it, in its place number <paramref name="slot"/>.</summary>
        public int Mine(int slot, byte owner, byte kind, byte x, byte y)
        {
            var at = MapStart + MapObjectLayout.MineTable + (slot * MapObjectLayout.MineRecordSize);
            new byte[] { owner, kind, 0xFF, 0xFF, x, y, 0xFF }.CopyTo(this.Bytes, at);
            return at;
        }

        /// <summary>Writes a block of a sphinx in its place number <paramref name="slot"/> of the area of the sphinxes and returns where its payload starts.</summary>
        public int Sphinx(int slot, Action<byte[]>? change = null, int length = 236)
        {
            var at = SphinxArea + (slot * SphinxBlockStride);
            MapObjectLayout.BlockMarker.CopyTo(this.Bytes, at);
            BitConverter.GetBytes((ushort)length).CopyTo(this.Bytes, at + 4);
            var payload = new byte[length];
            payload[MapObjectLayout.SphinxActive] = 1;
            payload[MapObjectLayout.SphinxAnswerCount] = 1;
            Encoding.Latin1.GetBytes("answer").CopyTo(payload, MapObjectLayout.SphinxAnswers);
            if (length > MapObjectLayout.SphinxRiddle + 20)
            {
                Encoding.Latin1.GetBytes("What am I?").CopyTo(payload, MapObjectLayout.SphinxRiddle);
            }

            change?.Invoke(payload);
            payload.CopyTo(this.Bytes, at + MapObjectLayout.BlockHeader);
            return at + MapObjectLayout.BlockHeader;
        }
    }
}
