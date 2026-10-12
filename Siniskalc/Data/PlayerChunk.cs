namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;

    /// <summary>
    /// The chunk of a player, named after the player's colour.
    /// </summary>
    public sealed class PlayerChunk : BytesChunk
    {
        public PlayerChunk(PlayerColor color, byte[] bytes, int startIndex, PlayerLayout layout)
            : base(color.ToString(), bytes, startIndex)
        {
            this.Color = color;
            this.Layout = layout;
        }

        /// <summary>
        /// Gets the colour the struct belongs to: the one the loader found it at in the table of players (a struct that is not in play has no colour byte of its own).
        /// </summary>
        public PlayerColor Color { get; }

        /// <summary>
        /// Gets where the fields of the player are in the chunk, which depends on the version of the savefile.
        /// </summary>
        public PlayerLayout Layout { get; }

        public override PlayerChunk WithBytes(byte[] bytes)
        {
            return new PlayerChunk(this.Color, this.CheckReplacement(bytes), this.StartIndex, this.Layout);
        }
    }
}
