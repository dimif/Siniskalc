namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
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
            var (controls, current) = ReadPlayers(bytes, layout);
            return new SaveMetadata(date, map, ReadDifficulty(bytes[layout.DifficultyStart]), current, controls);
        }

        // The game plays the humans the mask marks, in the order of the records, and the byte of the turn is the number of the record.
        private static (List<PlayerType> Controls, Color? Current) ReadPlayers(byte[] bytes, SaveLayout layout)
        {
            var colors = PlayerRecordOrder.ColorsOf(bytes, layout);
            var controls = Enumerable.Repeat(PlayerType.NotInGame, Constants.Table.PlayerCount).ToList();
            for (var record = 0; record < Constants.Table.PlayerCount; record++)
            {
                if (bytes[layout.ColorsInGameStart + colors[record]] != 0)
                {
                    controls[colors[record]] = bytes[layout.HumanMaskStart + record] != 0 ? PlayerType.Human : PlayerType.Computer;
                }
            }

            var turn = bytes[layout.CurrentHumanStart];
            Color? current = turn < Constants.Table.PlayerCount && bytes[layout.HumanMaskStart + turn] != 0 ? (Color)colors[turn] : null;
            return (controls, current);
        }

        // TODO:LOG warning: a difficulty that is no level was read as the first one.
        private static GameDifficulty ReadDifficulty(int code) => code < Difficulties.Length ? Difficulties[code] : Difficulties[0];
    }
}
