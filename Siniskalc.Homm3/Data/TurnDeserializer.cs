namespace Siniskalc.Homm3.Data
{
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads whose turn it is: the 32-bit colour after the human flags. A value that is no colour reads as <see cref="PlayerColor.Neutral"/>.
    /// </summary>
    public sealed class TurnDeserializer : IDeserializer<RecordChunk, TurnModel>
    {
        private static readonly ILogger<TurnDeserializer> Logger = LogFactory.For<TurnDeserializer>();

        /// <inheritdoc/>
        public TurnModel Deserialize(RecordChunk chunk)
        {
            var value = chunk.GetInt32At(TurnLayout.CurrentPlayer);
            var color = value is >= 0 and < Constants.Player.ColorCount ? PlayerColor.FromByte((byte)value) : PlayerColor.Neutral;
            if (color.IsNeutral)
            {
                Logger.LogTrace("Current player {Value:X2} is no colour, read as neutral and kept", value);
            }

            return new TurnModel(chunk.Index, color);
        }
    }
}
