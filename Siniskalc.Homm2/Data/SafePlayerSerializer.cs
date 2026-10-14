namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a player model into the record of a player, as it is given. A field whose bytes already read as the model says is left alone,
    /// and so is everything the model does not hold.
    /// </summary>
    public sealed class SafePlayerSerializer : IPlayerSerializer
    {
        /// <inheritdoc/>
        public PlayerChunk Serialize(PlayerChunk chunk, PlayerModel model)
        {
            var bytes = chunk.ToArray();

            foreach (var resource in Enum.GetValues<Resource>())
            {
                WriteAmount(bytes, PlayerLayout.Resources + ((int)resource * 4), model.Resources[resource], Constants.Player.MinResource, Constants.Player.MaxResource, nameof(model.Resources));
                if (resource != Resource.Gold)
                {
                    WriteAmount(bytes, PlayerLayout.Income + ((int)resource * 4), model.Income[resource], Constants.Player.MinResource, Constants.Player.MaxResource, nameof(model.Income));
                }
            }

            WriteAmount(bytes, PlayerLayout.GoldIncome, model.Income.Gold, Constants.Player.MinResource, Constants.Player.MaxResource, nameof(model.Income));

            WriteIds(bytes, PlayerLayout.Heroes, Constants.Player.HeroListSize, model.HeroIds, PlayerLayout.HeroCount);
            WriteIds(bytes, PlayerLayout.Castles, Constants.Player.CastleListSize, model.CastleIds, PlayerLayout.CastleCount);
            bytes[PlayerLayout.SelectedHero] = Bytes.IdByte(model.SelectedHero, bytes[PlayerLayout.SelectedHero]);
            bytes[PlayerLayout.SelectedCastle] = Bytes.IdByte(model.SelectedCastle, bytes[PlayerLayout.SelectedCastle]);
            bytes[PlayerLayout.DaysLeft] = Bytes.IdByte(model.DaysLeft, bytes[PlayerLayout.DaysLeft]);
            bytes[PlayerLayout.Tavern] = Bytes.IdByte(model.Tavern.First, bytes[PlayerLayout.Tavern]);
            bytes[PlayerLayout.Tavern + 1] = Bytes.IdByte(model.Tavern.Second, bytes[PlayerLayout.Tavern + 1]);

            return chunk.WithBytes(bytes);
        }

        private static void WriteAmount(byte[] bytes, int offset, long amount, long min, long max, string name)
        {
            if (BitConverter.ToUInt32(bytes, offset) == amount)
            {
                return;
            }

            RangeGuard.ThrowIfOutOfRange(amount, min, max, name);
            ByteBufferHelper.WriteUInt32(bytes, offset, (uint)amount);
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
