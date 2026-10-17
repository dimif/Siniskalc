namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Siniskalc.Core.Models;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// How the bytes of a savegame read as the values of a model, which the deserializers and the serializers share. Reading a byte that
    /// is no known value gives the first known one (or <c>null</c> where the value can be absent) and never throws; writing leaves a byte
    /// alone while the model holds what the byte reads as, so a savegame with values the editor does not know is not changed by saving it.
    /// </summary>
    public static class Bytes
    {
        public const byte Empty = Constants.Empty;

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

        // TODO:LOG warning: a colour byte that is no player colour was read as nobody.
        public static Color? ReadColor(byte code) => code <= (byte)Color.Purple ? (Color)code : null;

        public static byte ColorByte(Color? color, byte existing) => ReadColor(existing) == color ? existing : color is null ? Empty : (byte)color;

        // TODO:LOG warning: a faction byte that is no faction was read as none.
        public static Faction? ReadFaction(byte code) => code <= (byte)Faction.Necromancer ? (Faction)code : null;

        public static byte FactionByte(Faction? faction, byte existing) => ReadFaction(existing) == faction ? existing : faction is null ? Empty : (byte)faction;

        /// <summary>
        /// Reads a number of a hero, a castle or a tile that is 0xFF when there is none.
        /// </summary>
        public static int? Id(byte code) => code == Empty ? null : code;

        public static byte IdByte(int? id, byte existing) => Id(existing) == id ? existing : id is null ? Empty : (byte)id.Value;

        // TODO:LOG warning: a creature code the editor has no name for was read as a Peasant.
        public static Creature? ReadCreature(byte code) => code == Empty ? null : Creature.FromByte(code) ?? Creature.Peasant;

        public static byte CreatureByte(Creature? creature, byte existing) => ReadCreature(existing) == creature ? existing : creature?.Code ?? Empty;

        // TODO:LOG warning: an artifact code the editor has no name for was read as the Ultimate Book of Knowledge.
        public static Artifact? ReadArtifact(byte code) => code == Empty ? null : Artifact.FromByte(code) ?? Artifact.UltimateBookOfKnowledge;

        public static byte ArtifactByte(Artifact? artifact, byte existing) => ReadArtifact(existing) == artifact ? existing : artifact?.Code ?? Empty;

        // TODO:LOG warning: a skill level above Expert was read as Expert.
        public static SkillLevel ReadSkillLevel(byte code) => (SkillLevel)Math.Min(code, (byte)SkillLevel.Expert);

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
        /// Reads the stacks of the army, from the table of creature types and the table of counts.
        /// </summary>
        public static Army<Creature> Army(IReadOnlyList<byte> types, IReadOnlyList<int> counts)
        {
            return new Army<Creature>(Enumerable.Range(0, Constants.ArmySlotCount).Select(i => ReadCreature(types[i]) is { } creature ? new ArmySlot<Creature>(creature, counts[i]) : null));
        }

        /// <summary>
        /// Reads the spells the savegame marks, from its flags (the first is the flag of spell number 0, which is none).
        /// </summary>
        public static SpellSet<Spell> SpellBook(byte[] flags)
        {
            return new SpellSet<Spell>(Spell.All.Where(spell => spell.Code < flags.Length && flags[spell.Code] != 0));
        }

        /// <summary>
        /// Gets how many spells a Mage Guild lists in all (four for each of its five levels).
        /// </summary>
        public static int GuildSpellSlots => Constants.Castle.MageGuildLevels * Constants.Castle.MageGuildSlotsPerLevel;

        /// <summary>
        /// Reads a spell of a Mage Guild, which the savegame lists by its number minus one (0 when the byte is no spell).
        /// </summary>
        // TODO:LOG warning: a Mage Guild spell byte that is no spell was read as none.
        public static int GuildSpellNumber(byte stored) => stored < Constants.Hero.SpellCount ? stored + 1 : 0;

        public static byte GuildSpellByte(int number, byte existing) => GuildSpellNumber(existing) == number ? existing : number is >= 1 and <= Constants.Hero.SpellCount ? (byte)(number - 1) : Empty;

        /// <summary>
        /// Reads a coordinate that is 32 bits in the savegame; one the model cannot hold (2^31 or more) reads as the largest it can.
        /// </summary>
        public static int ReadCoordinate(uint value) => (int)Math.Min(value, int.MaxValue);

        /// <summary>
        /// Writes a stack of an army (a creature byte and a 16-bit count) as the model says, leaving the bytes alone while they read as it does.
        /// </summary>
        public static void WriteArmySlot(byte[] bytes, int typeOffset, int countOffset, ArmySlot<Creature>? wanted, string name)
        {
            var currentType = ReadCreature(bytes[typeOffset]);
            var currentCount = BitConverter.ToUInt16(bytes, countOffset);

            if (wanted is null)
            {
                if (currentType is not null)
                {
                    bytes[typeOffset] = Empty;
                    ByteBufferHelper.WriteUInt16(bytes, countOffset, 0);
                }

                return;
            }

            if (currentType == wanted.Creature && currentCount == wanted.Count)
            {
                return;
            }

            RangeGuard.ThrowIfOutOfRange<long>(wanted.Count, 0, Constants.MaxCreatureCount, name);
            bytes[typeOffset] = CreatureByte(wanted.Creature, bytes[typeOffset]);
            ByteBufferHelper.WriteUInt16(bytes, countOffset, (ushort)wanted.Count);
        }

        public static Position? OptionalPosition(uint x, uint y) => x == uint.MaxValue || y == uint.MaxValue ? null : new Position(ReadCoordinate(x), ReadCoordinate(y));

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
