namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// The raw bytes of one hero, edited through that hero's <see cref="HeroEditSession"/>.
    /// </summary>
    public sealed class HeroByteEditTarget : ChunkByteEditTarget<HeroChunk, HeroModel>
    {
        public HeroByteEditTarget(HeroEditSession session, IHeroRegionProvider regions)
            : base(
                session,
                regions.GetRegions(session.Offsets, session.Chunk.Length),
                () => session.Current.Name)
        {
        }
    }
}
