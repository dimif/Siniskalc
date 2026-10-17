namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Buffers.Binary;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Finds the layout of a savegame of Heroes of Might and Magic II, the base game's (<c>.GM1</c>) and the Price of Loyalty's (<c>.GX1</c>).
    /// </summary>
    public sealed class SaveLayoutLocator : ISaveLayoutLocator
    {
        /// <inheritdoc/>
        public int HeadLength => HeaderLayout.MarkerLength + HeaderLayout.MapFormatField + 4;

        /// <inheritdoc/>
        public bool Recognizes(ReadOnlySpan<byte> head) => ReadHeader(head) is not null;

        /// <inheritdoc/>
        public SaveLayout? Locate(byte[] bytes)
        {
            if (ReadHeader(bytes) is not { } header)
            {
                return null;
            }

            var expansion = header.Version == GameVersion.PriceOfLoyalty;
            var shift = expansion ? HeaderLayout.ExpansionShift : 0;
            var stride = expansion ? HeroLayout.ExpansionRecordSize : HeroLayout.BaseRecordSize;
            var heroTable = HeaderLayout.HeroTable + shift;
            var layout = new SaveLayout(
                header.Version,
                header.Size,
                header.Size,
                stride,
                HeaderLayout.Date + shift,
                HeaderLayout.PlayerTable + shift,
                heroTable,
                heroTable + (Constants.Table.HeroCount * stride) + HeaderLayout.CastleTableGap);

            if (bytes.Length < layout.MinimumLength || !HasTables(bytes, layout))
            {
                // TODO:LOG warning: a file with the header of a Heroes II savegame ({bytes.Length} bytes, map size {header.Size}) was refused because its tables are not where they are expected.
                return null;
            }

            return layout;
        }

        // Every castle holds its own number, and so does a hero unless the map gave it a portrait of its own (the heroes of a scenario
        // such as "Famine" or "Pestilence" hold another number): that is what the tables of the known versions have in common, and what a
        // table found at a wrong place would not, so most heroes must hold theirs.
        private static bool HasTables(byte[] bytes, SaveLayout layout)
        {
            var heroesInPlace = 0;
            for (var hero = 0; hero < Constants.Table.HeroCount; hero++)
            {
                if (bytes[layout.HeroStart(hero) + HeroLayout.Id] == hero)
                {
                    heroesInPlace++;
                }
            }

            if (heroesInPlace < Constants.Table.HeroCount / 2)
            {
                return false;
            }

            for (var castle = 0; castle < Constants.Table.CastleSlotCount; castle++)
            {
                if (bytes[layout.CastleStart(castle) + CastleLayout.Index] != castle)
                {
                    return false;
                }
            }

            return true;
        }

        private static (GameVersion Version, int Size)? ReadHeader(ReadOnlySpan<byte> bytes)
        {
            var version = bytes.Length >= HeaderLayout.MarkerLength && BinaryPrimitives.ReadUInt32LittleEndian(bytes) == HeaderLayout.Marker
                ? GameVersion.PriceOfLoyalty
                : GameVersion.BaseGame;
            var marker = version == GameVersion.PriceOfLoyalty ? HeaderLayout.MarkerLength : 0;
            if (bytes.Length < marker + HeaderLayout.MapFormatField + 4)
            {
                return null;
            }

            var width = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(marker + HeaderLayout.Width)..]);
            var height = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(marker + HeaderLayout.Height)..]);
            var format = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(marker + HeaderLayout.MapFormatField)..]);
            return format == HeaderLayout.MapFormat && width == height && width is > 0 and <= Constants.Table.MaxMapSize ? (version, (int)width) : null;
        }
    }
}
