namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The raw bytes of one player, edited through that player's <see cref="PlayerEditSession"/>.
    /// </summary>
    public sealed class PlayerByteEditTarget : ChunkByteEditTarget<PlayerChunk, PlayerModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerByteEditTarget"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="regions">The regions.</param>
        public PlayerByteEditTarget(PlayerEditSession session, IPlayerRegionProvider regions)
            : base(
                session,
                regions.GetRegions(session.Chunk.Layout, session.Chunk.Length),
                () => session.Current.Color.ToString())
        {
        }
    }
}
