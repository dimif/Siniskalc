namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// Finds the obelisks the player has visited: see <see cref="ObeliskLayout"/> for where they are and how sure that is.
    /// </summary>
    public sealed class ObeliskBytesLoader : IObeliskBytesLoader
    {
        private const byte MarkerByte = 0xFF;
        private const byte ObeliskObjectType = 57;

        public IReadOnlyList<ObeliskChunk> LoadObeliskChunks(byte[] bytes, IReadOnlyList<PlayerChunk> players)
        {
            if (players.Count == 0)
            {
                return [];
            }

            var firstColor = players[0].Color;

            // The struct of Red, which is in front of the first colour's if that is not Red (a table of players is a struct for every colour).
            var red = players[0].StartIndex - (players[0].Layout.Stride * (int)firstColor);
            var start = red - ObeliskLayout.BeforeFirstPlayer;
            if (start < 0)
            {
                return [];
            }

            if (bytes[start] != MarkerByte || bytes[start + 1] != 0 || bytes[start + 2] != 0)
            {
                return [];
            }

            var count = bytes[start + ObeliskLayout.CountOffset];
            var length = ObeliskLayout.MarkerLength + count;
            if (count < 1 || count > Constants.Obelisks.Max || start + length > red)
            {
                return [];
            }

            return [new ObeliskChunk("Obelisks", bytes.AsSpan(start, length).ToArray(), start, FindPositions(bytes, start, count))];
        }

        /// <summary>
        /// Returns the position of each obelisk. The map has a record for each obelisk: the object type 57 as a 32-bit number, the place of the
        /// obelisk in the list of the objects of the map (16 bits), then the number of the obelisk (32 bits). A hero standing on an obelisk hides
        /// its record, and that obelisk then has no position.
        /// </summary>
        private static IReadOnlyList<Position?> FindPositions(byte[] bytes, int start, int count)
        {
            var objects = MapObjectPositions.Find(bytes, start);
            if (objects.Count == 0)
            {
                return [];
            }

            var positions = new Position?[count];
            for (var i = 0; i + 10 <= start; i++)
            {
                if (bytes[i] != ObeliskObjectType || bytes[i + 1] != 0 || bytes[i + 2] != 0 || bytes[i + 3] != 0)
                {
                    continue;
                }

                var number = bytes[i + 6];
                var place = BitConverter.ToUInt16(bytes, i + 4);
                if (number < count && bytes[i + 7] == 0 && bytes[i + 8] == 0 && bytes[i + 9] == 0 && place > 0 && place < objects.Count && positions[number] == null)
                {
                    positions[number] = objects[place];
                }
            }

            return positions;
        }
    }
}
