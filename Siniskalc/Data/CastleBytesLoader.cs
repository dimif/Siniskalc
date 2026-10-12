namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Siniskalc.Entities;

    /// <summary>
    /// Finds the castles (towns) in a savefile and loads each as a <see cref="CastleChunk"/> named after the castle.
    /// </summary>
    /// <remarks>
    /// Castles are not in a table, so they are found by what they look like: a four byte prefix that ends with a 00 and
    /// whose third byte (the level of the Mage Guild) is small, the length of the name, a printable name, and around it a
    /// valid owner, faction, garrison and the counts of Mage Guild spells that come before the build masks.
    /// The first two bytes of the prefix vary from castle to castle.
    /// </remarks>
    public sealed class CastleBytesLoader : ICastleBytesLoader
    {
        private const int MaxCreatureCode = 200;
        private const int MaxNameLength = 48;
        private const int MaxMageGuildLevel = 5;
        private const int MaxSpellCount = 8;

        private static readonly int MaxFactionCode = Faction.All.Where(f => f != Faction.Neutral).Max(f => f.Code);

        private readonly Encoding encoding;

        public CastleBytesLoader(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public IReadOnlyList<CastleChunk> LoadCastleChunks(byte[] bytes)
        {
            var chunks = new List<CastleChunk>();
            var minimum = 0;

            // Position of the length of the name: a four byte prefix, then <length:u16>.
            for (var i = CastleLayout.NamePrefix; i + CastleLayout.NameLength + 2 < bytes.Length; i++)
            {
                if (bytes[i + 2] > MaxMageGuildLevel || bytes[i + 3] != 0x00)
                {
                    continue;
                }

                var start = i + CastleLayout.NamePrefixLength - CastleLayout.NameLength;
                if (start < minimum)
                {
                    continue;
                }

                var length = BitConverter.ToUInt16(bytes, i + CastleLayout.NamePrefixLength);
                var nameEnd = start + CastleLayout.Name + length;
                var tailEnd = nameEnd + CastleLayout.ChunkTailFromNameEnd;
                if (start < 0 || length == 0 || length > MaxNameLength || tailEnd > bytes.Length || !IsCastle(bytes, start, nameEnd, length))
                {
                    continue;
                }

                // The lists of the guild's spells follow the masks; a savefile that stops before them gives a shorter chunk.
                var end = Math.Min(nameEnd + CastleLayout.ChunkEndFromNameEnd, bytes.Length);

                var chunk = bytes.AsSpan(start, end - start).ToArray();
                chunks.Add(new CastleChunk(this.encoding.GetString(bytes, start + CastleLayout.Name, length), chunk, start));
                minimum = end;
                i = end - 1;
            }

            return chunks.AsReadOnly();
        }

        private static bool IsCastle(byte[] bytes, int start, int nameEnd, int nameLength)
        {
            var owner = bytes[start + CastleLayout.Owner];
            if (owner >= Constants.Player.ColorCount && owner != Constants.ByteOff)
            {
                return false;
            }

            if (bytes[start + CastleLayout.BuiltThisTurn] > 1 || bytes[start + CastleLayout.Faction] > MaxFactionCode)
            {
                return false;
            }

            for (var i = 0; i < nameLength; i++)
            {
                if (bytes[start + CastleLayout.Name + i] < 0x20 || bytes[start + CastleLayout.Name + i] > 0x7E)
                {
                    return false;
                }
            }

            for (var slot = 0; slot < Constants.CreatureSlotCount; slot++)
            {
                var type = BitConverter.ToInt32(bytes, start + CastleLayout.GarrisonTypes + (slot * 4));
                var count = BitConverter.ToInt32(bytes, start + CastleLayout.GarrisonCounts + (slot * 4));
                if ((type != -1 && (type < 0 || type > MaxCreatureCode)) || count < 0)
                {
                    return false;
                }
            }

            for (var level = 0; level < CastleLayout.SpellLevelCount; level++)
            {
                if (bytes[nameEnd + CastleLayout.SpellCountsFromNameEnd + level] > MaxSpellCount)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
