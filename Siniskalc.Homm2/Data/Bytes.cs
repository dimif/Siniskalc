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
        /// <summary>
        /// Empty.
        /// </summary>
        public const byte Empty = Constants.Empty;

        private static readonly ILogger Logger = LogFactory.For(typeof(Bytes));

        /// <summary>
        /// Gets how many spells a Mage Guild lists in all (four for each of its five levels).
        /// </summary>
        public static int GuildSpellSlots => Constants.Castle.MageGuildLevels * Constants.Castle.MageGuildSlotsPerLevel;

        /// <summary>
        /// Reads a zero terminated text of at most <paramref name="maxLength"/> bytes.
        /// </summary>
        /// <param name="bytes">The bytes of the savegame.</param>
        /// <param name="offset">Where the text begins.</param>
        /// <param name="maxLength">The most bytes the text can have.</param>
        /// <returns>The text, decoded as Latin-1.</returns>
        public static string ReadText(byte[] bytes, int offset, int maxLength)
        {
            var length = 0;
            while (length < maxLength && offset + length < bytes.Length && bytes[offset + length] != 0)
            {
                length++;
            }

            return Encoding.Latin1.GetString(bytes, offset, length);
        }

        /// <summary>
        /// Reads the colour of a player from its byte; a byte that is no colour reads as neutral.
        /// </summary>
        /// <param name="code">The byte.</param>
        /// <returns>The colour.</returns>
        public static PlayerColor ReadColor(byte code)
        {
            if (AbstractEnumeration.FromCode<PlayerColor>(code) is null)
            {
                Logger.LogTrace("Colour byte {Code:X2} is no player colour, read as neutral", code);
            }

            return PlayerColor.FromByte(code);
        }

        /// <summary>
        /// Gets the byte that holds a colour, keeping the existing byte while it reads as that colour.
        /// </summary>
        /// <param name="color">The colour.</param>
        /// <param name="existing">The byte that is in the savegame now.</param>
        /// <returns>The byte to write.</returns>
        public static byte ColorByte(PlayerColor color, byte existing) => ByteEnumerations.WriteKeeping(color, existing, PlayerColor.Neutral);

        /// <summary>
        /// Reads the faction of a castle from its byte; a byte that is no faction reads as the neutral faction.
        /// </summary>
        /// <param name="code">The byte.</param>
        /// <param name="catalog">The catalog of the factions.</param>
        /// <returns>The faction.</returns>
        public static Faction ReadFaction(byte code, GameCatalog catalog)
        {
            if (catalog.Factions.Find(code) is null)
            {
                Logger.LogTrace("Faction byte {Code:X2} is no faction, read as neutral", code);
            }

            return catalog.Factions.Find(code) ?? catalog.NeutralFaction;
        }

        /// <summary>
        /// Gets the byte that holds a faction, keeping the existing byte while it reads as that faction.
        /// </summary>
        /// <param name="faction">The faction.</param>
        /// <param name="existing">The byte that is in the savegame now.</param>
        /// <returns>The byte to write; <see cref="Constants.Empty"/> for the neutral faction.</returns>
        public static byte FactionByte(Faction faction, byte existing) => ReadFaction(existing, faction.Catalog) == faction ? existing : faction.IsNeutral ? Empty : faction.Code;

        /// <summary>
        /// Reads a number of a hero, a castle or a tile that is 0xFF when there is none.
        /// </summary>
        /// <param name="code">The byte.</param>
        /// <returns>The number, or <see langword="null"/> for 0xFF.</returns>
        public static int? Id(byte code) => code == Empty ? null : code;

        /// <summary>
        /// Gets the byte that holds a number of a hero, a castle or a tile, keeping the existing byte while it reads as that number.
        /// </summary>
        /// <param name="id">The number, or <see langword="null"/> for none.</param>
        /// <param name="existing">The byte that is in the savegame now.</param>
        /// <returns>The byte to write; <see cref="Constants.Empty"/> for none.</returns>
        public static byte IdByte(int? id, byte existing) => Id(existing) == id ? existing : id is null ? Empty : (byte)id.Value;

        /// <summary>
        /// Reads an artifact from its byte; a byte that is no artifact reads as the stand-in artifact of the catalog.
        /// </summary>
        /// <param name="code">The byte.</param>
        /// <param name="catalog">The catalog of the artifacts.</param>
        /// <returns>The artifact, or <see langword="null"/> for an empty slot.</returns>
        public static Artifact? ReadArtifact(byte code, GameCatalog catalog)
        {
            if (code == Empty)
            {
                return null;
            }

            var artifact = catalog.Artifacts.Find(code);
            if (artifact is null)
            {
                Logger.LogTrace("Artifact code {Code:X2} is unknown, read as {StandIn} and kept until changed", code, catalog.FallbackArtifact.Name);
            }

            return artifact ?? catalog.FallbackArtifact;
        }

        /// <summary>
        /// Gets the byte that holds an artifact, keeping the existing byte while it reads as that artifact.
        /// </summary>
        /// <param name="artifact">The artifact, or <see langword="null"/> for none.</param>
        /// <param name="existing">The byte that is in the savegame now.</param>
        /// <param name="catalog">The catalog of the artifacts.</param>
        /// <returns>The byte to write; <see cref="Constants.Empty"/> for none.</returns>
        public static byte ArtifactByte(Artifact? artifact, byte existing, GameCatalog catalog) => ReadArtifact(existing, catalog) == artifact ? existing : artifact?.Code ?? Empty;

        /// <summary>
        /// Reads the level of a secondary skill from its byte; a byte above Expert reads as Expert.
        /// </summary>
        /// <param name="code">The byte.</param>
        /// <returns>The level.</returns>
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
        /// <param name="levels">The level of each skill, by the number of the skill.</param>
        /// <param name="order">The place in which each skill was learned, by the number of the skill.</param>
        /// <param name="catalog">The catalog of the skills.</param>
        /// <returns>The skills.</returns>
        public static SecondarySkills SecondarySkills(byte[] levels, byte[] order, GameCatalog catalog)
        {
            var entries = catalog.Skills
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
        /// <param name="flags">One byte for each spell by its number; non-zero means the hero knows it.</param>
        /// <param name="catalog">The catalog of the spells.</param>
        /// <returns>The spells.</returns>
        public static SpellSet<Spell> SpellBook(byte[] flags, GameCatalog catalog)
        {
            return new SpellSet<Spell>(catalog.Spells.Where(spell => spell.Code < flags.Length && flags[spell.Code] != 0));
        }

        /// <summary>
        /// Reads a spell of a Mage Guild, which the savegame lists by its number minus one (0 when the byte is no spell).
        /// </summary>
        /// <param name="stored">The byte of the savegame.</param>
        /// <returns>The number of the spell, from 1, or 0 for none.</returns>
        public static int GuildSpellNumber(byte stored)
        {
            if (stored >= Constants.Hero.SpellCount && stored != Empty)
            {
                Logger.LogTrace("Mage Guild spell byte {Code:X2} is no spell, read as none", stored);
            }

            return stored < Constants.Hero.SpellCount ? stored + 1 : 0;
        }

        /// <summary>
        /// Gets the byte that holds a spell of a Mage Guild, keeping the existing byte while it reads as that spell.
        /// </summary>
        /// <param name="spell">The spell, or <see langword="null"/> for none.</param>
        /// <param name="existing">The byte that is in the savegame now.</param>
        /// <param name="catalog">The catalog of the spells.</param>
        /// <returns>The byte to write; <see cref="Constants.Empty"/> for none.</returns>
        public static byte GuildSpellByte(Spell? spell, byte existing, GameCatalog catalog) =>
            catalog.Spells.Find((byte)GuildSpellNumber(existing)) == spell ? existing : spell is null ? Empty : (byte)(spell.Code - 1);

        /// <summary>
        /// Reads a coordinate that is 32 bits in the savegame; one the model cannot hold (2^31 or more) reads as the largest it can.
        /// </summary>
        /// <param name="value">The 32 bit number of the savegame.</param>
        /// <returns>The coordinate.</returns>
        public static int ReadCoordinate(uint value) => (int)Math.Min(value, int.MaxValue);

        /// <summary>
        /// Reads the destination of a hero: two 32 bit numbers, 0xFFFFFFFF for none. Both none is <see cref="MapPosition.None"/>; one of them
        /// none is kept as a -1, so that what the user typed in a box is not lost.
        /// </summary>
        /// <param name="x">The column, 0xFFFFFFFF for none.</param>
        /// <param name="y">The row, 0xFFFFFFFF for none.</param>
        /// <returns>The destination.</returns>
        public static MapPosition Destination(uint x, uint y) => x == uint.MaxValue && y == uint.MaxValue
            ? MapPosition.None
            : new MapPosition(x == uint.MaxValue ? Heroes.Constants.Map.NoCoordinate : ReadCoordinate(x), y == uint.MaxValue ? Heroes.Constants.Map.NoCoordinate : ReadCoordinate(y));

        /// <summary>
        /// Reads the direction a hero faces from its byte; a byte that is no direction reads as north.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <returns>The direction.</returns>
        public static FacingDirection Facing(byte value) => Enum.IsDefined((FacingDirection)value) ? (FacingDirection)value : FacingDirection.North;

        /// <summary>
        /// Reads a list of numbers of heroes or castles, one byte each, leaving out the empty ones.
        /// </summary>
        /// <param name="bytes">The bytes of the savegame.</param>
        /// <param name="offset">Where the list begins.</param>
        /// <param name="size">The number of bytes of the list.</param>
        /// <returns>The numbers, in the order of the list.</returns>
        public static Numbers IdList(byte[] bytes, int offset, int size) => new(bytes.AsSpan(offset, size).ToArray().Where(b => b != Empty).Select(b => (int)b));

        /// <summary>
        /// Gets the bytes that hold a list of numbers, keeping the existing bytes while they read as the same numbers.
        /// </summary>
        /// <param name="ids">The numbers.</param>
        /// <param name="size">The number of bytes of the list.</param>
        /// <param name="existing">The bytes that are in the savegame now.</param>
        /// <returns>The bytes to write: the numbers in order, then <see cref="Constants.Empty"/> up to the size.</returns>
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
