namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Reads the header of a savegame. Nothing in it is written back: the date and the map are only shown.
    /// </summary>
    public sealed class HeaderReader : IHeaderReader
    {
        private static readonly ILogger<HeaderReader> Logger = LogFactory.For<HeaderReader>();

        // The levels in the order of the game's own numbers: easy, normal, hard, expert, impossible (a map has only the first four).
        private static readonly GameDifficulty[] Difficulties = [GameDifficulty.Pawn, GameDifficulty.Knight, GameDifficulty.Rook, GameDifficulty.Queen, GameDifficulty.King];

        /// <inheritdoc/>
        public SaveMetadata<PlayerColor> Read(byte[] bytes, SaveLayout layout)
        {
            Logger.LogDebug("Reading the header of {Length} bytes, the date at {Offset:X}", bytes.Length, layout.DateStart);

            var date = new GameDate(
                bytes[layout.DateStart + (2 * HeaderLayout.DateStep)],
                bytes[layout.DateStart + HeaderLayout.DateStep],
                bytes[layout.DateStart]);
            Logger.LogTrace("Read the date {Date}", date);

            Logger.LogTrace("Reading the map name at {Offset:X} and its description at {DescriptionOffset:X}", layout.MapNameStart, layout.MapDescriptionStart);
            var map = new MapHeader(
                Bytes.ReadText(bytes, layout.MapNameStart, HeaderLayout.MapNameLength),
                Bytes.ReadText(bytes, layout.MapDescriptionStart, HeaderLayout.MapDescriptionMaxLength),
                ReadDifficulty(BitConverter.ToUInt16(bytes, layout.MapDifficultyStart)),
                layout.Width);
            Logger.LogTrace("Read the map {Name}, {Size} tiles wide, with the difficulty {Difficulty}", map.Name, map.Size, map.Difficulty);

            var setup = new GameSetup<PlayerColor>(ReadControls(bytes, layout), ReadDifficulty(bytes[layout.DifficultyStart]));
            Logger.LogTrace("Read the game setup, played at {Difficulty}", setup.Difficulty);
            return new SaveMetadata<PlayerColor>(date, map, setup);
        }

        // The game plays the humans the mask marks, in the order of the records. Whose turn it is is the turn record (Game.CurrentPlayer).
        private static List<PlayerType> ReadControls(byte[] bytes, SaveLayout layout)
        {
            Logger.LogTrace("Reading who controls each colour, the in-game flags at {FlagsOffset:X} and the human mask at {MaskOffset:X}", layout.ColorsInGameStart, layout.HumanMaskStart);
            var colors = PlayerRecordOrder.ColorsOf(bytes, layout);
            var controls = Enumerable.Repeat(PlayerType.NotInGame, Constants.Table.PlayerCount).ToList();
            for (var record = 0; record < Constants.Table.PlayerCount; record++)
            {
                if (bytes[layout.ColorsInGameStart + colors[record]] != 0)
                {
                    controls[colors[record]] = bytes[layout.HumanMaskStart + record] != 0 ? PlayerType.Human : PlayerType.Computer;
                }

                Logger.LogTrace("Record {Slot} is colour {Colour} and is {Control}", record, colors[record], controls[colors[record]]);
            }

            return controls;
        }

        private static GameDifficulty ReadDifficulty(int code)
        {
            if (code >= Difficulties.Length)
            {
                Logger.LogTrace("Difficulty {Code:X2} is no level, read as the first one", code);
            }

            var difficulty = code < Difficulties.Length ? Difficulties[code] : Difficulties[0];
            Logger.LogTrace("Difficulty code {Code:X2} read as {Difficulty}", code, difficulty);
            return difficulty;
        }
    }
}
