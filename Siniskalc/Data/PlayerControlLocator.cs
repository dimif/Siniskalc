namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Finds which colours are played by a human, and the difficulty, in a savefile.
    /// </summary>
    /// <remarks>
    /// The savefile header has one byte for each colour, in colour order, just before the difficulty and the file name of the
    /// map: 0xFF when the map has no such player, 0x0A for a computer player and anything else for a human. What the other
    /// value means is not known: it was 0 for red and 2 for tan in the two saves of one map where either was the human
    /// (human_red and human_tan), and 0 for purple in a save of another map. The header's position varies with the strings
    /// before it, so the bytes are found by their shape: eight such bytes, a difficulty from 0 to 4, then the map's file name.
    /// That header only names the first human of a hotseat game, so who is human is read from the 32-bit flags further on: eight
    /// of them, in colour order, 1 for a human, followed by the colour whose turn it is (so that is exposed too) and then a marker: FF 10 eight bytes later in
    /// Horn of the Abyss (found by comparing saves of one map with red, tan and both as humans: human_red, human_tan, human_red_tan), FF 00 00 00 right
    /// after it in Shadow of Death and Restoration of Erathia (found in the hotseat saves with Red and Blue human, and in every single player save).
    /// The difficulty byte follows the colours directly: 0 is Pawn to 4 for King. Only a Queen (3) game has been checked against
    /// what the game showed; the other saves are all at 3 as well, so the order of the other levels is assumed.
    /// </remarks>
    public sealed class PlayerControlLocator : IPlayerControlLocator
    {
        internal const int HumanFlagSize = 4;
        internal const int HumanFlagsLength = Constants.Player.ColorCount * HumanFlagSize;
        internal const int HotaHumanFlagsToMarker = HumanFlagsLength + 8;
        internal const int ClassicHumanFlagsToMarker = HumanFlagsLength + 4;

        private const byte NotInGameByte = 0xFF;
        private const byte ComputerByte = 0x0A;

        private const int MaxFileNameLength = 130;

        private const int HumanFlagsSearchLength = 4000;

        // The byte is the level, from the easiest. The enum's values are the percentages the game scales the difficulty by.
        private static readonly GameDifficulty[] Difficulties = [GameDifficulty.Pawn, GameDifficulty.Knight, GameDifficulty.Rook, GameDifficulty.Queen, GameDifficulty.King];

        /// <summary>
        /// Returns who plays each colour, indexed by <see cref="PlayerColor"/>, or <c>null</c> when it cannot be found.
        /// </summary>
        public IReadOnlyList<PlayerType>? Find(byte[] bytes, GameVersion gameVersion) => this.FindSetup(bytes, gameVersion)?.PlayerControls;

        /// <summary>
        /// Returns who plays each colour and the difficulty, or <c>null</c> when they cannot be found.
        /// </summary>
        public GameSetup? FindSetup(byte[] bytes, GameVersion gameVersion)
        {
            var location = this.Locate(bytes, gameVersion);
            if (location == null)
            {
                return null;
            }

            var i = location.ControlsAt;
            var humanFlags = location.HumanFlagsAt;
            var result = new PlayerType[Constants.Player.ColorCount];
            for (var color = 0; color < Constants.Player.ColorCount; color++)
            {
                var value = bytes[i + color];
                result[color] = value == NotInGameByte ? PlayerType.NotInGame
                    : humanFlags >= 0 ? (BitConverter.ToInt32(bytes, humanFlags + (color * HumanFlagSize)) == 1 ? PlayerType.Human : PlayerType.Computer)
                    : value == ComputerByte ? PlayerType.Computer : PlayerType.Human;
            }

            PlayerColor? current = humanFlags >= 0 ? (PlayerColor)BitConverter.ToInt32(bytes, humanFlags + HumanFlagsLength) : null;
            return new GameSetup(result, Difficulties[bytes[location.DifficultyAt]], current);
        }

        /// <summary>
        /// Returns where the setup is, or <c>null</c> when it cannot be found.
        /// </summary>
        public PlayerSetupLocation? Locate(byte[] bytes, GameVersion gameVersion)
        {
            if (!gameVersion.HasGameSetup())
            {
                return null;
            }

            for (var i = 0; i + Constants.Player.ColorCount + 2 <= bytes.Length; i++)
            {
                if (IsPlayerControlAt(bytes, i) && bytes[i + Constants.Player.ColorCount] <= Constants.Map.MaxDifficulty)
                {
                    var fileNameAt = i + Constants.Player.ColorCount + 1;
                    var fileNameEnd = FindMapFileNameEnd(bytes, fileNameAt);
                    if (fileNameEnd >= 0)
                    {
                        var (flagsAt, toMarker) = FindHumanFlags(bytes, fileNameAt);
                        return new PlayerSetupLocation(i, fileNameAt, fileNameEnd - fileNameAt, flagsAt, toMarker);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns where the human flags are (or -1 when they are not found) and how far the marker after them is. Horn of the Abyss has FF 10 eight
        /// bytes after the current player; Shadow of Death and Restoration of Erathia have FF 00 00 00 right after it (hotseat saves of Shadow of Death).
        /// </summary>
        private static (int At, int ToMarker) FindHumanFlags(byte[] bytes, int searchFrom)
        {
            var last = Math.Min(bytes.Length - HotaHumanFlagsToMarker - 2, searchFrom + HumanFlagsSearchLength);
            for (var i = searchFrom; i <= last; i++)
            {
                var hota = bytes[i + HotaHumanFlagsToMarker] == 0xFF && bytes[i + HotaHumanFlagsToMarker + 1] == 0x10;
                var classic = bytes[i + ClassicHumanFlagsToMarker] == 0xFF && bytes[i + ClassicHumanFlagsToMarker + 1] == 0
                    && bytes[i + ClassicHumanFlagsToMarker + 2] == 0 && bytes[i + ClassicHumanFlagsToMarker + 3] == 0;
                if (!hota && !classic)
                {
                    continue;
                }

                var humans = 0;
                var valid = true;
                for (var color = 0; color < Constants.Player.ColorCount && valid; color++)
                {
                    var flag = BitConverter.ToInt32(bytes, i + (color * HumanFlagSize));
                    valid = flag is 0 or 1;
                    humans += flag;
                }

                // The colour whose turn it is must be a human.
                var current = BitConverter.ToInt32(bytes, i + HumanFlagsLength);
                if (valid && humans > 0 && current is >= 0 and < Constants.Player.ColorCount && BitConverter.ToInt32(bytes, i + (current * HumanFlagSize)) == 1)
                {
                    return (i, hota ? HotaHumanFlagsToMarker : ClassicHumanFlagsToMarker);
                }
            }

            return (-1, HotaHumanFlagsToMarker);
        }

        private static bool IsPlayerControlAt(byte[] bytes, int start)
        {
            var anyPlayer = false;
            for (var color = 0; color < Constants.Player.ColorCount; color++)
            {
                var value = bytes[start + color];
                if (value is not (NotInGameByte or ComputerByte or < Constants.Player.ColorCount))
                {
                    return false;
                }

                anyPlayer |= value != NotInGameByte;
            }

            return anyPlayer;
        }

        /// <summary>
        /// Returns the offset just past the extension of the map's file name when printable text ending in it follows, otherwise -1.
        /// </summary>
        private static int FindMapFileNameEnd(byte[] bytes, int start)
        {
            var end = Math.Min(bytes.Length, start + MaxFileNameLength);
            for (var i = start; i < end && bytes[i] is >= 0x20 and <= 0x7E; i++)
            {
                if (bytes[i] == (byte)'.' && i + 3 < bytes.Length && bytes[i + 1] == (byte)'h' && bytes[i + 2] == (byte)'3' && bytes[i + 3] is (byte)'m' or (byte)'c')
                {
                    return i + 4;
                }
            }

            return -1;
        }
    }
}
