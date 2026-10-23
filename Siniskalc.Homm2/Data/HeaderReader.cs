namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Reads the header of a savegame. Nothing in it is written back: the date and the map are only shown.
    /// </summary>
    public sealed class HeaderReader(ILogger<HeaderReader> logger) : IHeaderReader
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
                this.ReadDifficulty(BitConverter.ToUInt16(bytes, layout.MapDifficultyStart)),
                layout.Width);
            return new SaveMetadata(date, map, this.ReadDifficulty(bytes[layout.DifficultyStart]), this.ReadControls(bytes, layout));
        }

        // The game plays the humans the mask marks, in the order of the records. Whose turn it is is the turn record (Game.CurrentPlayer).
        private List<PlayerType> ReadControls(byte[] bytes, SaveLayout layout)
        {
            var colors = PlayerRecordOrder.ColorsOf(bytes, layout, logger);
            var controls = Enumerable.Repeat(PlayerType.NotInGame, Constants.Table.PlayerCount).ToList();
            for (var record = 0; record < Constants.Table.PlayerCount; record++)
            {
                if (bytes[layout.ColorsInGameStart + colors[record]] != 0)
                {
                    controls[colors[record]] = bytes[layout.HumanMaskStart + record] != 0 ? PlayerType.Human : PlayerType.Computer;
                }
            }

            return controls;
        }

        private GameDifficulty ReadDifficulty(int code)
        {
            if (code >= Difficulties.Length)
            {
                logger.LogTrace("Difficulty {Code} is no level, read as the first one", code);
            }

            return code < Difficulties.Length ? Difficulties[code] : Difficulties[0];
        }
    }
}
