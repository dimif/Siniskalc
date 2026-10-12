namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a player model into the record of a player, as it is given. A field whose bytes already read as the model says is left alone,
    /// and so is everything the model does not hold.
    /// </summary>
    public sealed class SafeHomm2PlayerSerializer : IHomm2PlayerSerializer
    {
        /// <inheritdoc/>
        public Homm2PlayerChunk Serialize(Homm2PlayerChunk chunk, Homm2PlayerModel model)
        {
            var bytes = chunk.ToArray();

            foreach (var resource in Enum.GetValues<Homm2Resource>())
            {
                WriteAmount(bytes, Homm2PlayerLayout.Resources + ((int)resource * 4), model.Resources[resource], Constants.Player.MinResource, Constants.Player.MaxResource, nameof(model.Resources));
                if (resource != Homm2Resource.Gold)
                {
                    WriteAmount(bytes, Homm2PlayerLayout.Income + ((int)resource * 4), model.Income[resource], Constants.Player.MinResource, Constants.Player.MaxResource, nameof(model.Income));
                }
            }

            WriteAmount(bytes, Homm2PlayerLayout.GoldIncome, model.Income.Gold, Constants.Player.MinResource, Constants.Player.MaxResource, nameof(model.Income));

            WriteIds(bytes, Homm2PlayerLayout.Heroes, Constants.Player.HeroListSize, model.HeroIds, Homm2PlayerLayout.HeroCount);
            WriteIds(bytes, Homm2PlayerLayout.Castles, Constants.Player.CastleListSize, model.CastleIds, Homm2PlayerLayout.CastleCount);
            bytes[Homm2PlayerLayout.SelectedHero] = Homm2Bytes.IdByte(model.SelectedHero, bytes[Homm2PlayerLayout.SelectedHero]);
            bytes[Homm2PlayerLayout.SelectedCastle] = Homm2Bytes.IdByte(model.SelectedCastle, bytes[Homm2PlayerLayout.SelectedCastle]);
            bytes[Homm2PlayerLayout.DaysLeft] = Homm2Bytes.IdByte(model.DaysLeft, bytes[Homm2PlayerLayout.DaysLeft]);
            bytes[Homm2PlayerLayout.Tavern] = Homm2Bytes.IdByte(model.Tavern.First, bytes[Homm2PlayerLayout.Tavern]);
            bytes[Homm2PlayerLayout.Tavern + 1] = Homm2Bytes.IdByte(model.Tavern.Second, bytes[Homm2PlayerLayout.Tavern + 1]);

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
        private static void WriteIds(byte[] bytes, int offset, int size, Homm2Numbers ids, int countOffset)
        {
            var current = bytes.AsSpan(offset, size).ToArray();
            var written = Homm2Bytes.IdListBytes(ids, size, current);
            if (ReferenceEquals(written, current))
            {
                return;
            }

            written.CopyTo(bytes, offset);
            bytes[countOffset] = (byte)ids.Count;
        }
    }
}
