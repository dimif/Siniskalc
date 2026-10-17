namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Reads the header of a savegame. Nothing in it is written back: the date and the map are only shown.
    /// </summary>
    public sealed class HeaderReader : IHeaderReader
    {
        // The levels in the order of the game's own numbers: easy, normal, hard, expert, impossible (a map has only the first four).
        private static readonly GameDifficulty[] Difficulties = [GameDifficulty.Pawn, GameDifficulty.Knight, GameDifficulty.Rook, GameDifficulty.Queen, GameDifficulty.King];

        /// <inheritdoc/>
        public SaveMetadata Read(byte[] bytes, SaveLayout layout)
        {
            var date = new GameDate(
                bytes[layout.DateStart + (2 * HeaderLayout.DateStep)],
                bytes[layout.DateStart + HeaderLayout.DateStep],
                bytes[layout.DateStart]);
            var map = new MapHeader(
                Bytes.ReadText(bytes, layout.MapNameStart, HeaderLayout.MapNameLength),
                Bytes.ReadText(bytes, layout.MapDescriptionStart, HeaderLayout.MapDescriptionMaxLength),
                ReadDifficulty(BitConverter.ToUInt16(bytes, layout.MapDifficultyStart)),
                layout.Width);
            return new SaveMetadata(date, map, ReadDifficulty(bytes[layout.DifficultyStart]));
        }

        // TODO:LOG warning: a difficulty that is no level was read as the first one.
        private static GameDifficulty ReadDifficulty(int code) => code < Difficulties.Length ? Difficulties[code] : Difficulties[0];
    }
}
