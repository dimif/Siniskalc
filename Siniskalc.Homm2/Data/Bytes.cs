namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// How the bytes of a savegame read as the values of a model, which the deserializers and the serializers share. Reading a byte that
    /// is no known value gives the first known one (or <c>null</c> where the value can be absent) and never throws; writing leaves a byte
    /// alone while the model holds what the byte reads as, so a savegame with values the editor does not know is not changed by saving it.
    /// The readers that fall back log it at Trace, whoever calls them (the serializers read to compare, so they log it too).
    /// </summary>
    public static class Bytes
    {
        public const byte Empty = Constants.Empty;

        private static readonly ILogger Logger = LogFactory.For(typeof(Bytes));

        /// <summary>
        /// Gets how many spells a Mage Guild lists in all (four for each of its five levels).
        /// </summary>
        public static int GuildSpellSlots => Constants.Castle.MageGuildLevels * Constants.Castle.MageGuildSlotsPerLevel;

        /// <summary>
        /// Reads a zero terminated text of at most <paramref name="maxLength"/> bytes.
        /// </summary>
        public static string ReadText(byte[] bytes, int offset, int maxLength)
        {
            var length = 0;
            while (length < maxLength && offset + length < bytes.Length && bytes[offset + length] != 0)
            {
                length++;
            }

            return Encoding.Latin1.GetString(bytes, offset, length);
        }

        public static PlayerColor ReadColor(byte code)
        {
            if (AbstractEnumeration.FromCode<PlayerColor>(code) is null)
            {
                Logger.LogTrace("Colour byte {Code:X2} is no player colour, read as neutral", code);
            }

            return PlayerColor.FromByte(code);
        }

        public static byte ColorByte(PlayerColor color, byte existing) => ByteEnumerations.WriteKeeping(color, existing, PlayerColor.Neutral);

        public static Faction ReadFaction(byte code)
        {
            if (AbstractEnumeration.FromCode<Faction>(code) is null)
            {
                Logger.LogTrace("Faction byte {Code:X2} is no faction, read as neutral", code);
            }

            return Faction.FromByte(code);
        }

        public static byte FactionByte(Faction faction, byte existing) => ByteEnumerations.WriteKeeping(faction, existing, Faction.Neutral);

        /// <summary>
        /// Reads a number of a hero, a castle or a tile that is 0xFF when there is none.
        /// </summary>
        public static int? Id(byte code) => code == Empty ? null : code;

        public static byte IdByte(int? id, byte existing) => Id(existing) == id ? existing : id is null ? Empty : (byte)id.Value;

        public static Artifact? ReadArtifact(byte code)
        {
            if (code == Empty)
            {
                return null;
            }

            var artifact = Artifact.FromByte(code);
            if (artifact is null)
            {
                Logger.LogTrace("Artifact code {Code:X2} is unknown, read as the Ultimate Book of Knowledge", code);
            }

            return artifact ?? Artifact.UltimateBookOfKnowledge;
        }

        public static byte ArtifactByte(Artifact? artifact, byte existing) => ReadArtifact(existing) == artifact ? existing : artifact?.Code ?? Empty;

        public static SkillLevel ReadSkillLevel(byte code)
        {
            if (code > SkillLevel.Expert.Code)
            {
                Logger.LogTrace("Skill level byte {Code:X2} is above Expert, read as Expert", code);
            }

            return SkillLevel.Of(Math.Min(code, SkillLevel.Expert.Code));
        }

        /// <summary>
        /// Reads the secondary skills from the table of levels and the table of the order they were learned in: the skills with a level,
        /// the first learned first (the ones with no order after those that have one, by the number of the skill), at most eight.
        /// </summary>
        public static SecondarySkills SecondarySkills(byte[] levels, byte[] order)
        {
            var entries = SecondarySkill.All
                .Where(skill => skill.Code < levels.Length && levels[skill.Code] > 0)
                .OrderBy(skill => order[skill.Code] == 0 ? int.MaxValue : order[skill.Code])
                .ThenBy(skill => skill.Code)
                .Take(Constants.Hero.MaxSecondarySkills)
                .Select(skill => new SkillEntry(skill, ReadSkillLevel(levels[skill.Code])));
            return new SecondarySkills(entries);
        }

        /// <summary>
        /// Reads the spells the savegame marks, from its flags (the first is the flag of spell number 0, which is none).
        /// </summary>
        public static SpellSet<Spell> SpellBook(byte[] flags)
        {
            return new SpellSet<Spell>(Spell.All.Where(spell => spell.Code < flags.Length && flags[spell.Code] != 0));
        }

        /// <summary>
        /// Reads a spell of a Mage Guild, which the savegame lists by its number minus one (0 when the byte is no spell).
        /// </summary>
        public static int GuildSpellNumber(byte stored)
        {
            if (stored >= Constants.Hero.SpellCount && stored != Empty)
            {
                Logger.LogTrace("Mage Guild spell byte {Code:X2} is no spell, read as none", stored);
            }

            return stored < Constants.Hero.SpellCount ? stored + 1 : 0;
        }

        public static byte GuildSpellByte(int number, byte existing) => GuildSpellNumber(existing) == number ? existing : number is >= 1 and <= Constants.Hero.SpellCount ? (byte)(number - 1) : Empty;

        /// <summary>
        /// Reads a coordinate that is 32 bits in the savegame; one the model cannot hold (2^31 or more) reads as the largest it can.
        /// </summary>
        public static int ReadCoordinate(uint value) => (int)Math.Min(value, int.MaxValue);

        /// <summary>
        /// Reads the destination of a hero: two 32 bit numbers, 0xFFFFFFFF for none. Both none is <see cref="MapPosition.None"/>; one of them
        /// none is kept as a -1, so that what the user typed in a box is not lost.
        /// </summary>
        public static MapPosition Destination(uint x, uint y) => x == uint.MaxValue && y == uint.MaxValue
            ? MapPosition.None
            : new MapPosition(x == uint.MaxValue ? Heroes.Constants.Map.NoCoordinate : ReadCoordinate(x), y == uint.MaxValue ? Heroes.Constants.Map.NoCoordinate : ReadCoordinate(y));

        public static FacingDirection Facing(byte value) => Enum.IsDefined((FacingDirection)value) ? (FacingDirection)value : FacingDirection.North;

        public static Numbers IdList(byte[] bytes, int offset, int size) => new(bytes.AsSpan(offset, size).ToArray().Where(b => b != Empty).Select(b => (int)b));

        public static byte[] IdListBytes(Numbers ids, int size, byte[] existing)
        {
            if (new Numbers(existing.Where(b => b != Empty).Select(b => (int)b).Take(size)).Equals(ids))
            {
                return existing;
            }

            var bytes = Enumerable.Repeat(Empty, size).ToArray();
            for (var i = 0; i < ids.Count && i < size; i++)
            {
                bytes[i] = (byte)ids[i];
            }

            return bytes;
        }
    }
}
