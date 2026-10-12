namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// How the bytes of a savegame read as the values of a model, which the deserializers and the serializers share. Reading a byte that
    /// is no known value gives the first known one (or <c>null</c> where the value can be absent) and never throws; writing leaves a byte
    /// alone while the model holds what the byte reads as, so a savegame with values the editor does not know is not changed by saving it.
    /// </summary>
    public static class Homm2Bytes
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

        public static Homm2Color? Color(byte code) => code <= (byte)Homm2Color.Purple ? (Homm2Color)code : null;

        public static byte ColorByte(Homm2Color? color, byte existing) => Color(existing) == color ? existing : color is null ? Empty : (byte)color;

        public static Homm2Race? Race(byte code) => code <= (byte)Homm2Race.Necromancer ? (Homm2Race)code : null;

        public static byte RaceByte(Homm2Race? race, byte existing) => Race(existing) == race ? existing : race is null ? Empty : (byte)race;

        /// <summary>
        /// Reads a number of a hero, a castle or a tile that is 0xFF when there is none.
        /// </summary>
        public static int? Id(byte code) => code == Empty ? null : code;

        public static byte IdByte(int? id, byte existing) => Id(existing) == id ? existing : id is null ? Empty : (byte)id.Value;

        public static Homm2Creature? Creature(byte code) => code == Empty ? null : Homm2Creature.FromByte(code) ?? Homm2Creature.Peasant;

        public static byte CreatureByte(Homm2Creature? creature, byte existing) => Creature(existing) == creature ? existing : creature?.Code ?? Empty;

        public static Homm2Artifact? Artifact(byte code) => code == Empty ? null : Homm2Artifact.FromByte(code) ?? Homm2Artifact.UltimateBookOfKnowledge;

        public static byte ArtifactByte(Homm2Artifact? artifact, byte existing) => Artifact(existing) == artifact ? existing : artifact?.Code ?? Empty;

        public static Homm2SkillLevel SkillLevel(byte code) => (Homm2SkillLevel)Math.Min(code, (byte)Homm2SkillLevel.Expert);

        /// <summary>
        /// Reads the secondary skills from the table of levels and the table of the order they were learned in: the skills with a level,
        /// the first learned first (the ones with no order after those that have one, by the number of the skill), at most eight.
        /// </summary>
        public static Homm2SecondarySkills SecondarySkills(byte[] levels, byte[] order)
        {
            var entries = Homm2SecondarySkill.All
                .Where(skill => skill.Code < levels.Length && levels[skill.Code] > 0)
                .OrderBy(skill => order[skill.Code] == 0 ? int.MaxValue : order[skill.Code])
                .ThenBy(skill => skill.Code)
                .Take(Constants.Hero.MaxSecondarySkills)
                .Select(skill => new Homm2SkillEntry(skill, SkillLevel(levels[skill.Code])));
            return new Homm2SecondarySkills(entries);
        }

        /// <summary>
        /// Reads the stacks of the army, from the table of creature types and the table of counts.
        /// </summary>
        public static Homm2Army Army(IReadOnlyList<byte> types, IReadOnlyList<int> counts)
        {
            return new Homm2Army(Enumerable.Range(0, Constants.ArmySlotCount).Select(i => Creature(types[i]) is { } creature ? new Homm2ArmySlot(creature, counts[i]) : null));
        }

        /// <summary>
        /// Reads the spells the savegame marks, from its flags (the first is the flag of spell number 0, which is none).
        /// </summary>
        public static Homm2SpellBook SpellBook(byte[] flags)
        {
            return new Homm2SpellBook(Homm2Spell.All.Where(spell => spell.Code < flags.Length && flags[spell.Code] != 0));
        }

        /// <summary>
        /// Gets how many spells a Mage Guild lists in all (four for each of its five levels).
        /// </summary>
        public static int GuildSpellSlots => Constants.Castle.MageGuildLevels * Constants.Castle.MageGuildSlotsPerLevel;

        /// <summary>
        /// Reads a spell of a Mage Guild, which the savegame lists by its number minus one (0 when the byte is no spell).
        /// </summary>
        public static int GuildSpellNumber(byte stored) => stored < Constants.Hero.SpellCount ? stored + 1 : 0;

        public static byte GuildSpellByte(int number, byte existing) => GuildSpellNumber(existing) == number ? existing : number is >= 1 and <= Constants.Hero.SpellCount ? (byte)(number - 1) : Empty;

        public static Homm2Position? OptionalPosition(uint x, uint y) => x == uint.MaxValue || y == uint.MaxValue ? null : new Homm2Position((int)Math.Min(x, int.MaxValue), (int)Math.Min(y, int.MaxValue));

        public static Homm2Numbers IdList(byte[] bytes, int offset, int size) => new(bytes.AsSpan(offset, size).ToArray().Where(b => b != Empty).Select(b => (int)b));

        public static byte[] IdListBytes(Homm2Numbers ids, int size, byte[] existing)
        {
            if (new Homm2Numbers(existing.Where(b => b != Empty).Select(b => (int)b).Take(size)).Equals(ids))
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
