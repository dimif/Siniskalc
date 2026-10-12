namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// Finds where the objects of the map are, in a savefile that has them: after the table of object definitions (a 16-bit length, the
    /// name of a .def file and 33 more bytes, for each) and a 16-bit count there is one entry for each object of the map: a 16-bit definition number and
    /// the x, y and z of the object, one byte each. An object that has a record of its own (an obelisk, with its visit marks) holds its
    /// number in that list. Found by comparing the positions of the obelisks with those of a hero standing on each, in the Restoration of
    /// Erathia saves 68 and 72 to 75; seen in Shadow of Death saves too, not looked for in other versions.
    /// </summary>
    public static class MapObjectPositions
    {
        private const int DefEntryExtraLength = 33;
        private const int CountLength = 2;
        private const int PositionEntryLength = 5;
        private const int MinDefinitions = 5;
        private const int MaxDefNameLength = 20;

        /// <summary>
        /// Returns the position of every object of the map, in the order of the list, or an empty list when the table is not found.
        /// </summary>
        /// <param name="bytes">The savefile.</param>
        /// <param name="searchEnd">Where to stop looking: the table comes before the players.</param>
        public static IReadOnlyList<Position> Find(byte[] bytes, int searchEnd)
        {
            var end = DefinitionsEnd(bytes, Math.Min(searchEnd, bytes.Length));
            if (end < 0)
            {
                return [];
            }

            // A 16-bit count of the objects comes right after the definitions, then the list.
            var count = BitConverter.ToUInt16(bytes, end);
            var positions = new List<Position>();
            for (var at = end + CountLength; positions.Count < count && at + PositionEntryLength <= searchEnd; at += PositionEntryLength)
            {
                var (x, y, z) = (bytes[at + 2], bytes[at + 3], bytes[at + 4]);
                if (z > 1 || x >= Constants.Map.MaxCoordinate || y >= Constants.Map.MaxCoordinate)
                {
                    break;
                }

                positions.Add(new Position(x, y, z));
            }

            return positions;
        }

        private static int DefinitionsEnd(byte[] bytes, int searchEnd)
        {
            for (var i = 2; i + 4 <= searchEnd; i++)
            {
                if (bytes[i] != (byte)'.' || bytes[i + 1] != (byte)'d' || bytes[i + 2] != (byte)'e' || bytes[i + 3] != (byte)'f')
                {
                    continue;
                }

                for (var length = 4; length <= MaxDefNameLength; length++)
                {
                    var entry = i + 4 - length - 2;
                    if (entry >= 0 && IsDefinitionAt(bytes, entry, searchEnd))
                    {
                        var count = 0;
                        var at = entry;
                        while (IsDefinitionAt(bytes, at, searchEnd))
                        {
                            at += 2 + BitConverter.ToUInt16(bytes, at) + DefEntryExtraLength;
                            count++;
                        }

                        if (count >= MinDefinitions)
                        {
                            return at;
                        }
                    }
                }
            }

            return -1;
        }

        private static bool IsDefinitionAt(byte[] bytes, int at, int searchEnd)
        {
            if (at + 2 > searchEnd)
            {
                return false;
            }

            var length = BitConverter.ToUInt16(bytes, at);
            if (length < 4 || length > MaxDefNameLength || at + 2 + length > searchEnd)
            {
                return false;
            }

            for (var i = 0; i < length; i++)
            {
                var c = bytes[at + 2 + i];
                if (c < 0x20 || c > 0x7E)
                {
                    return false;
                }
            }

            return bytes[at + 2 + length - 4] == (byte)'.' && (bytes[at + 2 + length - 3] | 0x20) == 'd' && (bytes[at + 2 + length - 2] | 0x20) == 'e' && (bytes[at + 2 + length - 1] | 0x20) == 'f';
        }
    }
}
