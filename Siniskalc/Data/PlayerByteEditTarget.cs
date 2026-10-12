namespace Siniskalc.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// The raw bytes of one player, edited through that player's <see cref="PlayerEditSession"/>.
    /// </summary>
    public sealed class PlayerByteEditTarget : ChunkByteEditTarget<PlayerChunk, PlayerModel>
    {
        public PlayerByteEditTarget(PlayerEditSession session, IPlayerRegionProvider regions)
            : base(
                session,
                regions.GetRegions(session.Chunk.Layout, session.Chunk.Length),
                () => session.Current.Color.ToString())
        {
        }
    }
}
