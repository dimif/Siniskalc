namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a player model into the record of a player, as it is given. A field whose bytes already read as the model says is left alone,
    /// and so is everything the model does not hold (the lists of heroes and castles are written by <see cref="SafePlayerListsSerializer"/>).
    /// </summary>
    public sealed class SafePlayerSerializer : ISerializer<PlayerChunk, PlayerModel>
    {
        /// <inheritdoc/>
        public PlayerChunk Serialize(PlayerChunk chunk, PlayerModel model)
        {
            var bytes = chunk.ToArray();

            foreach (var resource in Enum.GetValues<Resource>())
            {
                WriteAmount(bytes, PlayerLayout.Resources + ((int)resource * 4), model.Resources[resource], Heroes.Constants.Resources.Min, Constants.Player.MaxResource, nameof(model.Resources));
                if (resource != Resource.Gold)
                {
                    WriteAmount(bytes, PlayerLayout.Income + ((int)resource * 4), model.Income[resource], Heroes.Constants.Resources.Min, Constants.Player.MaxResource, nameof(model.Income));
                }
            }

            WriteAmount(bytes, PlayerLayout.GoldIncome, model.Income.Gold, Heroes.Constants.Resources.Min, Constants.Player.MaxResource, nameof(model.Income));

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
    }
}
