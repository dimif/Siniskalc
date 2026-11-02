namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes whose turn it is: the 32-bit colour after the human flags. A value that is no colour is left alone while the model says Neutral, as
    /// it reads. The turn can only go to a colour whose human flag is set: the game gives the turn to humans only, and the setup of a savefile
    /// whose current player is no human is not found again (see <see cref="PlayerControlLocator"/>).
    /// </summary>
    public sealed class SafeTurnSerializer : ISerializer<RecordChunk, TurnModel>
    {
        /// <inheritdoc/>
        public RecordChunk Serialize(RecordChunk chunk, TurnModel model)
        {
            var stored = chunk.GetInt32At(TurnLayout.CurrentPlayer);
            var storedColor = stored is >= 0 and < Constants.Player.ColorCount ? PlayerColor.FromByte((byte)stored) : PlayerColor.Neutral;
            if (storedColor == model.Color)
            {
                return chunk;
            }

            RangeGuard.ThrowIfOutOfRange(model.Color.Code, 0, Constants.Player.ColorCount - 1, nameof(model), "The turn can only be given to the colour of a player.");
            if (chunk.GetInt32At(model.Color.Code * TurnLayout.HumanFlagSize) != 1)
            {
                throw new GameRuleException($"{model.Color} is not played by a human, so it cannot be given the turn.");
            }

            var bytes = chunk.ToArray();
            ByteBufferHelper.WriteInt32(bytes, TurnLayout.CurrentPlayer, model.Color.Code);
            return chunk.WithBytes(bytes);
        }
    }
}
