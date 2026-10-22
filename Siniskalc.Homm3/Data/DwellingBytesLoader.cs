namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Finds the table of dwellings by what its records look like (see <see cref="DwellingLayout"/>): a run of records of 150 bytes that all
    /// name the type of a creature generator, with an owner that is a colour or none and a tile on the map.
    /// </summary>
    public sealed class DwellingBytesLoader : IDwellingBytesLoader
    {
        /// <inheritdoc/>
        public IReadOnlyList<DwellingChunk> LoadDwellingChunks(byte[] bytes, IReadOnlyList<MineChunk> mines, IEnumerable<BytesChunk> others)
        {
            // The first record starts right after the last mine (the table of the mines ends with the last mine on the test map). Anywhere else
            // the same bytes can be a table by chance, and a chunk that overlaps another one stops the savegame from being written.
            // TODO:LOG info: a map whose table of the mines ends with other records has no dwellings read until the start of the table is found.
            if (mines.Count == 0)
            {
                return [];
            }

            var taken = others.ToList();
            var at = mines[^1].End;
            var length = 0;
            while (at + ((length + 1) * DwellingLayout.RecordSize) <= bytes.Length
                && LooksLikeARecord(bytes, at + (length * DwellingLayout.RecordSize))
                && !taken.Any(o => o.StartIndex < at + ((length + 1) * DwellingLayout.RecordSize) && o.End > at + (length * DwellingLayout.RecordSize)))
            {
                length++;
            }

            return length < DwellingLayout.MinimumRun ? [] : Enumerable.Range(0, length).Select(i => Chunk(bytes, at + (i * DwellingLayout.RecordSize), i)).ToList();
        }

        private static DwellingChunk Chunk(byte[] bytes, int start, int index)
        {
            var creature = Creature.FromByte(bytes[start + DwellingLayout.Creature]);
            return new DwellingChunk(
                $"{creature?.Name ?? $"Creature {bytes[start + DwellingLayout.Creature]}"} dwelling {bytes[start + DwellingLayout.X]}, {bytes[start + DwellingLayout.Y]}",
                bytes.AsSpan(start, DwellingLayout.RecordSize).ToArray(),
                start,
                index);
        }

        private static bool LooksLikeARecord(byte[] bytes, int start)
        {
            var owner = bytes[start + DwellingLayout.Owner];
            return bytes[start + DwellingLayout.ObjectType] == DwellingLayout.CreatureGeneratorType
                && (owner == DwellingLayout.NoOwner || owner < PlayerColor.All.Count)
                && bytes[start + DwellingLayout.Level] <= 1
                && bytes[start + DwellingLayout.X] < Constants.Map.MaxCoordinate
                && bytes[start + DwellingLayout.Y] < Constants.Map.MaxCoordinate;
        }
    }
}
