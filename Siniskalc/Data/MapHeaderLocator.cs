namespace Siniskalc.Data
{
    using System;
    using System.Text;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the map's name, description and difficulty. The difficulty is the one the map was made with, which is not the difficulty the game was started at.
    /// </summary>
    /// <remarks>
    /// The savefile starts with the map's header: the name and the description, each a 16-bit length and then the text, and then
    /// a byte with the map's difficulty, from 0 for Pawn to 4 for King. The header's start differs between versions (the name is at
    /// byte 95 in HotA and 58 in Shadow of Death), so the name is found by its shape. It was checked by changing a map's difficulty
    /// in the map editor from Impossible (4) to Expert (3): cas_mod and map_diff_difficulty differ in this byte only (and in the
    /// file name, which changes the length of the rest).
    /// The width of the map is the 32-bit integer 5 bytes before the name (36, 72, 108, ... for Small, Medium, Large, ...), and the byte
    /// right before the name says whether the map has an underground level. The layout is that of the map file's header; the sizes
    /// 36, 72 and 144 were seen in the samples, and the underground byte is 0 in the one single-level sample custom_bios.
    /// </remarks>
    public sealed class MapHeaderLocator : IMapHeaderLocator
    {
        /// <summary>
        /// How far in front of the name the 32-bit width of the map is.
        /// </summary>
        internal const int SizeBeforeName = 5;

        private const int SearchLength = 200;
        private const int MaxNameLength = 60;

        private static readonly GameDifficulty[] Difficulties = [GameDifficulty.Pawn, GameDifficulty.Knight, GameDifficulty.Rook, GameDifficulty.Queen, GameDifficulty.King];

        /// <summary>
        /// Returns the map's header, or <c>null</c> when it cannot be found.
        /// </summary>
        public MapHeader? Find(byte[] bytes, GameVersion gameVersion)
        {
            var location = this.Locate(bytes, gameVersion);
            if (location == null)
            {
                return null;
            }

            var (size, hasUnderground) = ReadSize(bytes, location.NameLengthAt);
            return new MapHeader(
                ReadText(bytes, location.NameLengthAt + 2, location.NameLength),
                ReadText(bytes, location.DescriptionLengthAt + 2, location.DescriptionLength),
                Difficulties[bytes[location.DifficultyAt]],
                size,
                hasUnderground);
        }

        /// <summary>
        /// Returns where the map's header is, or <c>null</c> when it cannot be found.
        /// </summary>
        public MapHeaderLocation? Locate(byte[] bytes, GameVersion gameVersion)
        {
            if (!gameVersion.HasGameSetup())
            {
                return null;
            }

            var last = Math.Min(SearchLength, bytes.Length - 4);
            for (var i = 0; i < last; i++)
            {
                var nameLength = ReadLength(bytes, i);
                if (nameLength < 1 || nameLength > MaxNameLength || !IsTextAt(bytes, i + 2, nameLength))
                {
                    continue;
                }

                var descriptionStart = i + 2 + nameLength;
                if (descriptionStart + 2 > bytes.Length)
                {
                    continue;
                }

                var descriptionLength = ReadLength(bytes, descriptionStart);
                var difficultyAt = descriptionStart + 2 + descriptionLength;
                if (difficultyAt < bytes.Length && IsTextAt(bytes, descriptionStart + 2, descriptionLength) && bytes[difficultyAt] <= Constants.Map.MaxDifficulty)
                {
                    return new MapHeaderLocation(i, nameLength, descriptionStart, descriptionLength);
                }
            }

            return null;
        }

        // Before the name come a byte for whether the map has players, the map's width as a 32-bit integer and a byte for whether
        // it has an underground level, like in the header of the map file.
        private static (int Size, bool HasUnderground) ReadSize(byte[] bytes, int nameLengthAt)
        {
            if (nameLengthAt < SizeBeforeName)
            {
                return (0, false);
            }

            var size = BitConverter.ToInt32(bytes, nameLengthAt - SizeBeforeName);
            return (size is > 0 and <= Constants.Map.MaxSize ? size : 0, bytes[nameLengthAt - 1] == 1);
        }

        private static string ReadText(byte[] bytes, int start, int length) => Encoding.ASCII.GetString(bytes, start, length);

        private static int ReadLength(byte[] bytes, int start) => bytes[start] | (bytes[start + 1] << 8);

        private static bool IsTextAt(byte[] bytes, int start, int length)
        {
            if (start + length > bytes.Length)
            {
                return false;
            }

            for (var i = start; i < start + length; i++)
            {
                if (bytes[i] is not ((>= 0x20 and <= 0x7E) or (byte)'\r' or (byte)'\n'))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
