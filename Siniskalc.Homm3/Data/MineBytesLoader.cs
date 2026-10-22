namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Finds the table of records of the objects with an owner and a guard by what a record looks like (see <see cref="MineLayout"/>): a run of
    /// records of 62 bytes whose owner is a colour or none, whose guard has a creature and a count in each slot that is used and nothing in the
    /// others, and whose tile is on the map. The mines are the records of the run whose kind is one of the seven mines.
    /// </summary>
    public sealed class MineBytesLoader : IMineBytesLoader
    {
        /// <inheritdoc/>
        public IReadOnlyList<RecordChunk> LoadMineChunks(byte[] bytes)
        {
            var chunks = new List<RecordChunk>();
            var at = 0;
            while (at + MineLayout.RecordSize <= bytes.Length)
            {
                if (!LooksLikeARecord(bytes, at))
                {
                    at++;
                    continue;
                }

                var length = 0;
                while (at + ((length + 1) * MineLayout.RecordSize) <= bytes.Length && LooksLikeARecord(bytes, at + (length * MineLayout.RecordSize)))
                {
                    length++;
                }

                if (length >= MineLayout.MinimumRun)
                {
                    for (var i = 0; i < length; i++)
                    {
                        var start = at + (i * MineLayout.RecordSize);
                        if (IsMine(bytes, start))
                        {
                            chunks.Add(Chunk(bytes, start, chunks.Count));
                        }
                    }

                    at += length * MineLayout.RecordSize;
                }
                else
                {
                    at++;
                }
            }

            return chunks;
        }

        private static RecordChunk Chunk(byte[] bytes, int start, int index)
        {
            var kind = (MineKind)bytes[start + MineLayout.Kind];
            return new RecordChunk(
                MineKinds.NameAt(kind, new MapPosition(bytes[start + MineLayout.X], bytes[start + MineLayout.Y], bytes[start + MineLayout.Level]), hasLevels: true),
                bytes.AsSpan(start, MineLayout.RecordSize).ToArray(),
                start,
                index);
        }

        private static bool IsMine(byte[] bytes, int start) =>
            bytes[start + MineLayout.Kind] <= MineLayout.LastMineKind
            && (bytes[start + MineLayout.X] != 0 || bytes[start + MineLayout.Y] != 0);

        private static bool LooksLikeARecord(byte[] bytes, int start)
        {
            var owner = bytes[start + MineLayout.Owner];
            if (bytes[start + MineLayout.X] >= Constants.Map.MaxCoordinate
                || bytes[start + MineLayout.Y] >= Constants.Map.MaxCoordinate
                || bytes[start + MineLayout.Level] > 1
                || (owner != MineLayout.NoOwner && owner >= PlayerColor.All.Count)
                || bytes[start + MineLayout.Abandoned] > 1)
            {
                return false;
            }

            for (var slot = 0; slot < MineLayout.SlotCount; slot++)
            {
                var type = BitConverter.ToUInt32(bytes, start + MineLayout.Guard.TypeAt(slot));
                var count = BitConverter.ToUInt32(bytes, start + MineLayout.Guard.CountAt(slot));
                var used = type != uint.MaxValue;
                if (used ? type > MineLayout.MaxCreatureCode || count == 0 || count > MineLayout.MaxCount : count != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
