namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes the lists of heroes and castles of a player, and what is selected, into the record of the player, as they are given. A list
    /// whose bytes already read as the model says is left alone, with its count, and so is a selection that does.
    /// </summary>
    public sealed class SafePlayerListsSerializer : ISerializer<PlayerChunk, PlayerLists>
    {
        /// <inheritdoc/>
        public PlayerChunk Serialize(PlayerChunk chunk, PlayerLists model)
        {
            var bytes = chunk.ToArray();

            WriteIds(bytes, PlayerLayout.Heroes, Constants.Player.HeroListSize, model.HeroIds, PlayerLayout.HeroCount);
            WriteIds(bytes, PlayerLayout.Castles, Constants.Player.CastleListSize, model.CastleIds, PlayerLayout.CastleCount);
            bytes[PlayerLayout.SelectedHero] = Bytes.IdByte(model.SelectedHero, bytes[PlayerLayout.SelectedHero]);
            bytes[PlayerLayout.SelectedCastle] = Bytes.IdByte(model.SelectedCastle, bytes[PlayerLayout.SelectedCastle]);

            return chunk.WithBytes(bytes);
        }

        // The list and its count are written together, and only when the list in the savegame is not what the model says.
        private static void WriteIds(byte[] bytes, int offset, int size, Numbers ids, int countOffset)
        {
            var current = bytes.AsSpan(offset, size).ToArray();
            var written = Bytes.IdListBytes(ids, size, current);
            if (ReferenceEquals(written, current))
            {
                return;
            }

            written.CopyTo(bytes, offset);
            bytes[countOffset] = (byte)ids.Count;
        }
    }
}
