namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Buffers.Binary;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Finds the layout of a savegame of Heroes of Might and Magic II, the base game's (<c>.GM1</c>) and the Price of Loyalty's (<c>.GX1</c>).
    /// </summary>
    public sealed class Homm2SaveLayoutLocator : IHomm2SaveLayoutLocator
    {
        // The largest map of the game is 144 tiles wide.
        private const int MaxMapSize = 144;

        /// <inheritdoc/>
        public int HeadLength => Homm2HeaderLayout.MarkerLength + Homm2HeaderLayout.MapFormatField + 4;

        /// <inheritdoc/>
        public bool Recognizes(ReadOnlySpan<byte> head) => ReadHeader(head) is not null;

        /// <inheritdoc/>
        public Homm2SaveLayout? Locate(byte[] bytes)
        {
            if (ReadHeader(bytes) is not { } header)
            {
                return null;
            }

            var expansion = header.Marker > 0;
            var shift = expansion ? Homm2HeaderLayout.ExpansionShift : 0;
            var stride = expansion ? Homm2HeroLayout.ExpansionRecordSize : Homm2HeroLayout.BaseRecordSize;
            var heroTable = Homm2HeaderLayout.HeroTable + shift;
            var layout = new Homm2SaveLayout(
                expansion ? Homm2GameVersion.PriceOfLoyalty : Homm2GameVersion.BaseGame,
                header.Marker,
                header.Size,
                header.Size,
                stride,
                Homm2HeaderLayout.Date + shift,
                Homm2HeaderLayout.PlayerTable + shift,
                heroTable,
                heroTable + (Constants.Table.HeroCount * stride) + Homm2HeaderLayout.CastleTableGap);

            if (bytes.Length < layout.MinimumLength || !HasTables(bytes, layout))
            {
                // TODO:LOG warning: a file with the header of a Heroes II savegame ({bytes.Length} bytes, map size {header.Size}) was refused because its tables are not where they are expected.
                return null;
            }

            return layout;
        }

        // Every hero and every castle holds its own number: that is what the tables of the two known versions have in common, and
        // what a table found at a wrong place would not.
        private static bool HasTables(byte[] bytes, Homm2SaveLayout layout)
        {
            for (var hero = 0; hero < Constants.Table.HeroCount; hero++)
            {
                if (bytes[layout.HeroStart(hero) + Homm2HeroLayout.Id] != hero)
                {
                    return false;
                }
            }

            for (var castle = 0; castle < Constants.Table.CastleSlotCount; castle++)
            {
                if (bytes[layout.CastleStart(castle) + Homm2CastleLayout.Index] != castle)
                {
                    return false;
                }
            }

            return true;
        }

        private static (int Marker, int Size)? ReadHeader(ReadOnlySpan<byte> bytes)
        {
            var marker = bytes.Length >= Homm2HeaderLayout.MarkerLength && BinaryPrimitives.ReadUInt32LittleEndian(bytes) == Homm2HeaderLayout.Marker
                ? Homm2HeaderLayout.MarkerLength
                : 0;
            if (bytes.Length < marker + Homm2HeaderLayout.MapFormatField + 4)
            {
                return null;
            }

            var width = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(marker + Homm2HeaderLayout.Width)..]);
            var height = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(marker + Homm2HeaderLayout.Height)..]);
            var format = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(marker + Homm2HeaderLayout.MapFormatField)..]);
            return format == Homm2HeaderLayout.MapFormat && width == height && width is > 0 and <= MaxMapSize ? (marker, (int)width) : null;
        }
    }
}
