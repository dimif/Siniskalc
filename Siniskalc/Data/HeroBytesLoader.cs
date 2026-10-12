namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Siniskalc.Entities;

    /// <summary>
    /// Loads hero structs from a savefile as <see cref="HeroChunk"/> chunks.
    /// </summary>
    public sealed class HeroBytesLoader : IHeroBytesLoader
    {
        /// <summary>
        /// Where the faction byte is in a hero struct; a match of the hero regex starts at the hero position.
        /// </summary>
        private const int FactionOffset = 26;

        /// <summary>
        /// How far into the savefile the heroes of the game start at the earliest: before it there may be campaign carry-over heroes.
        /// </summary>
        private const int CarryOverHeroesLength = 30000;

        /// <summary>After the first hero, the next one is searched for this far from where the last one ended.</summary>
        private const int NextHeroSearchLength = 5000;

        // Offsets from the hero position, in the order of the struct.
        private const int SkillsUsedAt = 69;
        private const int NameAt = 195;
        private const int NameLength = 13;
        private const int SkillSlotsAt = NameAt + NameLength;
        private const int SkillBlockLength = 56;
        private const int ClassicSkillCount = 28;
        private const int HotaPrimarySkillCount = 30;
        private const int SpellsAt = SkillSlotsAt + SkillBlockLength + 4;
        private const int SpellBytes = 140;
        private const int EquipmentAt = SpellsAt + SpellBytes;
        private const int EquipmentSlotSize = 8;
        private const int SideSlotLength = 8;
        private const int BackpackSlotCount = 64;
        private const int BackpackLength = BackpackSlotCount * 8;
        private const int CombinationTailLength = 10;
        private const int HotaTailGap = 36;

        /// <summary>
        /// The bytes after the hero struct that belong to the same hero in a Shadow of Death savefile (a hero record is the struct and these
        /// twelve bytes). They hold, among others, the flag of a hero that was put to sleep. Horn of the Abyss has twelve bytes there too, but
        /// they are laid out differently; the other versions have not been seen.
        /// </summary>
        private const int ShadowOfDeathTailLength = 12;
        private const byte MaxSkillLevel = 0x1C;

        private static readonly HeroShape RoeShape = new(18, false, false, false);
        private static readonly HeroShape AbNewShape = new(18, true, false, false);
        private static readonly HeroShape GenericShape = new(19, false, false, true);
        private static readonly HeroShape ShadowOfDeathShape = new(19, false, false, true, ShadowOfDeathTailLength);
        private static readonly HeroShape HotaShape = new(19, false, true, true);

        private readonly Encoding saveEncoding;

        public HeroBytesLoader(Encoding encoding)
        {
            this.saveEncoding = encoding;
        }

        /// <param name="bytes">The savefile bytes.</param>
        /// <param name="gameVersion">The version of the savefile, as <see cref="SavegameVersionDetector.Detect"/> tells.</param>
        public IReadOnlyList<HeroChunk> LoadHeroChunks(byte[] bytes, GameVersion gameVersion)
        {
            var shape = GetHeroShape(gameVersion);
            if (shape == null || bytes.Length <= CarryOverHeroesLength)
            {
                return Array.Empty<HeroChunk>();
            }

            var chunks = new List<HeroChunk>();

            // jump over potential campaign carry-over heroes, stored in savefile with their
            // original armies+artifacts; the structs used by game come later.
            var position = CarryOverHeroesLength;

            while (position < bytes.Length)
            {
                var searchLength = chunks.Count == 0
                    ? bytes.Length - position
                    : Math.Min(NextHeroSearchLength, bytes.Length - position);
                if (!TryFindHero(bytes, shape, position, position + searchLength, out var index, out var length))
                {
                    break;
                }

                if (this.TryGetCandidateName(bytes, index, shape, out var name))
                {
                    var chunkBytes = bytes.AsSpan(index, length).ToArray();
                    var biography = this.ReadBiography(bytes, index + FactionOffset, out var headerBytes, out var headerIndex);
                    chunks.Add(new HeroChunk(name, chunkBytes, index, biography, headerBytes, headerIndex));
                    position = index + length;
                }
                else
                {
                    position = index + 1;
                }
            }

            return chunks.AsReadOnly();
        }

        /// <summary>
        /// Finds the first position at or after <paramref name="from"/> where a hero struct that ends before <paramref name="limit"/> starts.
        /// </summary>
        private static bool TryFindHero(byte[] bytes, HeroShape shape, int from, int limit, out int index, out int length)
        {
            for (index = from; index + shape.MinimumLength <= limit; index++)
            {
                length = MatchLength(bytes, index, limit, shape);
                if (length > 0)
                {
                    return true;
                }
            }

            index = -1;
            length = 0;
            return false;
        }

        /// <summary>
        /// Returns the length of the hero struct that starts at <paramref name="at"/>, or 0 when the bytes do not have its shape.
        /// </summary>
        private static int MatchLength(byte[] b, int at, int limit, HeroShape shape)
        {
            // Skills used: 0 to 28 as a 32-bit number.
            if (b[at + SkillsUsedAt] > MaxSkillLevel || b[at + SkillsUsedAt + 1] != 0 || b[at + SkillsUsedAt + 2] != 0 || b[at + SkillsUsedAt + 3] != 0)
            {
                return 0;
            }

            // Name: 12 characters, the first of them printable, then a 00.
            if (b[at + NameAt] <= 0x20 || b[at + NameAt + NameLength - 1] != 0)
            {
                return 0;
            }

            var skillSlotsAt = at + SkillSlotsAt;
            var spellsAt = at + SpellsAt;
            var equipmentAt = at + EquipmentAt;
            var backpackAt = equipmentAt + (shape.EquipmentSlots * EquipmentSlotSize) + (shape.SideSlot ? SideSlotLength : 0);
            var tailAt = backpackAt + BackpackLength;
            var end = tailAt + (shape.CombinationTail ? CombinationTailLength : 0);

            if (!AllIn(b, skillSlotsAt, shape.Hota ? HotaPrimarySkillCount : ClassicSkillCount, 0x00, 0x03)
                || (!shape.Hota && !AllIn(b, skillSlotsAt + ClassicSkillCount, ClassicSkillCount, 0x00, MaxSkillLevel))
                || !AllIn(b, spellsAt, SpellBytes, 0x00, 0x01)
                || !AreEquipmentSlots(b, equipmentAt, shape.EquipmentSlots)
                || !AreBackpackSlots(b, backpackAt))
            {
                return 0;
            }

            if (shape.CombinationTail
                && (!AllIn(b, tailAt + 1, 6, 0x00, 0x01) || b[tailAt + 7] > 0x02 || b[tailAt + 8] > 0x01 || b[tailAt + 9] > 0x05))
            {
                return 0;
            }

            if (!shape.Hota)
            {
                return end + shape.TailLength <= limit ? end + shape.TailLength - at : 0;
            }

            // The skill slots after the gap are 30 bytes, or 29 in the older HotA saves.
            var hotaSkillsAt = end + HotaTailGap;
            if (hotaSkillsAt + 30 <= limit && AllIn(b, hotaSkillsAt, 30, 0x00, MaxSkillLevel))
            {
                return hotaSkillsAt + 30 - at;
            }

            return hotaSkillsAt + 29 <= limit && AllIn(b, hotaSkillsAt, 29, 0x00, MaxSkillLevel) ? hotaSkillsAt + 29 - at : 0;
        }

        private static bool AllIn(byte[] bytes, int start, int count, byte minimum, byte maximum)
        {
            for (var i = start; i < start + count; i++)
            {
                if (bytes[i] < minimum || bytes[i] > maximum)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// An equipment slot is 8 bytes: FF FF FF FF and four more, or a byte, 00 00 00 and four more.
        /// </summary>
        private static bool AreEquipmentSlots(byte[] bytes, int start, int count)
        {
            for (var slot = 0; slot < count; slot++)
            {
                var at = start + (slot * EquipmentSlotSize);
                if (!IsFfDword(bytes, at) && !IsByteThenZeros(bytes, at))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// A backpack slot is two 32-bit numbers, each FF FF FF FF or a byte and three zeros.
        /// </summary>
        private static bool AreBackpackSlots(byte[] bytes, int start)
        {
            for (var at = start; at < start + BackpackLength; at += 4)
            {
                if (!IsFfDword(bytes, at) && !IsByteThenZeros(bytes, at))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsFfDword(byte[] bytes, int at) => bytes[at] == 0xFF && bytes[at + 1] == 0xFF && bytes[at + 2] == 0xFF && bytes[at + 3] == 0xFF;

        private static bool IsByteThenZeros(byte[] bytes, int at) => bytes[at + 1] == 0 && bytes[at + 2] == 0 && bytes[at + 3] == 0;

        /// <summary>
        /// Takes the name: the characters up to the first control character, with only zeros after it. A blank name (all FF) is none.
        /// </summary>
        private static bool TryGetNameBytes(byte[] bytes, int at, [NotNullWhen(true)] out byte[]? nameBytes)
        {
            nameBytes = null;
            var length = 1;
            while (length < NameLength && bytes[at + length] >= 0x1A)
            {
                length++;
            }

            for (var i = length; i < NameLength; i++)
            {
                if (bytes[at + i] != 0)
                {
                    return false;
                }
            }

            if (AllIn(bytes, at, length, 0xFF, 0xFF))
            {
                return false;
            }

            nameBytes = bytes.AsSpan(at, length).ToArray();
            return true;
        }

        /// <summary>
        /// Rejects equipment that only looks like it: blank slots, and slots that are not artifacts, scrolls or war machines.
        /// Blank spots are FF FF FF FF XY XY XY XY, artifacts XY 00 00 00 FF FF FF FF, scrolls XY 00 00 00 00 00 00 00 and
        /// war machines XY 00 00 00 XY XY 00 00. The section is good when its last slots, from one that does not start with a zero,
        /// are all of those.
        /// </summary>
        private static bool IsEquipmentSection(byte[] bytes, int start, int slotCount)
        {
            for (var slot = slotCount - 1; slot >= 0; slot--)
            {
                var at = start + (slot * EquipmentSlotSize);
                if (!IsFfDword(bytes, at) && !(IsByteThenZeros(bytes, at) && IsArtifactScrollOrMachine(bytes, at + 4)))
                {
                    return false;
                }

                if (bytes[at] != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsArtifactScrollOrMachine(byte[] bytes, int at) =>
            (bytes[at] == 0 && bytes[at + 1] == 0 && bytes[at + 2] == 0 && bytes[at + 3] == 0)
            || IsFfDword(bytes, at)
            || (bytes[at + 2] == 0 && bytes[at + 3] == 0);

        private static HeroShape? GetHeroShape(GameVersion variant)
        {
            return variant switch
            {
                GameVersion.RestorationOfErathia => RoeShape,
                GameVersion.ArmageddonsBlade => AbNewShape,
                GameVersion.ShadowOfDeath => ShadowOfDeathShape,
                GameVersion.HeroesChronicles => GenericShape,
                GameVersion.HornOfTheAbyss => HotaShape,
                _ => null,
            };
        }

        /// <summary>
        /// Reads the biography, which sits immediately before the faction byte of a hero: a 4-byte length, then that many
        /// characters. Returns null if there is no biography; otherwise also gives the hero header in front of it.
        /// </summary>
        private string? ReadBiography(byte[] bytes, int factionIndex, out byte[]? headerBytes, out int headerIndex)
        {
            headerBytes = null;
            headerIndex = -1;

            // Without a biography the byte in front of the faction is the last of the unknown bytes, so only a
            // length field that agrees with the distance walked back is trusted.
            var start = factionIndex;
            while (start > 4 && bytes[start - 1] != 0)
            {
                start--;
            }

            var length = factionIndex - start;
            if (length == 0 || start < 4 || BitConverter.ToInt32(bytes, start - 4) != length)
            {
                return null;
            }

            // The struct is a fixed 26 bytes (position, unknown bytes and the length) in front of the biography.
            var positionIndex = start - FactionOffset;
            if (positionIndex < 0)
            {
                return null;
            }

            try
            {
                var biography = this.saveEncoding.GetString(bytes, start, length);
                headerBytes = bytes.AsSpan(positionIndex, HeroChunk.HeaderLength).ToArray();
                headerIndex = positionIndex;
                return biography;
            }
            catch (DecoderFallbackException)
            {
                // TODO:LOG info: a hero biography at {start} is not text in the encoding of the savefile; the hero is read without one.
                return null;
            }
        }

        private bool TryGetCandidateName(byte[] bytes, int index, HeroShape shape, [NotNullWhen(true)] out string? name)
        {
            name = null;
            if (!TryGetNameBytes(bytes, index + NameAt, out var nameBytes) || !IsEquipmentSection(bytes, index + EquipmentAt, shape.EquipmentSlots))
            {
                return false;
            }

            try
            {
                name = this.saveEncoding.GetString(nameBytes);
                return name.Length > 0;
            }
            catch (DecoderFallbackException)
            {
                // TODO:LOG debug: bytes at {index} look like a hero but their name is not text in the encoding of the savefile; the scan goes on.
                return false;
            }
        }

        /// <summary>
        /// What differs in the hero struct between game versions.
        /// </summary>
        private sealed record HeroShape(int EquipmentSlots, bool SideSlot, bool Hota, bool CombinationTail, int TailLength = 0)
        {
            /// <summary>Gets the length of the shortest struct of this shape.</summary>
            public int MinimumLength => EquipmentAt + (this.EquipmentSlots * EquipmentSlotSize) + (this.SideSlot ? SideSlotLength : 0)
                + BackpackLength + (this.CombinationTail ? CombinationTailLength : 0) + (this.Hota ? HotaTailGap + 29 : 0) + this.TailLength;
        }
    }
}
